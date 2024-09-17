using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using CalamityMod.Balancing;
using CalamityMod.CalPlayer;
using CalamityMod.CalPlayer.Dashes;
using CalamityMod.Cooldowns;
using CalamityMod.DataStructures;
using CalamityMod.Effects;
using CalamityMod.Events;
using CalamityMod.FluidSimulation;
using CalamityMod.Graphics.Primitives;
using CalamityMod.Items;
using CalamityMod.Items.Dyes.HairDye;
using CalamityMod.Items.VanillaArmorChanges;
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
using CalamityMod.Particles;
using CalamityMod.Projectiles;
using CalamityMod.Projectiles.BaseProjectiles;
using CalamityMod.Schematics;
using CalamityMod.Skies;
using CalamityMod.Systems;
using CalamityMod.UI;
using CalamityMod.UI.CalamitasEnchants;
using CalamityMod.UI.DraedonsArsenal;
using CalamityMod.UI.Rippers;
using CalamityMod.Waters;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Dyes;
using Terraria.GameContent.Liquid;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Config;

[assembly: InternalsVisibleTo("CalTestHelpers")]
[assembly: InternalsVisibleTo("InfernumMode")]
namespace CalamityMod
{
    public class CalamityMod : Mod
    {
        // TODO -- A huge amount of random floating variables exist here.
        // These should all be moved to other files, whether that's CalamityLists or brand new ModSystems.
        // It is best to have a ton of small ModSystems.

        // Holds the Texture Arrays for all the lava textures.
        // These are used for the lava styles. They are seperate from Textureasset.Instance._liquidTexture as they will conflict with ModWaterStyle
        // Can hold up to 255 lava styles (more than enough) (excluding the normal lava texture which is liquidTexture 1)
        public struct LavaTextures
        {
            public static Asset<Texture2D>[] liquid = new Asset<Texture2D>[1];
            public static Asset<Texture2D>[] slope = new Asset<Texture2D>[1];
            public static Asset<Texture2D>[] block = new Asset<Texture2D>[1];
            public static Asset<Texture2D>[] fall = new Asset<Texture2D>[1];
        }

        public static int LavaStyle;

        public static float[] lavaAlpha = new float[1];

        // Boss Kill Time data structure
        public static SortedDictionary<int, int> bossKillTimes;

        #region External Flags
        // External flag to disable non-Revengeance boss AI edits
        // This can be edited by other mods using reflection to prevent compatibility issues
        public static bool ExternalFlag_DisableNonRevBossAI = false;

        // External flag to disable Defense Damage
        // This can be edited by other mods using reflection if desired
        // Note that this flag trumps Bloodflare Core and will stop that accessory from working properly.
        // There is also a means to disable defense damage on a per-player basis.
        public static bool ExternalFlag_DisableDefenseDamage = false;
        #endregion

        internal static CalamityMod Instance => _Instance ??= ModContent.GetInstance<CalamityMod>();
        private static CalamityMod _Instance;

        #region Load
        public override void Load()
        {
            // Initialize the CalamityLists as this is coupled in tons of other type
            CalamityLists.Load();

            // Initialize the EnemyStats struct as early as it is safe to do so
            NPCStats.Load();

            // Initialize Calamity Balance, since it is tightly coupled with the remaining systems
            CalamityGlobalItem.LoadTweaks();
            CalamityGlobalProjectile.LoadTweaks();

            if (!Main.dedServ)
            {
                LoadClient();
                PrimitiveRenderer.Initialize();
                ForegroundDrawing.ForegroundManager.Load();
            }

            EnchantmentManager.LoadAllEnchantments();
            VanillaArmorChangeManager.Load();
            SetupBossKillTimes();
            SchematicManager.Load();

            //lava
            LavaRendering.instance = new LavaRendering();

            Attunement.Load();
            BalancingChangesManager.Load();
            BaseIdleHoldoutProjectile.LoadAll();
            PlayerDashManager.Load();
        }

