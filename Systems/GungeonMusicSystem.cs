using Microsoft.Xna.Framework;
using ReLogic.Utilities;
using Terraria.Audio;
using Terraria.ModLoader;

namespace CalamityMod.Systems
{
    public class GungeonMusicSystem : ModSystem
    {
        // Copied straight from the ORDER easter egg

        private static readonly SoundStyle GungeonTrack = new("CalamityMod/Sounds/Custom/GungeonCreditMusic", SoundType.Music);
        private static SlotId gungeonSoundSlot;

        internal static float DefaultMusicTime = 100f;
        internal static float DefaultResetTime = 240f;

        internal static float remainingPlaytime = 0f;
        internal static float timeUntilReset = 0f;

        private static bool currentlyPlaying = false;

        public override void UpdateUI(GameTime gameTime)
        {
            // Decrement timers
            if (remainingPlaytime > 0)
                --remainingPlaytime;
            if (timeUntilReset > 0)
                --timeUntilReset;

            if (currentlyPlaying)
            {
                // If the reset timer has run out, stop the active sound instance entirely.
                // The next bullet hit will start the song from the beginning.
                if (timeUntilReset <= 0f)
                {
                    currentlyPlaying = false;
                    if (SoundEngine.TryGetActiveSound(gungeonSoundSlot, out var activeSound))
                        activeSound.Stop();
                    gungeonSoundSlot = SlotId.Invalid;
                }

                // Otherwise, set the volume of the active sound instance appropriately.
                else
                {
                    bool foundTheGun = SoundEngine.TryGetActiveSound(gungeonSoundSlot, out var activeSound);
                    if (!foundTheGun)
                    {
                        currentlyPlaying = false;
                        return;
                    }

                    float newVolume = MathHelper.Clamp(remainingPlaytime / DefaultMusicTime, 0f, 1f);
                    activeSound.Volume = newVolume;
                    activeSound.Update();
                }
            }
        }

        // EEEEEEEEEEEEEEEEEEEEEEENTER THE GUNGEON, ENTER THE GUNGEON
        public static void GUN()
        {
            if (!currentlyPlaying)
                gungeonSoundSlot = SoundEngine.PlaySound(GungeonTrack);

            currentlyPlaying = true;
            remainingPlaytime = DefaultMusicTime + 30;
            timeUntilReset = DefaultResetTime;
        }
    }
}
