using System.Linq;
using CalamityMod.NPCs.AcidRain;
using CalamityMod.NPCs.Astral;
using CalamityMod.NPCs.NormalNPCs;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.NPCs
{
    public sealed class CalamityNaturalSpawnBossNPC : GlobalNPC
    {
        public override bool InstancePerEntity => false;

        public static int ghostKillCount = 0;
        public static int sharkKillCount = 0;

        private static int[] _PolterghastTriggerNPCS;
        private static int[] _GreatSharkTriggerNPCS;

        public override void SetStaticDefaults()
        {
            ghostKillCount = 0;
            sharkKillCount = 0;

            _PolterghastTriggerNPCS = [
                ModContent.NPCType<PhantomSpirit>(),
                ModContent.NPCType<PhantomSpiritS>(),
                ModContent.NPCType<PhantomSpiritM>(),
                ModContent.NPCType<PhantomSpiritL>()
            ];

            _GreatSharkTriggerNPCS = [
                NPCID.SandShark,
                NPCID.SandsharkHallow,
                NPCID.SandsharkCorrupt,
                NPCID.SandsharkCrimson
            ];
        }

        public override void Unload()
        {
            _PolterghastTriggerNPCS = null;
            _GreatSharkTriggerNPCS = null;
        }

        public override void OnKill(NPC npc)
        {
            CheckPolterghastCondition(npc);
            CheckGreatSandSharkCondition(npc);
        }

        private static void CheckPolterghastCondition(NPC slainedNPC)
        {
            if (DownedBossSystem.downedPolterghast)
                return;

            if (NPC.AnyNPCs(ModContent.NPCType<Polterghast.Polterghast>()))
                return;

            if (!_PolterghastTriggerNPCS.Contains(slainedNPC.type))
                return;

            ghostKillCount++;
            if (ghostKillCount == 10)
            {
                string key = "Mods.CalamityMod.Status.Boss.GhostBossText2";
                Color messageColor = Color.Cyan;

                CalamityUtils.BroadcastLocalizedText(key, messageColor);
            }
            else if (ghostKillCount == 20)
            {
                string key = "Mods.CalamityMod.Status.Boss.GhostBossText3";
                Color messageColor = Color.Cyan;

                CalamityUtils.BroadcastLocalizedText(key, messageColor);
            }

            if (ghostKillCount >= 30 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                int lastPlayer = slainedNPC.lastInteraction;

                if (!Main.player[lastPlayer].active || Main.player[lastPlayer].dead)
                {
                    lastPlayer = slainedNPC.FindClosestPlayer();
                }

                if (lastPlayer >= 0)
                {
                    SoundEngine.PlaySound(Polterghast.Polterghast.SpawnSound, Main.player[lastPlayer].Center);
                    NPC.SpawnOnPlayer(lastPlayer, ModContent.NPCType<Polterghast.Polterghast>());
                    ghostKillCount = 0;
                }
            }
        }

        private static void CheckGreatSandSharkCondition(NPC slainedNPC)
        {
            if (!NPC.downedPlantBoss)
                return;

            if (NPC.AnyNPCs(ModContent.NPCType<GreatSandShark.GreatSandShark>()))
                return;

            var fusionFeeder = slainedNPC.type == ModContent.NPCType<FusionFeeder>() && Main.zenithWorld;
            if (!_GreatSharkTriggerNPCS.Contains(slainedNPC.type) && !fusionFeeder)
                return;

            sharkKillCount++;
            if (sharkKillCount == 4)
            {
                string key = "Mods.CalamityMod.Status.Boss.SandSharkText";
                Color messageColor = Color.Goldenrod;

                CalamityUtils.BroadcastLocalizedText(key, messageColor);
            }
            else if (sharkKillCount == 8)
            {
                string key = "Mods.CalamityMod.Status.Boss.SandSharkText2";
                Color messageColor = Color.Goldenrod;

                CalamityUtils.BroadcastLocalizedText(key, messageColor);
            }
            if (sharkKillCount >= 10 && Main.netMode != NetmodeID.MultiplayerClient)
            {
                if (!Main.LocalPlayer.dead && Main.LocalPlayer.active)
                {
                    SoundEngine.PlaySound(Mauler.RoarSound, Main.LocalPlayer.Center);
                }

                int lastPlayer = slainedNPC.lastInteraction;

                if (!Main.player[lastPlayer].active || Main.player[lastPlayer].dead)
                {
                    lastPlayer = slainedNPC.FindClosestPlayer();
                }

                if (lastPlayer >= 0)
                {
                    NPC.SpawnOnPlayer(lastPlayer, ModContent.NPCType<GreatSandShark.GreatSandShark>());
                    sharkKillCount = -5;
                }
            }
        }
    }
}