        private void LoadClient()
        {
            // Lava Texture
            LavaTextures.liquid[0] = LiquidRenderer.Instance._liquidTextures[1];
            LavaTextures.slope[0] = TextureAssets.LiquidSlope[1];
            LavaTextures.block[0] = TextureAssets.Liquid[1];
            var waterfallTexture = (Asset<Texture2D>[])typeof(WaterfallManager).GetField("waterfallTexture", BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public).GetValue(Main.instance.waterfallManager);
            LavaTextures.fall[0] = waterfallTexture[1];

            // This must be done separately from immediate loading, as loading is now multithreaded.
            // However, render targets and certain other graphical objects can only be created on the main thread.
            Main.QueueMainThreadAction(() =>
            {
                Main.OnPreDraw += PrepareRenderTargets;
            });

            InvasionProgressUIManager.LoadGUIs();
        }
        #endregion

        #region Unload
        public override void Unload()
        {
            bossKillTimes?.Clear();
            bossKillTimes = null;

            CalamityLists.Unload();

            BalancingChangesManager.Unload();
            Attunement.Unload();
            EnchantmentManager.UnloadAllEnchantments();
            VanillaArmorChangeManager.Unload();
            NPCStats.Unload();
            CalamityGlobalItem.UnloadTweaks();
            CalamityGlobalProjectile.UnloadTweaks();

            PopupGUIManager.UnloadGUIs();
            InvasionProgressUIManager.UnloadGUIs();
            SchematicManager.Unload();
            PlayerDashManager.Unload();

            Main.QueueMainThreadAction(() =>
            {
                Main.OnPreDraw -= PrepareRenderTargets;
            });

            _Instance = null;
            base.Unload();
        }
        #endregion

        #region Render Target Management

        public static void PrepareRenderTargets(GameTime gameTime)
        {
            DeathAshParticle.PrepareRenderTargets();
            FluidFieldManager.Update();
        }
        #endregion Render Target Management

        #region Force ModConfig save (Reflection)
        internal static void SaveConfig(CalamityClientConfig cfg)
        {
            // There is no current way to manually save a mod configuration file in tModLoader.
            // The method which saves mod config files is private in ConfigManager, so reflection is used to invoke it.
            try
            {
                MethodInfo saveMethodInfo = typeof(ConfigManager).GetMethod("Save", BindingFlags.Static | BindingFlags.NonPublic);
                if (saveMethodInfo is not null)
                    saveMethodInfo.Invoke(null, new object[] { cfg });
                else
                    Instance.Logger.Error("TML ConfigManager.Save reflection failed. Method signature has changed. Notify Calamity Devs if you see this in your log.");
            }
            catch
            {
                Instance.Logger.Error("An error occurred while manually saving Calamity mod configuration. This may be due to a complex mod conflict. It is safe to ignore this error.");
            }
        }
        #endregion

