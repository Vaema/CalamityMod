using System;
using System.Collections.Generic;
using CalamityMod.NPCs.AcidRain;
using CalamityMod.NPCs.AquaticScourge;
using CalamityMod.NPCs.AstrumAureus;
using CalamityMod.NPCs.AstrumDeus;
using CalamityMod.NPCs.BrimstoneElemental;
using CalamityMod.NPCs.Bumblebirb;
using CalamityMod.NPCs.CalClone;
using CalamityMod.NPCs.CeaselessVoid;
using CalamityMod.NPCs.Crabulon;
using CalamityMod.NPCs.Cryogen;
using CalamityMod.NPCs.DesertScourge;
using CalamityMod.NPCs.DevourerofGods;
using CalamityMod.NPCs.ExoMechs.Apollo;
using CalamityMod.NPCs.ExoMechs.Ares;
using CalamityMod.NPCs.ExoMechs.Artemis;
using CalamityMod.NPCs.ExoMechs.Thanatos;
using CalamityMod.NPCs.HiveMind;
using CalamityMod.NPCs.Leviathan;
using CalamityMod.NPCs.NormalNPCs;
using CalamityMod.NPCs.OldDuke;
using CalamityMod.NPCs.Perforator;
using CalamityMod.NPCs.PlaguebringerGoliath;
using CalamityMod.NPCs.Polterghast;
using CalamityMod.NPCs.PrimordialWyrm;
using CalamityMod.NPCs.ProfanedGuardians;
using CalamityMod.NPCs.Providence;
using CalamityMod.NPCs.Ravager;
using CalamityMod.NPCs.Signus;
using CalamityMod.NPCs.SlimeGod;
using CalamityMod.NPCs.StormWeaver;
using CalamityMod.NPCs.SupremeCalamitas;
using CalamityMod.NPCs.Yharon;
using CalamityMod.Projectiles.Boss;
using CalamityMod.Projectiles.Enemy;
using CalamityMod.World;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod
{
    // TODO -- This can be made into a ModSystem with simple OnModLoad and Unload hooks.
    public static partial class NPCStats
    {
        private const double ExpertContactVanillaMultiplier = 2D;
        private const double MasterContactVanillaMultiplier = 3D;
        private const double NormalProjectileVanillaMultiplier = 2D;
        private const double ExpertProjectileVanillaMultiplier = 4D;
        private const double MasterProjectileVanillaMultiplier = 6D;

        #region Enemy Stats Container Struct
        internal partial struct EnemyStats
        {
            public static SortedDictionary<int, int[]> ContactDamageValues;
            public static SortedDictionary<Tuple<int, int>, int[]> ProjectileDamageValues;
        };
        #endregion

        #region Stat Retrieval Methods
        public static void GetNPCDamage(this NPC npc)
        {
            double damageAdjustment = Main.masterMode ? MasterContactVanillaMultiplier : ExpertContactVanillaMultiplier;

            // Safety check: If for some reason the contact damage array is not initialized yet, set the NPC's damage to 1.
            bool exists = EnemyStats.ContactDamageValues.TryGetValue(npc.type, out int[] output);
            if (!exists)
            {
                npc.damage = 1;
                return;
            }
            int[] contactDamage = new int[4];
            if (output.Length == 1)
            {
                contactDamage[0] = output[0];
                contactDamage[1] = output[0] * 2;
                contactDamage[2] = output[0] * 2;
                contactDamage[3] = output[0] * 3;
            }
            else if (output.Length == 2)
            {
                contactDamage[0] = output[0];
                contactDamage[1] = output[1];
                contactDamage[2] = output[1];
                contactDamage[3] = (int)(output[1] * 1.5f);
            }
            else if (output.Length == 4)
                contactDamage = output;

            int normalDamage = contactDamage[0];
            int expertDamage = contactDamage[1] == -1 ? -1 : (int)Math.Round(contactDamage[1] / damageAdjustment);
            int revengeanceDamage = contactDamage[2] == -1 ? -1 : (int)Math.Round(contactDamage[2] / damageAdjustment);
            int masterDamage = contactDamage[3] == -1 ? -1 : (int)Math.Round(contactDamage[3] / damageAdjustment);

            // If the assigned value would be -1, don't actually assign it. This allows for conditionally disabling the system.
            int damageToUse = Main.masterMode ? masterDamage : CalamityWorld.revenge ? revengeanceDamage : Main.expertMode ? expertDamage : normalDamage;
            if (damageToUse != -1)
                npc.damage = damageToUse;
        }

        // Gets the amount of damage a given projectile should do from this NPC.
        // Automatically compensates for Terraria's internal spaghetti scaling.
        public static int GetProjectileDamage(this NPC npc, int projType)
        {
            double damageAdjustment = Main.masterMode ? MasterProjectileVanillaMultiplier : Main.expertMode ? ExpertProjectileVanillaMultiplier : NormalProjectileVanillaMultiplier;

            // Safety check: If for some reason the projectile damage array is not initialized yet, return 1.
            bool exists = EnemyStats.ProjectileDamageValues.TryGetValue(new Tuple<int, int>(npc.type, projType), out int[] output);
            if (!exists)
                return 1;

            int[] projectileDamage = new int[4];
            if (output.Length == 1)
            {
                projectileDamage[0] = output[0];
                projectileDamage[1] = output[0] * 2;
                projectileDamage[2] = output[0] * 2;
                projectileDamage[3] = output[0] * 3;
            }
            else if (output.Length == 2)
            {
                projectileDamage[0] = output[0];
                projectileDamage[1] = output[1];
                projectileDamage[2] = output[1];
                projectileDamage[3] = (int)(output[1] * 1.5f);
            }
            else if (output.Length == 4)
                projectileDamage = output;

            int normalDamage = (int)Math.Round(projectileDamage[0] / damageAdjustment);
            int expertDamage = (int)Math.Round(projectileDamage[1] / damageAdjustment);
            int revengeanceDamage = (int)Math.Round(projectileDamage[2] / damageAdjustment);
            int masterDamage = (int)Math.Round(projectileDamage[3] / damageAdjustment);

            int damageToUse = Main.masterMode ? masterDamage : CalamityWorld.revenge ? revengeanceDamage : Main.expertMode ? expertDamage : normalDamage;

            return damageToUse;
        }

        // Gets the amount of damage this projectile should do from a given NPC.
        // Automatically compensates for Terraria's internal spaghetti scaling.
        public static int GetProjectileDamage(this Projectile projectile, int npcType)
        {
            double damageAdjustment = Main.masterMode ? MasterProjectileVanillaMultiplier : Main.expertMode ? ExpertProjectileVanillaMultiplier : NormalProjectileVanillaMultiplier;

            // Safety check: If for some reason the projectile damage array is not initialized yet, return 1.
            bool exists = EnemyStats.ProjectileDamageValues.TryGetValue(new Tuple<int, int>(npcType, projectile.type), out int[] output);
            if (!exists)
                return 1;

            int[] projectileDamage = new int[4];
            if (output.Length == 1)
            {
                projectileDamage[0] = output[0];
                projectileDamage[1] = output[0] * 2;
                projectileDamage[2] = output[0] * 2;
                projectileDamage[3] = output[0] * 3;
            }
            else if (output.Length == 2)
            {
                projectileDamage[0] = output[0];
                projectileDamage[1] = output[1];
                projectileDamage[2] = output[1];
                projectileDamage[3] = (int)(output[1] * 1.5f);
            }
            else if (output.Length == 4)
                projectileDamage = output;

            int normalDamage = (int)Math.Round(projectileDamage[0] / damageAdjustment);
            int expertDamage = (int)Math.Round(projectileDamage[1] / damageAdjustment);
            int revengeanceDamage = (int)Math.Round(projectileDamage[2] / damageAdjustment);
            int masterDamage = (int)Math.Round(projectileDamage[3] / damageAdjustment);

            int damageToUse = Main.masterMode ? masterDamage : CalamityWorld.revenge ? revengeanceDamage : Main.expertMode ? expertDamage : normalDamage;

            return damageToUse;
        }

        // Gets the raw amount of damage a projectile should do from this NPC.
        // That is, this doesn't adjust the value to compensate for Terraria's internal spaghetti scaling.
        public static int GetProjectileDamageNoScaling(this NPC npc, int projType)
        {
            bool exists = EnemyStats.ProjectileDamageValues.TryGetValue(new Tuple<int, int>(npc.type, projType), out int[] output);
            if (!exists)
                return 1;

            int[] projectileDamage = new int[4];
            if (output.Length == 1)
            {
                projectileDamage[0] = output[0];
                projectileDamage[1] = output[0] * 2;
                projectileDamage[2] = output[0] * 2;
                projectileDamage[3] = output[0] * 3;
            }
            else if (output.Length == 2)
            {
                projectileDamage[0] = output[0];
                projectileDamage[1] = output[1];
                projectileDamage[2] = output[1];
                projectileDamage[3] = (int)(output[1] * 1.5f);
            }
            else if (output.Length == 4)
                projectileDamage = output;

            return Main.masterMode ? projectileDamage[3]
                : CalamityWorld.revenge ? projectileDamage[2]
                : Main.expertMode ? projectileDamage[1]
                : projectileDamage[0];
        }
        #endregion

        #region Load/Unload
        internal static void Load()
        {
            LoadEnemyStats();
            LoadDebuffs();
        }
        internal static void Unload()
        {
            UnloadEnemyStats();
            UnloadDebuffs();
        }

        // A static function, called exactly once, which initializes the EnemyStats struct at a predictable time.
        // This is necessary to ensure this dictionary is populated as early as possible.
        internal static void LoadEnemyStats()
        {
            EnemyStats.ContactDamageValues = new SortedDictionary<int, int[]>
            {
                { ModContent.NPCType<DesertScourgeHead>(), new int[] { 44, 70 } },
                { ModContent.NPCType<DesertScourgeBody>(), new int[] { 20, 36 } },
                { ModContent.NPCType<DesertScourgeTail>(), new int[] { 15 } },
                { ModContent.NPCType<DesertNuisanceHead>(), new int[] { 25, 40 } },
                { ModContent.NPCType<DesertNuisanceBody>(), new int[] { 16 } },
                { ModContent.NPCType<DesertNuisanceTail>(), new int[] { 10 } },
                { ModContent.NPCType<DesertNuisanceHeadYoung>(), new int[] { 25, 40 } },
                { ModContent.NPCType<DesertNuisanceBodyYoung>(), new int[] { 16 } },
                { ModContent.NPCType<DesertNuisanceTailYoung>(), new int[] { 10 } },

                { ModContent.NPCType<Crabulon>(), new int[] { 40, 64 } },
                { ModContent.NPCType<CrabShroom>(), new int[] { 25, 40 } },

                { ModContent.NPCType<HiveMind>(), new int[] { 40, 64 } },
                { ModContent.NPCType<DankCreeper>(), new int[] { 30, 48 } },

                { ModContent.NPCType<PerforatorHive>(), new int[] { 30, 54 } },
                { ModContent.NPCType<PerforatorHeadLarge>(), new int[] { 40, 88 } },
                { ModContent.NPCType<PerforatorBodyLarge>(), new int[] { 23, 40 } },
                { ModContent.NPCType<PerforatorTailLarge>(), new int[] { 20, 32 } },
                { ModContent.NPCType<PerforatorHeadMedium>(), new int[] { 24, 72 } },
                { ModContent.NPCType<PerforatorBodyMedium>(), new int[] { 18, 28 } },
                { ModContent.NPCType<PerforatorTailMedium>(), new int[] { 16, 25 } },
                { ModContent.NPCType<PerforatorHeadSmall>(), new int[] { 20, 60 } },
                { ModContent.NPCType<PerforatorBodySmall>(), new int[] { 14, 22 } },
                { ModContent.NPCType<PerforatorTailSmall>(), new int[] { 12, 19 } },

                { ModContent.NPCType<SlimeGodCore>(), new int[] { 40 } },
                { ModContent.NPCType<EbonianPaladin>(), new int[] { 45 } },
                { ModContent.NPCType<SplitEbonianPaladin>(), new int[] { 40 } },
                { ModContent.NPCType<CrimulanPaladin>(), new int[] { 50 } },
                { ModContent.NPCType<SplitCrimulanPaladin>(), new int[] { 45 } },
                { ModContent.NPCType<CorruptSlimeSpawn>(), new int[] { 30 } },
                { ModContent.NPCType<CorruptSlimeSpawn2>(), new int[] { 20 } },
                { ModContent.NPCType<CrimsonSlimeSpawn>(), new int[] { 35 } },
                { ModContent.NPCType<CrimsonSlimeSpawn2>(), new int[] { 25 } },

                { ModContent.NPCType<Cryogen>(), new int[] { 69 } },
                { ModContent.NPCType<CryogenShield>(), new int[] { 60 } },

                { ModContent.NPCType<AquaticScourgeHead>(), new int[] { 70, 176 } },
                { ModContent.NPCType<AquaticScourgeBody>(), new int[] { 65, 112 } },
                { ModContent.NPCType<AquaticScourgeBodyAlt>(), new int[] { 60, 104 } },
                { ModContent.NPCType<AquaticScourgeTail>(), new int[] { 48, 81 } },

                { ModContent.NPCType<BrimstoneElemental>(), new int[] { 65, 112 } },

                { ModContent.NPCType<CalamitasClone>(), new int[] { 60 } },
                { ModContent.NPCType<Cataclysm>(), new int[] { 50 } },
                { ModContent.NPCType<Catastrophe>(), new int[] { 55 } },

                { ModContent.NPCType<Leviathan>(), new int[] { 100 } },
                { ModContent.NPCType<Anahita>(), new int[] { 75, 110 } }, // Dash: 113, 165
                { ModContent.NPCType<AnahitasIceShield>(), new int[] { 60, 90 } },
                { ModContent.NPCType<AquaticAberration>(), new int[] { 70 } },

                { ModContent.NPCType<AstrumAureus>(), new int[] { 100 } },
                { ModContent.NPCType<AureusSpawn>(), new int[] { 75, 110 } },

                { ModContent.NPCType<PlaguebringerGoliath>(), new int[] { 120, 180 } },
                { ModContent.NPCType<PlagueHomingMissile>(), new int[] { 100, 150 } },
                { ModContent.NPCType<PlagueMine>(), new int[] { 120, 180 } },

                { ModContent.NPCType<RavagerBody>(), new int[] { 120, 180 } },
                { ModContent.NPCType<RavagerClawLeft>(), new int[] { 100, 150 } },
                { ModContent.NPCType<RavagerClawRight>(), new int[] { 100, 150 } },
                { ModContent.NPCType<RockPillar>(), new int[] { 120, 180 } },
                { ModContent.NPCType<FlamePillar>(), new int[] { 100, 150 } },

                { ModContent.NPCType<AstrumDeusHead>(), new int[] { 120 } },
                { ModContent.NPCType<AstrumDeusBody>(), new int[] { 80 } },
                { ModContent.NPCType<AstrumDeusTail>(), new int[] { 64 } },

                { ModContent.NPCType<ProfanedGuardianCommander>(), new int[] { 110 } },
                { ModContent.NPCType<ProfanedGuardianDefender>(), new int[] { 110 } },
                { ModContent.NPCType<ProfanedGuardianHealer>(), new int[] { 100 } },
                { ModContent.NPCType<ProfanedRocks>(), new int[] { 100 } },

                { ModContent.NPCType<Dragonfolly>(), new int[] { 120 } },
                { ModContent.NPCType<DraconicSwarmer>(), new int[] { 110 } },

                { ModContent.NPCType<CeaselessVoid>(), new int[] { 180 } },
                { ModContent.NPCType<DarkEnergy>(), new int[] { 120 } },

                { ModContent.NPCType<StormWeaverHead>(), new int[] { 180 } },
                { ModContent.NPCType<StormWeaverBody>(), new int[] { 96 } },
                { ModContent.NPCType<StormWeaverTail>(), new int[] { 80 } },

                { ModContent.NPCType<Signus>(), new int[] { 160 } },
                { ModContent.NPCType<CosmicLantern>(), new int[] { 120 } },
                { ModContent.NPCType<CosmicMine>(), new int[] { 140 } },

                { ModContent.NPCType<Polterghast>(), new int[] { 120 } }, // Phase 2: 144 + Phase 3: 168
                { ModContent.NPCType<PolterPhantom>(), new int[] { 168 } },

                { ModContent.NPCType<OldDuke>(), new int[] { 140 } }, // Phase 2: 154 + Phase 3: 168
                { ModContent.NPCType<OldDukeToothBall>(), new int[] { 120 } },
                { ModContent.NPCType<SulphurousSharkron>(), new int[] { 120 } },

                { ModContent.NPCType<DevourerofGodsHead>(), new int[] { 250 } },
                { ModContent.NPCType<DevourerofGodsBody>(), new int[] { 150 } },
                { ModContent.NPCType<DevourerofGodsTail>(), new int[] { 100 } },
                { ModContent.NPCType<CosmicGuardianHead>(), new int[] { 180 } },
                { ModContent.NPCType<CosmicGuardianBody>(), new int[] { 120 } },
                { ModContent.NPCType<CosmicGuardianTail>(), new int[] { 90 } },

                { ModContent.NPCType<Yharon>(), new int[] { 200 } },

                { ModContent.NPCType<SupremeCalamitas>(), new int[] { 225 } },

                { ModContent.NPCType<Apollo>(), new int[] { 240 } },
                { ModContent.NPCType<Artemis>(), new int[] { 200 } },

                { ModContent.NPCType<ThanatosHead>(), new int[] { 270 } },
                { ModContent.NPCType<ThanatosBody1>(), new int[] { 180 } },
                { ModContent.NPCType<ThanatosBody2>(), new int[] { 180 } },
                { ModContent.NPCType<ThanatosTail>(), new int[] { 150 } },

                { ModContent.NPCType<PrimordialWyrmHead>(), new int[] { 300 } }
            };

            EnemyStats.ProjectileDamageValues = new SortedDictionary<Tuple<int, int>, int[]>
            {
                { new Tuple<int, int>(ModContent.NPCType<DesertScourgeHead>(), ModContent.ProjectileType<DesertScourgeSpit>()), new int[] { 18 } },
                { new Tuple<int, int>(ModContent.NPCType<DesertNuisanceHeadYoung>(), ModContent.ProjectileType<DesertScourgeSpit>()), new int[] { 18 } },

                { new Tuple<int, int>(ModContent.NPCType<Crabulon>(), ModContent.ProjectileType<MushBomb>()), new int[] { 22 } },
                { new Tuple<int, int>(ModContent.NPCType<Crabulon>(), ModContent.ProjectileType<MushBombFall>()), new int[] { 22 } },
                { new Tuple<int, int>(ModContent.NPCType<Crabulon>(), ModContent.ProjectileType<MushBombGround>()), new int[] { 22 } },

                { new Tuple<int, int>(ModContent.NPCType<HiveMind>(), ModContent.ProjectileType<ShadeNimbusHostile>()), new int[] { 20 } },
                { new Tuple<int, int>(ModContent.NPCType<DankCreeper>(), ModContent.ProjectileType<ShadeNimbusHostile>()), new int[] { 20 } },
                { new Tuple<int, int>(ModContent.NPCType<DarkHeart>(), ModContent.ProjectileType<ShaderainHostile>()), new int[] { 20 } },
                { new Tuple<int, int>(ModContent.NPCType<HiveBlob>(), ModContent.ProjectileType<VileClot>()), new int[] { 16 } },
                { new Tuple<int, int>(ModContent.NPCType<HiveBlob2>(), ModContent.ProjectileType<VileClot>()), new int[] { 16 } },

                { new Tuple<int, int>(ModContent.NPCType<PerforatorHive>(), ModContent.ProjectileType<BloodGeyser>()), new int[] { 20 } },
                { new Tuple<int, int>(ModContent.NPCType<PerforatorHive>(), ModContent.ProjectileType<IchorShot>()), new int[] { 20 } },
                { new Tuple<int, int>(ModContent.NPCType<PerforatorHive>(), ModContent.ProjectileType<IchorBlob>()), new int[] { 20 } },
                { new Tuple<int, int>(ModContent.NPCType<PerforatorHeadMedium>(), ModContent.ProjectileType<IchorBlob>()), new int[] { 20 } },
                { new Tuple<int, int>(ModContent.NPCType<PerforatorBodyMedium>(), ModContent.ProjectileType<IchorBlob>()), new int[] { 20 } },
                { new Tuple<int, int>(ModContent.NPCType<PerforatorTailMedium>(), ModContent.ProjectileType<IchorBlob>()), new int[] { 20 } },
                { new Tuple<int, int>(ModContent.NPCType<PerforatorHeadLarge>(), ModContent.ProjectileType<BloodGeyser>()), new int[] { 20 } },
                { new Tuple<int, int>(ModContent.NPCType<PerforatorHeadLarge>(), ModContent.ProjectileType<IchorShot>()), new int[] { 20 } },
                { new Tuple<int, int>(ModContent.NPCType<PerforatorHeadLarge>(), ModContent.ProjectileType<IchorBlob>()), new int[] { 20 } },
                { new Tuple<int, int>(ModContent.NPCType<PerforatorHeadLarge>(), ModContent.ProjectileType<DoGDeath>()), new int[] { 22 } },

                { new Tuple<int, int>(ModContent.NPCType<SlimeGodCore>(), ModContent.ProjectileType<UnstableEbonianGlob>()), new int[] { 30 } },
                { new Tuple<int, int>(ModContent.NPCType<SlimeGodCore>(), ModContent.ProjectileType<UnstableCrimulanGlob>()), new int[] { 30 } },
                { new Tuple<int, int>(ModContent.NPCType<EbonianPaladin>(), ModContent.ProjectileType<UnstableEbonianGlob>()), new int[] { 30 } },
                { new Tuple<int, int>(ModContent.NPCType<CrimulanPaladin>(), ModContent.ProjectileType<UnstableCrimulanGlob>()), new int[] { 30 } },
                { new Tuple<int, int>(ModContent.NPCType<CrimulanPaladin>(), ModContent.ProjectileType<CrimulanSpike>()), new int[] { 42 } },
                { new Tuple<int, int>(ModContent.NPCType<SplitEbonianPaladin>(), ModContent.ProjectileType<UnstableEbonianGlob>()), new int[] { 30 } },
                { new Tuple<int, int>(ModContent.NPCType<SplitCrimulanPaladin>(), ModContent.ProjectileType<UnstableCrimulanGlob>()), new int[] { 30 } },
                { new Tuple<int, int>(ModContent.NPCType<CorruptSlimeSpawn>(), ModContent.ProjectileType<ShadeNimbusHostile>()), new int[] { 34 } },
                { new Tuple<int, int>(ModContent.NPCType<CrimsonSlimeSpawn2>(), ModContent.ProjectileType<CrimsonSpike>()), new int[] { 24 } },

                { new Tuple<int, int>(ModContent.NPCType<Cryogen>(), ModContent.ProjectileType<IceBlast>()), new int[] { 45 } },
                { new Tuple<int, int>(ModContent.NPCType<Cryogen>(), ModContent.ProjectileType<IceBomb>()), new int[] { 60 } },
                { new Tuple<int, int>(ModContent.NPCType<Cryogen>(), ModContent.ProjectileType<IceRain>()), new int[] { 45 } },
                { new Tuple<int, int>(ModContent.NPCType<CryogenShield>(), ModContent.ProjectileType<IceBlast>()), new int[] { 45 } },

                { new Tuple<int, int>(ModContent.NPCType<AquaticScourgeHead>(), ModContent.ProjectileType<SulphuricAcidMist>()), new int[] { 50, 92 } },
                { new Tuple<int, int>(ModContent.NPCType<AquaticScourgeHead>(), ModContent.ProjectileType<SandPoisonCloud>()), new int[] { 70, 120 } },
                { new Tuple<int, int>(ModContent.NPCType<AquaticScourgeHead>(), ModContent.ProjectileType<ToxicCloud>()), new int[] { 80, 140 } },
                { new Tuple<int, int>(ModContent.NPCType<AquaticScourgeBody>(), ModContent.ProjectileType<SandTooth>()), new int[] { 50, 92 } },

                { new Tuple<int, int>(ModContent.NPCType<BrimstoneElemental>(), ModContent.ProjectileType<BrimstoneHellfireball>()), new int[] { 60, 108 } },
                { new Tuple<int, int>(ModContent.NPCType<BrimstoneElemental>(), ModContent.ProjectileType<BrimstoneBarrage>()), new int[] { 50, 92 } },
                { new Tuple<int, int>(ModContent.NPCType<BrimstoneElemental>(), ModContent.ProjectileType<BrimstoneHellblast>()), new int[] { 60, 108 } },
                { new Tuple<int, int>(ModContent.NPCType<BrimstoneElemental>(), ModContent.ProjectileType<BrimstoneRay>()), new int[] { 80, 200 } }, // Split shots: 48, 120
                { new Tuple<int, int>(ModContent.NPCType<Brimling>(), ModContent.ProjectileType<BrimstoneHellfireball>()), new int[] { 60, 108 } },
                { new Tuple<int, int>(ModContent.NPCType<Brimling>(), ModContent.ProjectileType<BrimstoneBarrage>()), new int[] { 50, 92 } },

                { new Tuple<int, int>(ModContent.NPCType<CalamitasClone>(), ModContent.ProjectileType<BrimstoneHellblast>()), new int[] { 60, 108 } },
                { new Tuple<int, int>(ModContent.NPCType<CalamitasClone>(), ModContent.ProjectileType<BrimstoneHellfireball>()), new int[] { 60, 108 } },
                { new Tuple<int, int>(ModContent.NPCType<CalamitasClone>(), ModContent.ProjectileType<BrimstoneHellblast2>()), new int[] { 60, 108 } },
                { new Tuple<int, int>(ModContent.NPCType<CalamitasClone>(), ModContent.ProjectileType<SCalBrimstoneFireblast>()), new int[] { 80, 140 } },
                { new Tuple<int, int>(ModContent.NPCType<CalamitasClone>(), ModContent.ProjectileType<BrimstoneBarrage>()), new int[] { 50, 92 } },
                { new Tuple<int, int>(ModContent.NPCType<Cataclysm>(), ModContent.ProjectileType<BrimstoneFire>()), new int[] { 70, 120 } },
                { new Tuple<int, int>(ModContent.NPCType<Catastrophe>(), ModContent.ProjectileType<BrimstoneBall>()), new int[] { 60, 108 } },
                { new Tuple<int, int>(ModContent.NPCType<SoulSeeker>(), ModContent.ProjectileType<BrimstoneBarrage>()), new int[] { 50, 92 } },

                { new Tuple<int, int>(ModContent.NPCType<Leviathan>(), ModContent.ProjectileType<LeviathanBomb>()), new int[] { 100, 160 } },
                { new Tuple<int, int>(ModContent.NPCType<Anahita>(), ModContent.ProjectileType<WaterSpear>()), new int[] { 48 } },
                { new Tuple<int, int>(ModContent.NPCType<Anahita>(), ModContent.ProjectileType<FrostMist>()), new int[] { 48 } },
                { new Tuple<int, int>(ModContent.NPCType<Anahita>(), ModContent.ProjectileType<SirenSong>()), new int[] { 60 } },

                { new Tuple<int, int>(ModContent.NPCType<AstrumAureus>(), ModContent.ProjectileType<AstralLaser>()), new int[] { 48 } },
                { new Tuple<int, int>(ModContent.NPCType<AstrumAureus>(), ModContent.ProjectileType<AstralFlame>()), new int[] { 60 } },
                { new Tuple<int, int>(ModContent.NPCType<AureusSpawn>(), ModContent.ProjectileType<AstralLaser>()), new int[] { 48 } },

                { new Tuple<int, int>(ModContent.NPCType<PlaguebringerGoliath>(), ModContent.ProjectileType<PlagueStingerGoliath>()), new int[] { 56 } },
                { new Tuple<int, int>(ModContent.NPCType<PlaguebringerGoliath>(), ModContent.ProjectileType<PlagueStingerGoliathV2>()), new int[] { 56 } },
                { new Tuple<int, int>(ModContent.NPCType<PlaguebringerGoliath>(), ModContent.ProjectileType<HiveBombGoliath>()), new int[] { 75 } },

                { new Tuple<int, int>(ModContent.NPCType<RavagerBody>(), ModContent.ProjectileType<RavagerBlaster>()), new int[] { 90 } },
                { new Tuple<int, int>(ModContent.NPCType<RavagerHead>(), ModContent.ProjectileType<RavagerNuke>()), new int[] { 75 } },
                { new Tuple<int, int>(ModContent.NPCType<RavagerHead2>(), ModContent.ProjectileType<HomingLaserDart>()), new int[] { 60 } },
                { new Tuple<int, int>(ModContent.NPCType<RavagerHead2>(), ModContent.ProjectileType<RavagerNuke>()), new int[] { 75 } },
                { new Tuple<int, int>(ModContent.NPCType<FlamePillar>(), ModContent.ProjectileType<RavagerFlame>()), new int[] { 60 } },

                { new Tuple<int, int>(ModContent.NPCType<AstrumDeusBody>(), ModContent.ProjectileType<AstralShot2>()), new int[] { 60 } },
                { new Tuple<int, int>(ModContent.NPCType<AstrumDeusBody>(), ModContent.ProjectileType<DeusMine>()), new int[] { 80 } },
                { new Tuple<int, int>(ModContent.NPCType<AstrumDeusBody>(), ModContent.ProjectileType<AstralGodRay>()), new int[] { 80 } },

                { new Tuple<int, int>(ModContent.NPCType<ProfanedGuardianCommander>(), ModContent.ProjectileType<ProfanedSpear>()), new int[] { 110 } },
                { new Tuple<int, int>(ModContent.NPCType<ProfanedGuardianCommander>(), ModContent.ProjectileType<HolySpear>()), new int[] { 110 } },
                { new Tuple<int, int>(ModContent.NPCType<ProfanedGuardianCommander>(), ModContent.ProjectileType<HolyBlast>()), new int[] { 110 } },
                { new Tuple<int, int>(ModContent.NPCType<ProfanedGuardianCommander>(), ModContent.ProjectileType<HolyFire>()), new int[] { 96 } },
                { new Tuple<int, int>(ModContent.NPCType<ProfanedGuardianCommander>(), ModContent.ProjectileType<HolyFire2>()), new int[] { 96 } },
                { new Tuple<int, int>(ModContent.NPCType<ProfanedGuardianCommander>(), ModContent.ProjectileType<ProvidenceHolyRay>()), new int[] { 160 } },
                { new Tuple<int, int>(ModContent.NPCType<ProfanedGuardianDefender>(), ModContent.ProjectileType<HolyBomb>()), new int[] { 110 } },
                { new Tuple<int, int>(ModContent.NPCType<ProfanedGuardianDefender>(), ModContent.ProjectileType<HolyFlare>()), new int[] { 85 } },
                { new Tuple<int, int>(ModContent.NPCType<ProfanedGuardianDefender>(), ModContent.ProjectileType<MoltenBlast>()), new int[] { 110 } },
                { new Tuple<int, int>(ModContent.NPCType<ProfanedGuardianDefender>(), ModContent.ProjectileType<MoltenBlob>()), new int[] { 85 } },
                { new Tuple<int, int>(ModContent.NPCType<ProfanedGuardianHealer>(), ModContent.ProjectileType<ProvidenceCrystalShard>()), new int[] { 110 } },
                { new Tuple<int, int>(ModContent.NPCType<ProfanedGuardianHealer>(), ModContent.ProjectileType<HolyBurnOrb>()), new int[] { 96 } },
                { new Tuple<int, int>(ModContent.NPCType<ProfanedGuardianHealer>(), ModContent.ProjectileType<HolyLight>()), new int[] { 35, 50 } },

                { new Tuple<int, int>(ModContent.NPCType<Dragonfolly>(), ModContent.ProjectileType<RedLightningFeather>()), new int[] { 85 } },
                { new Tuple<int, int>(ModContent.NPCType<Dragonfolly>(), ModContent.ProjectileType<BirbAuraFlare>()), new int[] { 150 } },

                { new Tuple<int, int>(ModContent.NPCType<Providence>(), ModContent.ProjectileType<HolyBlast>()), new int[] { 128 } }, // Split holy fire: 96
                { new Tuple<int, int>(ModContent.NPCType<Providence>(), ModContent.ProjectileType<HolyFire>()), new int[] { 96 } },
                { new Tuple<int, int>(ModContent.NPCType<Providence>(), ModContent.ProjectileType<HolyFire2>()), new int[] { 96 } },
                { new Tuple<int, int>(ModContent.NPCType<Providence>(), ModContent.ProjectileType<HolyBurnOrb>()), new int[] { 110 } },
                { new Tuple<int, int>(ModContent.NPCType<Providence>(), ModContent.ProjectileType<HolyLight>()), new int[] { 35, 50 } },
                { new Tuple<int, int>(ModContent.NPCType<Providence>(), ModContent.ProjectileType<MoltenBlast>()), new int[] { 110 } },
                { new Tuple<int, int>(ModContent.NPCType<Providence>(), ModContent.ProjectileType<MoltenBlob>()), new int[] { 85 } },
                { new Tuple<int, int>(ModContent.NPCType<Providence>(), ModContent.ProjectileType<HolyBomb>()), new int[] { 110 } },
                { new Tuple<int, int>(ModContent.NPCType<Providence>(), ModContent.ProjectileType<HolyFlare>()), new int[] { 85 } },
                { new Tuple<int, int>(ModContent.NPCType<Providence>(), ModContent.ProjectileType<HolySpear>()), new int[] { 110 } },
                { new Tuple<int, int>(ModContent.NPCType<Providence>(), ModContent.ProjectileType<ProvidenceCrystal>()), new int[] { 110 } },
                { new Tuple<int, int>(ModContent.NPCType<Providence>(), ModContent.ProjectileType<ProvidenceCrystalShard>()), new int[] { 110 } },
                { new Tuple<int, int>(ModContent.NPCType<Providence>(), ModContent.ProjectileType<ProvidenceHolyRay>()), new int[] { 200 } },

                { new Tuple<int, int>(ModContent.NPCType<CeaselessVoid>(), ModContent.ProjectileType<DoGBeamPortal>()), new int[] { 120 } },
                { new Tuple<int, int>(ModContent.NPCType<CeaselessVoid>(), ModContent.ProjectileType<DarkEnergyBall>()), new int[] { 120 } },
                { new Tuple<int, int>(ModContent.NPCType<CeaselessVoid>(), ModContent.ProjectileType<DarkEnergyBall2>()), new int[] { 120 } },

                { new Tuple<int, int>(ModContent.NPCType<StormWeaverHead>(), ProjectileID.CultistBossLightningOrbArc), new int[] { 132 } },
                { new Tuple<int, int>(ModContent.NPCType<StormWeaverHead>(), ProjectileID.FrostWave), new int[] { 132 } },
                { new Tuple<int, int>(ModContent.NPCType<StormWeaverHead>(), ModContent.ProjectileType<StormMarkHostile>()), new int[] { 132 } },
                { new Tuple<int, int>(ModContent.NPCType<StormWeaverBody>(), ModContent.ProjectileType<DestroyerElectricLaser>()), new int[] { 120 } },
                { new Tuple<int, int>(ModContent.NPCType<StormWeaverTail>(), ProjectileID.CultistBossLightningOrb), new int[] { 132 } },

                { new Tuple<int, int>(ModContent.NPCType<Signus>(), ModContent.ProjectileType<SignusScythe>()), new int[] { 120 } },
                { new Tuple<int, int>(ModContent.NPCType<Signus>(), ModContent.ProjectileType<EssenceDust>()), new int[] { 120 } },

                { new Tuple<int, int>(ModContent.NPCType<Polterghast>(), ModContent.ProjectileType<PhantomShot>()), new int[] { 120 } },
                { new Tuple<int, int>(ModContent.NPCType<Polterghast>(), ModContent.ProjectileType<PhantomShot2>()), new int[] { 132 } },
                { new Tuple<int, int>(ModContent.NPCType<Polterghast>(), ModContent.ProjectileType<PhantomBlast>()), new int[] { 132 } },
                { new Tuple<int, int>(ModContent.NPCType<Polterghast>(), ModContent.ProjectileType<PhantomBlast2>()), new int[] { 144 } },
                { new Tuple<int, int>(ModContent.NPCType<PolterghastHook>(), ModContent.ProjectileType<PhantomHookShot>()), new int[] { 120 } },
                { new Tuple<int, int>(ModContent.NPCType<PhantomFuckYou>(), ModContent.ProjectileType<PhantomMine>()), new int[] { 150 } },
                { new Tuple<int, int>(ModContent.NPCType<PhantomSpiritL>(), ModContent.ProjectileType<PhantomGhostShot>()), new int[] { 132 } },

                { new Tuple<int, int>(ModContent.NPCType<Mauler>(), ModContent.ProjectileType<MaulerAcidBubble>()), new int[] { 110 } },
                { new Tuple<int, int>(ModContent.NPCType<Mauler>(), ModContent.ProjectileType<MaulerAcidDrop>()), new int[] { 110 } },

                { new Tuple<int, int>(ModContent.NPCType<OldDuke>(), ModContent.ProjectileType<OldDukeGore>()), new int[] { 140 } },
                { new Tuple<int, int>(ModContent.NPCType<OldDuke>(), ModContent.ProjectileType<OldDukeVortex>()), new int[] { 210 } },
                { new Tuple<int, int>(ModContent.NPCType<OldDukeToothBall>(), ModContent.ProjectileType<OldDukeToothBallSpike>()), new int[] { 120 } },
                { new Tuple<int, int>(ModContent.NPCType<OldDukeToothBall>(), ModContent.ProjectileType<SandPoisonCloudOldDuke>()), new int[] { 140 } },
                { new Tuple<int, int>(ModContent.NPCType<SulphurousSharkron>(), ModContent.ProjectileType<OldDukeGore>()), new int[] { 120 } },

                { new Tuple<int, int>(ModContent.NPCType<DevourerofGodsHead>(), ModContent.ProjectileType<DoGDeath>()), new int[] { 150 } },
                { new Tuple<int, int>(ModContent.NPCType<DevourerofGodsHead>(), ModContent.ProjectileType<DoGFire>()), new int[] { 125 } },

                { new Tuple<int, int>(ModContent.NPCType<Yharon>(), ModContent.ProjectileType<SkyFlareRevenge>()), new int[] { 250 } },
                { new Tuple<int, int>(ModContent.NPCType<Yharon>(), ModContent.ProjectileType<FlareBomb>()), new int[] { 144 } },
                { new Tuple<int, int>(ModContent.NPCType<Yharon>(), ModContent.ProjectileType<Flarenado>()), new int[] { 180 } },
                { new Tuple<int, int>(ModContent.NPCType<Yharon>(), ModContent.ProjectileType<Infernado>()), new int[] { 180 } },
                { new Tuple<int, int>(ModContent.NPCType<Yharon>(), ModContent.ProjectileType<Infernado2>()), new int[] { 180 } },
                { new Tuple<int, int>(ModContent.NPCType<Yharon>(), ModContent.ProjectileType<FlareDust>()), new int[] { 144 } },
                { new Tuple<int, int>(ModContent.NPCType<Yharon>(), ModContent.ProjectileType<FlareDust2>()), new int[] { 144 } },
                { new Tuple<int, int>(ModContent.NPCType<Yharon>(), ModContent.ProjectileType<YharonFireball>()), new int[] { 144 } },
                { new Tuple<int, int>(ModContent.NPCType<Yharon>(), ModContent.ProjectileType<YharonBulletHellVortex>()), new int[] { 180 } },

                { new Tuple<int, int>(ModContent.NPCType<SupremeCalamitas>(), ModContent.ProjectileType<BrimstoneHellblast2>()), new int[] { 200 } },
                { new Tuple<int, int>(ModContent.NPCType<SupremeCalamitas>(), ModContent.ProjectileType<SCalBrimstoneFireblast>()), new int[] { 200 } },
                { new Tuple<int, int>(ModContent.NPCType<SupremeCalamitas>(), ModContent.ProjectileType<SCalBrimstoneGigablast>()), new int[] { 225 } },
                { new Tuple<int, int>(ModContent.NPCType<SupremeCalamitas>(), ModContent.ProjectileType<BrimstoneMonster>()), new int[] { 225 } }, // Deals damage non-conventionally. Ignore this value.
                { new Tuple<int, int>(ModContent.NPCType<SupremeCalamitas>(), ModContent.ProjectileType<BrimstoneWave>()), new int[] { 200 } },
                { new Tuple<int, int>(ModContent.NPCType<SupremeCalamitas>(), ModContent.ProjectileType<BrimstoneBarrage>()), new int[] { 170 } },
                { new Tuple<int, int>(ModContent.NPCType<SupremeCalamitas>(), ModContent.ProjectileType<BrimstoneHellblast>()), new int[] { 200 } },
                { new Tuple<int, int>(ModContent.NPCType<SepulcherBodyEnergyBall>(), ModContent.ProjectileType<BrimstoneBarrage>()), new int[] { 170 } },
                { new Tuple<int, int>(ModContent.NPCType<SoulSeekerSupreme>(), ModContent.ProjectileType<BrimstoneBarrage>()), new int[] { 170 } },
                { new Tuple<int, int>(ModContent.NPCType<SupremeCataclysm>(), ModContent.ProjectileType<SupremeCataclysmFist>()), new int[] { 225 } },
                { new Tuple<int, int>(ModContent.NPCType<SupremeCataclysm>(), ModContent.ProjectileType<BrimstoneBarrage>()), new int[] { 170 } },
                { new Tuple<int, int>(ModContent.NPCType<SupremeCatastrophe>(), ModContent.ProjectileType<SupremeCatastropheSlash>()), new int[] { 225 } },
                { new Tuple<int, int>(ModContent.NPCType<SupremeCatastrophe>(), ModContent.ProjectileType<BrimstoneBarrage>()), new int[] { 170 } },

                { new Tuple<int, int>(ModContent.NPCType<Artemis>(), ModContent.ProjectileType<ArtemisSpinLaserbeam>()), new int[] { 225 } },
                { new Tuple<int, int>(ModContent.NPCType<Artemis>(), ModContent.ProjectileType<ArtemisLaser>()), new int[] { 170 } },
                { new Tuple<int, int>(ModContent.NPCType<Apollo>(), ModContent.ProjectileType<ApolloFireball>()), new int[] { 170 } },
                { new Tuple<int, int>(ModContent.NPCType<Apollo>(), ModContent.ProjectileType<ApolloRocket>()), new int[] { 200 } },

                { new Tuple<int, int>(ModContent.NPCType<ThanatosHead>(), ModContent.ProjectileType<ThanatosBeamStart>()), new int[] { 270 } },
                { new Tuple<int, int>(ModContent.NPCType<ThanatosHead>(), ModContent.ProjectileType<ThanatosLaser>()), new int[] { 170 } },
                { new Tuple<int, int>(ModContent.NPCType<ThanatosBody1>(), ModContent.ProjectileType<ThanatosLaser>()), new int[] { 170 } },
                { new Tuple<int, int>(ModContent.NPCType<ThanatosBody2>(), ModContent.ProjectileType<ThanatosLaser>()), new int[] { 170 } },
                { new Tuple<int, int>(ModContent.NPCType<ThanatosTail>(), ModContent.ProjectileType<ThanatosLaser>()), new int[] { 170 } },

                { new Tuple<int, int>(ModContent.NPCType<AresBody>(), ModContent.ProjectileType<AresDeathBeamStart>()), new int[] { 225 } },
                { new Tuple<int, int>(ModContent.NPCType<AresLaserCannon>(), ModContent.ProjectileType<AresLaserBeamStart>()), new int[] { 225 } },
                { new Tuple<int, int>(ModContent.NPCType<AresLaserCannon>(), ModContent.ProjectileType<ThanatosLaser>()), new int[] { 170 } },
                { new Tuple<int, int>(ModContent.NPCType<AresPlasmaFlamethrower>(), ModContent.ProjectileType<AresPlasmaFireball>()), new int[] { 170 } },
                { new Tuple<int, int>(ModContent.NPCType<AresTeslaCannon>(), ModContent.ProjectileType<AresTeslaOrb>()), new int[] { 170 } },
                { new Tuple<int, int>(ModContent.NPCType<AresGaussNuke>(), ModContent.ProjectileType<AresGaussNukeProjectile>()), new int[] { 270 } },

                { new Tuple<int, int>(ModContent.NPCType<PrimordialWyrmHead>(), ProjectileID.CultistBossIceMist), new int[] { 225 } },
                { new Tuple<int, int>(ModContent.NPCType<PrimordialWyrmHead>(), ProjectileID.CultistBossLightningOrbArc), new int[] { 280 } },
                { new Tuple<int, int>(ModContent.NPCType<PrimordialWyrmHead>(), ProjectileID.AncientDoomProjectile), new int[] { 225 } },
                { new Tuple<int, int>(ModContent.NPCType<PrimordialWyrmBodyAlt>(), ProjectileID.CultistBossFireBallClone), new int[] { 225 } }
            };
        }

        // Destroys the EnemyStats struct to save memory because mod assemblies will not be fully unloaded until TML 1.4.
        internal static void UnloadEnemyStats()
        {
            EnemyStats.ContactDamageValues = null;
            EnemyStats.ProjectileDamageValues = null;
        }
        #endregion
    }
}
