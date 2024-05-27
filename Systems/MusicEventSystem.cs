using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace CalamityMod.Systems
{
    public record class MusicEventEntry(string Id, int Song, TimeSpan Length, TimeSpan IntroSilence, Func<bool> ShouldPlay);

    public class MusicEventSystem : ModSystem
    {
        #region Statics

        public static MusicEventEntry CurrentEvent { get; set; } = null;

        public static DateTime? TrackStart { get; set; } = null;

        // This allows for some additional silence at the end of a track
        public static DateTime? TrackEnd { get; set; } = null;

        public static int LastPlayedEvent { get; set; } = -1;

        public static float VolumeCache { get; set; } = -1f;

        public static Thread EventTrackerThread { get; set; } = null;

        public static List<string> PlayedEvents { get; set; } = [];

        public static List<MusicEventEntry> EventCollection { get; set; } = [];

        #endregion

        #region Events List

        public override void OnModLoad()
        {
            static void AddEntry(string eventId, string songName, TimeSpan introSilence, TimeSpan length, Func<bool> shouldPlay)
            {
                MusicEventEntry entry = new(eventId, MusicLoader.GetMusicSlot(CalamityMod.Instance.musicMod, songName), introSilence, length, shouldPlay);
                EventCollection.Add(entry);
            }

            AddEntry("CloneDefeated", "Interlude1", TimeSpan.FromSeconds(5), TimeSpan.FromSeconds(216.287d), () => DownedBossSystem.downedCalamitasClone);
            AddEntry("MLDefeated", "Interlude2", TimeSpan.Zero, TimeSpan.FromSeconds(160.989), () => NPC.downedMoonlord);
            AddEntry("YharonDefeated", "Interlude3", TimeSpan.FromSeconds(4), TimeSpan.FromSeconds(295.932), () => DownedBossSystem.downedYharon);

            AddEntry("DoGDefeated", "DevourerofGodsEulogy", TimeSpan.FromSeconds(3), TimeSpan.FromSeconds(203.62d), () => DownedBossSystem.downedDoG);
        }

        public override void Unload() => EventCollection.Clear();

        #endregion

        #region Event Handling

        public override void PostUpdateTime()
        {
            // If the event has just finished, we want a little silence before fading back to normal
            if (TrackEnd is not null)
            {
                // `silence` is the time after a track ends before music goes back to normal
                TimeSpan silence = TimeSpan.FromSeconds(3);
                TimeSpan postTrack = DateTime.Now - TrackEnd.Value;

                // Continue "playing" the track for the amount of silent time specified
                if (postTrack < silence)
                    Main.musicBox2 = LastPlayedEvent;

                else
                {
                    LastPlayedEvent = -1;
                    TrackEnd = null;
                }

                // But in reality set the volume to 0, so it stays silent
                Main.musicFade[Main.curMusic] = 0f;

                return;
            }

            // Only check for new events to play if none is currently playing
            // This makes sure events always finish before a new one starts
            if (CurrentEvent is null)
            {
                foreach (MusicEventEntry musicEvent in EventCollection)
                {
                    // Make sure the event hasn't already played and SHOULD play
                    if (!PlayedEvents.Contains(musicEvent.Id) && musicEvent.ShouldPlay())
                    {
                        // Assign the current event and start time
                        CurrentEvent = musicEvent;
                        TrackStart = DateTime.Now + musicEvent.IntroSilence;

                        // On clients, use a background thread to make sure the track always plays for exactly
                        // the specified length, regardless of if the game gets minimized, lags, or time becomes
                        // detangled from a consistent 60fps in any other way
                        if (!Main.dedServ)
                        {
                            EventTrackerThread = new(WatchMusicEvent);
                            EventTrackerThread.Start();
                        }

                        break;
                    }
                }
            }

            if (TrackStart is not null)
            {
                if (TrackStart > DateTime.Now)
                {
                    int silenceSlot = MusicLoader.GetMusicSlot(Mod, "Sounds/Music/Silence");
                    Main.musicBox2 = silenceSlot;

                    VolumeCache = Main.musicVolume;
                }

                else
                {
                    Main.musicBox2 = CurrentEvent.Song;

                    // This sets the music volume to 0 for one frame, then back to normal
                    // Effectively, it immediately starts the music without fading
                    // This only happens if there is intended silence before the song starts
                    if (VolumeCache != -1f)
                    {
                        if (Main.musicVolume != 0f)
                            Main.musicVolume = 0f;

                        else
                        {
                            Main.musicVolume = VolumeCache;
                            VolumeCache = -1f;
                        }
                    }
                }

                // If the event has finished playing, mark the end as now and clear the current event
                if (DateTime.Now - TrackStart > CurrentEvent.Length)
                {
                    PlayedEvents.Add(CurrentEvent.Id);

                    TrackEnd = DateTime.Now;
                    LastPlayedEvent = CurrentEvent.Song;

                    TrackStart = null;
                    CurrentEvent = null;
                }
            }
        }

        /// <summary>
        /// Watches for the game minimizing at any point, and adjusts the amount of time to play the song for accordingly
        /// </summary>
        public static void WatchMusicEvent()
        {
            DateTime? minimized = null;

            int currentEventSong;
            while ((currentEventSong = CurrentEvent?.Song ?? -1) != -1)
            {
                bool musicPaused = Main.audioSystem.IsTrackPlaying(currentEventSong) && Main.musicVolume != 0f;
                
                if (musicPaused && !minimized.HasValue)
                    minimized = DateTime.Now;

                else if (!musicPaused && minimized.HasValue)
                {
                    TrackStart = TrackStart.Value + (DateTime.Now - minimized.Value);
                    minimized = null;
                }
            }

            EventTrackerThread = null;
        }

        #endregion

        #region Event Saving

        public override void SaveWorldData(TagCompound tag)
        {
            tag["calamityPlayedMusicEventCount"] = PlayedEvents.Count;
            for (int i = 0; i < PlayedEvents.Count; i++)
                tag[$"calamityPlayedMusicEvent{i}"] = PlayedEvents[i];
        }

        public override void LoadWorldData(TagCompound tag)
        {
            PlayedEvents.Clear();

            if (tag.TryGet("calamityPlayedMusicEventCount", out int playedMusicEventCount))
            {
                for (int i = 0; i < playedMusicEventCount; i++)
                {
                    if (tag.TryGet($"calamityPlayedMusicEvent{i}", out string playedEvent))
                        PlayedEvents.Add(playedEvent);
                }
            }
        }

        #endregion

        #region Event Syncing

        public static void SendSyncRequest()
        {
            ModPacket packet = CalamityMod.Instance.GetPacket();
            packet.Write((byte)CalamityModMessageType.MusicEventSyncRequest);
            packet.Send();
        }

        public static void FulfillSyncRequest(int requester)
        {
            // Only fulfill requests as the server host
            if (!Main.dedServ)
                return;

            ModPacket packet = CalamityMod.Instance.GetPacket();
            packet.Write((byte)CalamityModMessageType.MusicEventSyncResponse);

            int trackCount = PlayedEvents.Count;
            packet.Write(trackCount);

            for (int i = 0; i < trackCount; i++)
                packet.Write(PlayedEvents[i]);

            packet.Send(toClient: requester);
        }

        public static void ReceiveSyncResponse(BinaryReader reader)
        {
            // Only receive info on clients
            if (Main.dedServ)
                return;

            PlayedEvents.Clear();
            int trackCount = reader.ReadInt32();

            for (int i = 0; i < trackCount; i++)
                PlayedEvents.Add(reader.ReadString());
        }

        #endregion
    }

    public class CalamityModMusicEventPlayer : ModPlayer
    {
        public override void OnEnterWorld()
        {
            if (Main.netMode == NetmodeID.MultiplayerClient && Player.whoAmI != Main.myPlayer)
                MusicEventSystem.SendSyncRequest();
        }
    }
}