        #region Boss Kill Times
        private void SetupBossKillTimes()
        {
            // Kill times are measured exactly in frames.
            // 60   frames = 1 second
            // 3600 frames = 1 minute
            bossKillTimes = new SortedDictionary<int, int> {
                //
                // VANILLA BOSSES
                //
                { NPCID.KingSlime, 5400 }, // 1:30 (90 seconds)
                { NPCID.EyeofCthulhu, 5400 }, // 1:30 (90 seconds)
                { NPCID.EaterofWorldsHead, 7200 }, // 2:00 (120 seconds)
                { NPCID.EaterofWorldsBody, 7200 },
                { NPCID.EaterofWorldsTail, 7200 },
                { NPCID.BrainofCthulhu, 7200 }, // 2:00 (120 seconds, total length of fight including Creepers phase)
                { NPCID.Creeper, 1800 }, // 0:30 (30 seconds, length of Creepers phase)
                { NPCID.Deerclops, 5400 }, // 1:30 (90 seconds)
                { NPCID.QueenBee, 7200 }, // 2:00 (120 seconds)
                { NPCID.SkeletronHead, 9000 }, // 2:30 (150 seconds)
                { NPCID.WallofFlesh, 7200 }, // 2:00 (120 seconds)
                { NPCID.WallofFleshEye, 7200 },
                { NPCID.QueenSlimeBoss, 7200 }, // 2:00 (120 seconds)
                { NPCID.Spazmatism, 10800 }, // 3:00 (180 seconds)
                { NPCID.Retinazer, 10800 },
                { NPCID.TheDestroyer, 10800 }, // 3:00 (180 seconds)
                { NPCID.TheDestroyerBody, 10800 },
                { NPCID.TheDestroyerTail, 10800 },
                { NPCID.SkeletronPrime, 10800 }, // 3:00 (180 seconds)
                { NPCID.Plantera, 10800 }, // 3:00 (180 seconds)
                { NPCID.HallowBoss, 10800 }, // 3:00 (180 seconds)
                { NPCID.Golem, 9000 }, // 2:30 (150 seconds)
                { NPCID.GolemHead, 3600 }, // 1:00 (60 seconds)
                { NPCID.DukeFishron, 9000 }, // 2:30 (150 seconds)
                { NPCID.CultistBoss, 9000 }, // 2:30 (150 seconds)
                { NPCID.MoonLordCore, 14400 }, // 4:00 (240 seconds)
                { NPCID.MoonLordHand, 7200 }, // 2:00 (120 seconds)
                { NPCID.MoonLordHead, 7200 }, // 2:00 (120 seconds)

                //
                // CALAMITY BOSSES
                //
                { ModContent.NPCType<DesertScourgeHead>(), 5400 }, // 1:30 (90 seconds)
                { ModContent.NPCType<DesertScourgeBody>(), 5400 },
                { ModContent.NPCType<DesertScourgeTail>(), 5400 },
                { ModContent.NPCType<Crabulon>(), 5400 }, // 1:30 (90 seconds)
                { ModContent.NPCType<HiveMind>(), 7200 }, // 2:00 (120 seconds)
                { ModContent.NPCType<PerforatorHive>(), 7200 }, // 2:00 (120 seconds)
                { ModContent.NPCType<SlimeGodCore>(), 9000 }, // 2:30 (150 seconds) -- total length of Slime God fight
                { ModContent.NPCType<EbonianPaladin>(), 4500 }, // 1:15 (75 seconds)
                { ModContent.NPCType<CrimulanPaladin>(), 4500 }, // 1:15 (75 seconds)
                { ModContent.NPCType<SplitEbonianPaladin>(), 4500 }, // 1:15 (75 seconds) -- split slimes should spawn at 1:15 and die at around 2:30
                { ModContent.NPCType<SplitCrimulanPaladin>(), 4500 }, // 1:15 (75 seconds)
                { ModContent.NPCType<Cryogen>(), 10800 }, // 3:00 (180 seconds)
                { ModContent.NPCType<AquaticScourgeHead>(), 9000 }, // 2:30 (150 seconds)
                { ModContent.NPCType<AquaticScourgeBody>(), 9000 },
                { ModContent.NPCType<AquaticScourgeBodyAlt>(), 9000 },
                { ModContent.NPCType<AquaticScourgeTail>(), 9000 },
                { ModContent.NPCType<BrimstoneElemental>(), 10800 }, // 3:00 (180 seconds)
                { ModContent.NPCType<CalamitasClone>(), 14400 }, // 4:00 (240 seconds)
                { ModContent.NPCType<Anahita>(), 10800 }, // 3:00 (180 seconds)
                { ModContent.NPCType<Leviathan>(), 10800 },
                { ModContent.NPCType<AstrumAureus>(), 10800 }, // 3:00 (180 seconds)
                { ModContent.NPCType<AstrumDeusHead>(), 7200 }, // 2:00 (120 seconds) -- first phase is 1:00
                { ModContent.NPCType<AstrumDeusBody>(), 7200 },
                { ModContent.NPCType<AstrumDeusTail>(), 7200 },
                { ModContent.NPCType<PlaguebringerGoliath>(), 10800 }, // 3:00 (180 seconds)
                { ModContent.NPCType<RavagerBody>(), 10800 }, // 3:00 (180 seconds)
                { ModContent.NPCType<ProfanedGuardianCommander>(), 5400 }, // 1:30 (90 seconds)
                { ModContent.NPCType<Bumblefuck>(), 7200 }, // 2:00 (120 seconds)
                { ModContent.NPCType<Providence>(), 14400 }, // 4:00 (240 seconds)
                { ModContent.NPCType<CeaselessVoid>(), 10800 }, // 3:00 (180 seconds)
                { ModContent.NPCType<DarkEnergy>(), 1200 }, // 0:20 (20 seconds)
                { ModContent.NPCType<StormWeaverHead>(), 8100 }, // 2:15 (135 seconds)
                { ModContent.NPCType<StormWeaverBody>(), 8100 },
                { ModContent.NPCType<StormWeaverTail>(), 8100 },
                { ModContent.NPCType<Signus>(), 7200 }, // 2:00 (120 seconds)
                { ModContent.NPCType<Polterghast>(), 10800 }, // 3:00 (180 seconds)
                { ModContent.NPCType<OldDuke>(), 10800 }, // 3:00 (180 seconds)
                { ModContent.NPCType<DevourerofGodsHead>(), 14400 }, // 4:00 (240 seconds)
                { ModContent.NPCType<DevourerofGodsBody>(), 14400 }, // DoG Phase 1 is 1:30, DoG Phase 2 is 2:30
                { ModContent.NPCType<DevourerofGodsTail>(), 14400 },
                { ModContent.NPCType<Yharon>(), 14700 }, // 4:05 (245 seconds) -- he spends 5 seconds invincible where you can't do anything
                { ModContent.NPCType<Apollo>(), 21600 }, // 6:00 (360 seconds)
                { ModContent.NPCType<Artemis>(), 21600 },
                { ModContent.NPCType<AresBody>(), 21600 }, // 6:00 (360 seconds)
                { ModContent.NPCType<AresGaussNuke>(), 21600 },
                { ModContent.NPCType<AresLaserCannon>(), 21600 },
                { ModContent.NPCType<AresPlasmaFlamethrower>(), 21600 },
                { ModContent.NPCType<AresTeslaCannon>(), 21600 },
                { ModContent.NPCType<ThanatosHead>(), 21600 }, // 6:00 (360 seconds)
                { ModContent.NPCType<ThanatosBody1>(), 21600 },
                { ModContent.NPCType<ThanatosBody2>(), 21600 },
                { ModContent.NPCType<ThanatosTail>(), 21600 },
                { ModContent.NPCType<SupremeCalamitas>(), 18000 }, // 5:00 (300 seconds)
                { ModContent.NPCType<PrimordialWyrmHead>(), 18000 } // 5:00 (300 seconds)
            };
        }
        #endregion

        #region Music

        // This function returns an available Calamity Music Mod track, or null if the Calamity Music Mod is not available.
        public int? GetMusicFromMusicMod(string songFilename) => ExternalMods.MusicAvailable ? MusicLoader.GetMusicSlot(ExternalMods.musicMod, "Sounds/Music/" + songFilename) : null;

        // This function returns an available VCMM track, or null if VCMM is not available.
        // Unlike the main Music Mod, VCMM is hierarchical.
        public int? GetMusicFromVCMM(string songPath) => ExternalMods.VCMMAvailable ? MusicLoader.GetMusicSlot(ExternalMods.vcmm, "Assets/" + songPath) : null;

        #endregion

        #region Mod Support
        public override object Call(params object[] args) => ModCalls.Call(args);
        #endregion

        #region Seasons
        public static Season CurrentSeason
        {
            get
            {
                DateTime date = DateTime.Now;
                int day = date.DayOfYear - Convert.ToInt32(DateTime.IsLeapYear(date.Year) && date.DayOfYear > 59);

                if (day < 80 || day >= 355)
                {
                    return Season.Winter;
                }

                else if (day >= 80 && day < 172)
                {
                    return Season.Spring;
                }

                else if (day >= 172 && day < 266)
                {
                    return Season.Summer;
                }

                else
                {
                    return Season.Fall;
                }
            }
        }
        #endregion

        #region Netcode
        public override void HandlePacket(BinaryReader reader, int whoAmI) => CalamityNetcode.HandlePacket(this, reader, whoAmI);
        #endregion
    }
}
