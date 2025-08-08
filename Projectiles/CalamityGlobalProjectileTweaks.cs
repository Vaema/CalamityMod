using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles
{
    // TODO -- The projectile tweaks database and functions should be stored in a ModSystem.
    // ApplyTweaks(ref Projectile proj) would be the one exposed function, which CalamityGlobalProjectile would call in SetDefaults.
    public partial class CalamityGlobalProjectile : GlobalProjectile
    {
        #region Database and Initialization
        internal static SortedDictionary<int, IProjectileTweak[]> currentTweaks = null;

        internal static void LoadTweaks()
        {
            // Various shorthands for projectiles which receive very simple changes, such as setting one flag.
            IProjectileTweak[] defenseDamage = Do(DefenseDamage);
            IProjectileTweak[] trueMelee = Do(TrueMelee, DefaultIDStaticIFrames); // All the tweaked true melee projectiles need to be changed from global to static as well
            IProjectileTweak[] trueMeleeNoSpeed = Do(TrueMeleeNoSpeed, DefaultIDStaticIFrames);
            IProjectileTweak[] pointBlank = Do(PointBlank);
            IProjectileTweak[] standardBulletTweaks = Do(PointBlank, ExtraUpdatesDelta(+2));
            IProjectileTweak[] standardChainsawTweaks = Do(TrueMeleeNoSpeed, ArmorPenetrationDelta(+15), LocalIFrames(5));
            IProjectileTweak[] standardDrillTweaks = Do(TrueMeleeNoSpeed, ArmorPenetrationDelta(+25), LocalIFrames(5));
            IProjectileTweak[] counterweightTweaks = Do(MaxUpdatesExact(2), DefaultIDStaticIFrames);

            // Shorthand for changing all the stats of a yoyo at once. This handles extra update related math for you.
            // For topSpeed, put in how fast you want the yoyo to be EXACTLY: it will be divided out in extra updates for you.
            static IProjectileTweak[] RebalanceYoyo(float lifetime, float range, float topSpeed, int extraUpdates, int iframes = 10) => new IProjectileTweak[]
            {
                ExtraUpdatesExact(extraUpdates),
                LocalIFrames(iframes * (extraUpdates + 1)),
                YoyoLifetime(lifetime <= 0f ? -1f : lifetime * (extraUpdates + 1)),
                YoyoRange(range),
                YoyoTopSpeed(topSpeed / (extraUpdates + 1)),
            };

            // SORTING NOTES:
            // 1. Sort tweaks by categories first, then sort by the internal name in alphabetical order. Navigate through categories and names using the search function.
            // 2. Higher categories hold priority over lower ones (ie. Balancing with PB tweaks belong in balancing, rather than PB)
            // 3. Ambiguous internal names should have comments for ease of access.
            currentTweaks = new SortedDictionary<int, IProjectileTweak[]>
            {
                #region CATEGORY 1: Vanilla Yoyo Balancing
                // note this is only yoyos, not counterweights

                // original: 15s lifetime | 270px range | 14px/f top speed | 0 extra updates
                { ProjectileID.Amarok, RebalanceYoyo(-1f, 432f, 28f, 1, 12) },

                // original: 13s lifetime | 235px range | 14px/f top speed | 0 extra updates
                { ProjectileID.Cascade, RebalanceYoyo(30f, 384f, 28f, 1, 15) },

                // original: 16s lifetime | 275px range | 17px/f top speed | 0 extra updates
                { ProjectileID.Chik, RebalanceYoyo(-1f, 400f, 32f, 1, 12) },

                // original: 9s lifetime | 220px range | 13px/f top speed | 0 extra updates
                { ProjectileID.Code1, RebalanceYoyo(21f, 320f, 25f, 1, 15) },

                // original: INF lifetime | 280px range | 17px/f top speed | 0 extra updates
                { ProjectileID.Code2, RebalanceYoyo(-1f, 432f, 42f, 1, 12) },

                // original: 7s lifetime | 195px range | 12.5px/f top speed | 0 extra updates
                { ProjectileID.CorruptYoyo, RebalanceYoyo(18f, 288f, 22f, 0, 20) }, // Malaise

                // original: 6s lifetime | 207px range | 12px/f top speed | 0 extra updates
                { ProjectileID.CrimsonYoyo, RebalanceYoyo(18f, 288f, 22f, 0, 20) }, // Artery

                // original: 8s lifetime | 235px range | 15px/f top speed | 0 extra updates
                { ProjectileID.FormatC, RebalanceYoyo(-1f, 384f, 36f, 1, 12) },

                // original: 10s lifetime | 250px range | 12px/f top speed | 0 extra updates
                { ProjectileID.Gradient, RebalanceYoyo(-1f, 384f, 36f, 1, 12) },

                // original: 12s lifetime | 275px range | 15px/f top speed | 0 extra updates
                { ProjectileID.HelFire, RebalanceYoyo(-1f, 352f, 42f, 2, 12) },

                // original: 11s lifetime | 225px range | 14px/f top speed | 0 extra updates
                { ProjectileID.HiveFive, RebalanceYoyo(24f, 320f, 20f, 0, 15) },

                // original: 8s lifetime | 215px range | 13px/f top speed | 0 extra updates
                { ProjectileID.JungleYoyo, RebalanceYoyo(20f, 288f, 17f, 0, 20) }, // Amazon

                // original: INF lifetime | 340px range | 16px/f top speed | 0 extra updates
                { ProjectileID.Kraken, RebalanceYoyo(-1f, 480f, 54f, 2) },

                // original: 5s lifetime | 170px range | 11px/f top speed | 0 extra updates
                { ProjectileID.Rally, RebalanceYoyo(16f, 272f, 20f, 0, 20) },

                // original: INF lifetime | 370px range | 16px/f top speed | 0 extra updates
                { ProjectileID.RedsYoyo, RebalanceYoyo(-1f, 480f, 42f, 2, 12) }, // Red's Throw

                // original: INF lifetime | 400px range | 17.5px/f top speed | 0 extra updates
                { ProjectileID.Terrarian, RebalanceYoyo(-1f, 512f, 54f, 2) },
                // 12AUG2023: Ozzatron: Terrarian has been IL edited to not emit more orb spawns with extra updates. This iframe change is safe.
                { ProjectileID.TerrarianBeam, Do(LocalIFrames(-1)) }, // Terrarian yoyo orbs

                // original: INF lifetime | 360px range | 16.5px/f top speed | 0 extra updates
                { ProjectileID.TheEyeOfCthulhu, RebalanceYoyo(-1f, 480f, 36f, 1, 12) }, // the yoyo, of course

                // original: INF lifetime | 370px range | 16px/f top speed | 0 extra updates
                { ProjectileID.ValkyrieYoyo, RebalanceYoyo(-1f, 480f, 42f, 2, 12) },

                // original: 11s lifetime | 225px range | 14px/f top speed | 0 extra updates
                { ProjectileID.Valor, RebalanceYoyo(30f, 400f, 36f, 1, 15) },

                // original: 3s lifetime | 130px range | 9px/f top speed | 0 extra updates
                { ProjectileID.WoodYoyo, RebalanceYoyo(15f, 240f, 14f, 0, 20) },

                // original: 14s lifetime | 290px range | 16px/f top speed | 0 extra updates
                { ProjectileID.Yelets, RebalanceYoyo(-1f, 400f, 36f, 1, 12) },
                #endregion

                #region CATEGORY 2: Weapon/Enemy Balancing
                { ProjectileID.AdamantiteChainsaw, standardChainsawTweaks },
                { ProjectileID.AdamantiteDrill, standardDrillTweaks },
                { ProjectileID.AdamantiteGlaive, Do(TrueMelee, LocalIFrames(7)) },
                { ProjectileID.Anchor, Do(ExtraUpdatesExact(1), DefaultIDStaticIFrames) },
                { ProjectileID.Arkhalis, Do(TrueMeleeNoSpeed, ScaleExact(1.25f), IDStaticIFrames(5)) }, // Has an exception in Vanilla iframe code, uses 5 iframes
                { ProjectileID.Bee, Do(PiercingExact(2), DefaultIDStaticIFrames) },
                { ProjectileID.BeeArrow, Do(PointBlank, ExtraUpdatesExact(1), DefaultIDStaticIFrames) },
                { ProjectileID.BlackCounterweight, counterweightTweaks },
                { ProjectileID.BlueCounterweight, counterweightTweaks },
                { ProjectileID.BlueMoon, Do(ExtraUpdatesExact(1)) },
                { ProjectileID.Bullet, Do(PointBlank, ExtraUpdatesDelta(+2), DefaultIDStaticIFrames) },
                { ProjectileID.BulletHighVelocity, Do(PointBlank, LocalIFrames(-1)) },
                { ProjectileID.ButchersChainsaw, Do(TrueMeleeNoSpeed, ArmorPenetrationDelta(+15), LocalIFrames(7), ScaleExact(1.5f)) },
                { ProjectileID.ChlorophyteChainsaw, standardChainsawTweaks },
                { ProjectileID.ChlorophyteDrill, standardDrillTweaks },
                { ProjectileID.ChlorophyteOrb, Do(NoPiercing) },
                { ProjectileID.CobaltChainsaw, standardChainsawTweaks },
                { ProjectileID.CobaltDrill, standardDrillTweaks },
                { ProjectileID.CobaltNaginata, Do(TrueMelee, LocalIFrames(9)) },
                { ProjectileID.CrystalBullet, Do(PointBlank, ExtraUpdatesDelta(+2), DefaultIDStaticIFrames) },
                { ProjectileID.CrystalVileShardHead, Do(LocalIFrames(23)) },
                { ProjectileID.CrystalVileShardShaft, Do(LocalIFrames(23)) },
                { ProjectileID.CursedBullet, Do(PointBlank, ExtraUpdatesDelta(+2), DefaultIDStaticIFrames) },
                { ProjectileID.ClusterRocketI, Do(LocalIFrames(15)) },
                { ProjectileID.ClusterFragmentsI, Do(IDStaticIFrames(15)) },
                { ProjectileID.ClusterRocketII, Do(LocalIFrames(15)) },
                { ProjectileID.ClusterFragmentsII, Do(IDStaticIFrames(15)) },
                { ProjectileID.ClusterSnowmanRocketI, Do(LocalIFrames(15)) },
                { ProjectileID.ClusterSnowmanRocketII, Do(LocalIFrames(15)) },
                { ProjectileID.DangerousSpider, Do( ExtraUpdatesExact(2), LocalIFrames(45)) }, //Spider Staff spiders. It has Venom, Dangerous, and Jumping spiders.
                { ProjectileID.DD2SquireSonicBoom, Do(PiercingExact(3), DefaultIDStaticIFrames) }, // Flying Dragon
                { ProjectileID.DeadlySphere, Do(LocalIFrames(30)) },
                { ProjectileID.EmeraldBolt, Do(NoPiercing) },
                { ProjectileID.EmpressBlade, Do(LocalIFrames(30)) }, // Terraprisma
                { ProjectileID.EnchantedBoomerang, Do(ExtraUpdatesExact(1), DefaultIDStaticIFrames) },
                { ProjectileID.ExplosiveBullet, Do(PointBlank, ExtraUpdatesDelta(+2), IDStaticIFrames(5)) }, // Has an exception in Vanilla iframe code, uses 5 iframes
                { ProjectileID.FairyQueenRangedItemShot, Do(PiercingExact(7), ExtraUpdatesExact(1))  }, // Eventide Convert
                { ProjectileID.FlaironBubble, Do(ExtraUpdatesExact(1), TimeLeftExact(150), DefaultIDStaticIFrames) },
                { ProjectileID.Flamarang, Do(ExtraUpdatesExact(2), DefaultIDStaticIFrames) },
                { ProjectileID.Flames, Do(IDStaticIFrames(5)) }, // Flamethrower AND Elf Melter flames
                { ProjectileID.FlamingJack, Do(ExtraUpdatesExact(1), DefaultIDStaticIFrames) },
                { ProjectileID.FlowerPetal, Do(MaxUpdatesExact(4), LocalIFrames(10)) }, // Orichalcum armor
                { ProjectileID.FlyingKnife, Do(ExtraUpdatesExact(1), DefaultIDStaticIFrames) },
                { ProjectileID.FrostBoltStaff, Do(ExtraUpdatesExact(1), DefaultIDStaticIFrames) },
                { ProjectileID.FruitcakeChakram, Do(ExtraUpdatesExact(1), DefaultIDStaticIFrames) },
                { ProjectileID.GiantBee, Do(PiercingExact(2), DefaultIDStaticIFrames) },
                { ProjectileID.GladiusStab, Do(TrueMelee, LocalIFrames(-1)) },
                { ProjectileID.GoldenBullet, Do(PointBlank, ExtraUpdatesDelta(+2), DefaultIDStaticIFrames) },
                { ProjectileID.GoldenShowerFriendly, Do(PiercingExact(2), DefaultIDStaticIFrames) },
                { ProjectileID.GreenCounterweight, counterweightTweaks },
                { ProjectileID.Hamdrax, standardDrillTweaks }, // Drax (never internally renamed since 1.1)
                { ProjectileID.IceBoomerang, Do(ExtraUpdatesExact(1), DefaultIDStaticIFrames) },
                { ProjectileID.IceSickle, Do(ExtraUpdatesExact(1)) },
                { ProjectileID.IchorBullet, Do(PointBlank, ExtraUpdatesDelta(+2), DefaultIDStaticIFrames) },
                { ProjectileID.InfluxWaver, Do(ExtraUpdatesExact(1)) },
                { ProjectileID.InfernoFriendlyBolt, Do(ExtraUpdatesExact(1), DefaultIDStaticIFrames) },
                { ProjectileID.InfernoFriendlyBlast, Do(ExtraUpdatesExact(2), DefaultIDStaticIFrames) },
                { ProjectileID.JumperSpider, Do( ExtraUpdatesExact(2), LocalIFrames(45)) }, //Spider Staff spiders. It has Venom, Dangerous, and Jumping spiders.
                { ProjectileID.LaserDrill, Do(ArmorPenetrationDelta(+25), LocalIFrames(5)) },
                { ProjectileID.LightDisc, Do(MaxUpdatesExact(3), DefaultIDStaticIFrames) },
                { ProjectileID.LostSoulHostile, Do(TileCollide) }, // Ragged Caster
                { ProjectileID.MeteorShot, standardBulletTweaks },
                { ProjectileID.Meowmere, Do(PiercingExact(3), LocalIFrames(-1)) },
                { ProjectileID.MonkStaffT1, Do(TrueMeleeNoSpeed, ScaleExact(3f)) }, // Sleepy Octopod
                { ProjectileID.MonkStaffT2, Do(TrueMelee, IDStaticIFrames(18)) }, // Ghastly Glaive
                { ProjectileID.MonkStaffT3, Do(ScaleRatio(2f)) }, // Sky Dragon's Fury
                { ProjectileID.MoonlordBullet, standardBulletTweaks }, // Luminite Bullet
                { ProjectileID.MythrilChainsaw, standardChainsawTweaks },
                { ProjectileID.MythrilDrill, standardDrillTweaks },
                { ProjectileID.MythrilHalberd, Do(TrueMelee, LocalIFrames(8)) },
                { ProjectileID.NanoBullet, Do(PointBlank, ExtraUpdatesDelta(+2), DefaultIDStaticIFrames) },
                { ProjectileID.NebulaDrill, standardDrillTweaks },
                { ProjectileID.NebulaLaser, Do(ExtraUpdatesDelta(-1)) },
                { ProjectileID.OrichalcumChainsaw, standardChainsawTweaks },
                { ProjectileID.OrichalcumDrill, standardDrillTweaks },
                { ProjectileID.PalladiumChainsaw, standardChainsawTweaks },
                { ProjectileID.PalladiumDrill, standardDrillTweaks },
                { ProjectileID.PartyBullet, Do(PointBlank, ExtraUpdatesDelta(+2), DefaultIDStaticIFrames) },
                { ProjectileID.PoisonFang, Do(LocalIFrames(10)) },
                { ProjectileID.PurpleCounterweight, counterweightTweaks },
                { ProjectileID.QueenSlimeGelAttack, Do(NoPiercing) },
                { ProjectileID.QueenSlimeMinionPinkBall, Do(NoPiercing) },
                { ProjectileID.RedCounterweight, counterweightTweaks },
                { ProjectileID.RocketFireworkBlue, Do(TimeLeftDelta(+45)) },
                { ProjectileID.RocketFireworkGreen, Do(TimeLeftDelta(+45)) },
                { ProjectileID.RocketFireworkRed, Do(TimeLeftDelta(+45)) },
                { ProjectileID.RocketFireworkYellow, Do(TimeLeftDelta(+45)) },
                { ProjectileID.SawtoothShark, Do(TrueMeleeNoSpeed, ArmorPenetrationDelta(+15), LocalIFrames(6)) },
                { ProjectileID.Shroomerang, Do(ExtraUpdatesExact(1), DefaultIDStaticIFrames) },
                { ProjectileID.SolarFlareDrill, standardDrillTweaks },
                { ProjectileID.StardustDrill, standardDrillTweaks },
                { ProjectileID.Starfury, Do(TimeLeftExact(75), DefaultIDStaticIFrames) },
                { ProjectileID.StarWrath, Do(NoPiercing) },
                { ProjectileID.Sunfury, Do(ExtraUpdatesExact(1)) },
                { ProjectileID.SwordBeam, Do(ExtraUpdatesExact(2), DefaultIDStaticIFrames) }, // Beam Sword projectile
                { ProjectileID.Terragrim, Do(TrueMeleeNoSpeed, ScaleExact(1.25f), IDStaticIFrames(5)) }, // Has an exception in Vanilla iframe code, uses 5 iframes
                { ProjectileID.ThunderStaffShot, Do(PiercingExact(3), DefaultIDStaticIFrames) }, //Thunder Zapper projectile
                { ProjectileID.TitaniumChainsaw, standardChainsawTweaks },
                { ProjectileID.TitaniumDrill, standardDrillTweaks },
                { ProjectileID.Trimarang, Do(ExtraUpdatesExact(1), DefaultIDStaticIFrames) },
                { ProjectileID.TrueNightsEdge, Do(PiercingExact(4)) },
                { ProjectileID.VenomBullet, Do(PointBlank, ExtraUpdatesDelta(+2), DefaultIDStaticIFrames) },
                { ProjectileID.VenomFang, Do(LocalIFrames(10)) },
                { ProjectileID.VenomSpider, Do( ExtraUpdatesExact(2), LocalIFrames(45)) }, //Spider Staff spiders. It has Venom, Dangerous, and Jumping spiders.
                { ProjectileID.VortexDrill, standardDrillTweaks },
                { ProjectileID.Wasp, Do(PiercingExact(2)) },
                { ProjectileID.WeatherPainShot, Do(ExtraUpdatesExact(3), TimeLeftExact(1920)) },
                { ProjectileID.YellowCounterweight, counterweightTweaks },
                #endregion

                #region CATEGORY 3: True Melee support
                { ProjectileID.ChlorophyteJackhammer, trueMeleeNoSpeed },
                { ProjectileID.CopperShortswordStab, trueMelee },
                { ProjectileID.DarkLance, trueMelee },
                { ProjectileID.GoldShortswordStab, trueMelee },
                { ProjectileID.Gungnir, trueMelee },
                { ProjectileID.HallowJoustingLance, trueMelee },
                { ProjectileID.IronShortswordStab, trueMelee },
                { ProjectileID.JoustingLance, trueMelee },
                { ProjectileID.LeadShortswordStab, trueMelee },
                { ProjectileID.MushroomSpear, trueMelee },
                { ProjectileID.NebulaChainsaw, trueMeleeNoSpeed },
                { ProjectileID.ObsidianSwordfish, trueMelee },
                { ProjectileID.OrichalcumHalberd, trueMelee },
                { ProjectileID.PalladiumPike, trueMelee },
                { ProjectileID.PiercingStarlight, Do(TrueMelee, IDStaticIFrames(4)) }, // Has an exception in Vanilla iframe code, uses 4 iframes
                { ProjectileID.PlatinumShortswordStab, trueMelee },
                { ProjectileID.RulerStab, trueMelee },
                { ProjectileID.ShadowJoustingLance, trueMelee },
                { ProjectileID.SilverShortswordStab, trueMelee },
                { ProjectileID.SolarFlareChainsaw, trueMeleeNoSpeed },
                { ProjectileID.Spear, trueMelee },
                { ProjectileID.StardustChainsaw, trueMeleeNoSpeed },
                { ProjectileID.Swordfish, trueMelee },
                { ProjectileID.TheRottedFork, trueMelee },
                { ProjectileID.TinShortswordStab, trueMelee },
                { ProjectileID.TitaniumTrident, trueMelee },
                { ProjectileID.Trident, trueMelee },
                { ProjectileID.TungstenShortswordStab, trueMelee },
                { ProjectileID.VortexChainsaw, trueMeleeNoSpeed },
                #endregion

                #region CATEGORY 4: Point Blank support
                { ProjectileID.Blizzard, Do(PointBlank, DefaultIDStaticIFrames) }, // Blizzard Staff projectiles, re-used in Frostbite Blaster.
                { ProjectileID.BlueFlare, Do(PointBlank, DefaultIDStaticIFrames) },
                { ProjectileID.BoneArrow, Do(PointBlank, DefaultIDStaticIFrames) },
                { ProjectileID.CandyCorn, Do(PointBlank, IDStaticIFrames(7)) }, // Has an exception in Vanilla iframe code, uses 7 iframes
                { ProjectileID.ChlorophyteArrow, Do(PointBlank, DefaultIDStaticIFrames) },
                { ProjectileID.ChlorophyteBullet, Do(PointBlank, DefaultIDStaticIFrames) },
                { ProjectileID.CrimsandBallGun, Do(PointBlank, DefaultIDStaticIFrames) },
                { ProjectileID.CrystalDart, Do(PointBlank, DefaultIDStaticIFrames) },
                { ProjectileID.CursedArrow, Do(PointBlank, DefaultIDStaticIFrames) },
                { ProjectileID.CursedDart, Do(PointBlank, DefaultIDStaticIFrames) },
                { ProjectileID.DD2PhoenixBowShot, pointBlank }, // Phantom Phoenix
                { ProjectileID.EbonsandBallGun, Do(PointBlank, DefaultIDStaticIFrames) },
                { ProjectileID.FireArrow, Do(PointBlank, DefaultIDStaticIFrames) },
                { ProjectileID.Flare, Do(PointBlank, DefaultIDStaticIFrames) },
                { ProjectileID.FrostburnArrow, Do(PointBlank, DefaultIDStaticIFrames) },
                { ProjectileID.Harpoon, pointBlank },
                { ProjectileID.Hellwing, Do(PointBlank, DefaultIDStaticIFrames) },
                { ProjectileID.HellfireArrow, Do(PointBlank, ExtraUpdatesDelta(+2)) },
                { ProjectileID.HolyArrow, Do(PointBlank, DefaultIDStaticIFrames) },
                { ProjectileID.IchorArrow, Do(PointBlank, DefaultIDStaticIFrames) },
                { ProjectileID.JestersArrow, Do(PointBlank, DefaultIDStaticIFrames) },
                { ProjectileID.MoonlordArrow, pointBlank }, // Luminite Arrow
                { ProjectileID.PainterPaintball, Do(PointBlank, DefaultIDStaticIFrames) },
                { ProjectileID.PearlSandBallGun, Do(PointBlank, DefaultIDStaticIFrames) },
                { ProjectileID.PhantasmArrow, Do(PointBlank, DefaultIDStaticIFrames) },
                { ProjectileID.PoisonDartBlowgun, Do(PointBlank, DefaultIDStaticIFrames) },
                { ProjectileID.PulseBolt, Do(PointBlank, DefaultIDStaticIFrames) },
                { ProjectileID.SandBallGun, Do(PointBlank, DefaultIDStaticIFrames) },
                { ProjectileID.Seed, Do(PointBlank, DefaultIDStaticIFrames) },
                { ProjectileID.ShadowFlameArrow, Do(PointBlank, DefaultIDStaticIFrames) },
                { ProjectileID.SnowBallFriendly, Do(PointBlank, DefaultIDStaticIFrames) },
                { ProjectileID.Stake, Do(PointBlank, DefaultIDStaticIFrames) },
                { ProjectileID.UnholyArrow, Do(PointBlank, DefaultIDStaticIFrames) },
                { ProjectileID.VenomArrow, Do(PointBlank, DefaultIDStaticIFrames) },
                { ProjectileID.WoodenArrowFriendly, Do(PointBlank, DefaultIDStaticIFrames) },
                #endregion

                #region CATEGORY 5: Defense Damage support
                { ProjectileID.Cthulunado, defenseDamage }, // Duke Fishron's larger Sharknados
                { ProjectileID.DD2BetsyFlameBreath, defenseDamage },
                { ProjectileID.DeerclopsIceSpike, defenseDamage },
                { ProjectileID.FairyQueenSunDance, defenseDamage }, // Empress of Light's Sun Dance
                { ProjectileID.FlamingScythe, defenseDamage }, // Pumpking orange spinning scythes
                { ProjectileID.InfernoHostileBlast, defenseDamage }, // Diabolist inferno fork explosions
                { ProjectileID.PaladinsHammerHostile, defenseDamage },
                { ProjectileID.PhantasmalDeathray, defenseDamage },
                { ProjectileID.PhantasmalSphere, defenseDamage },
                { ProjectileID.SaucerDeathray, defenseDamage },
                { ProjectileID.Sharknado, defenseDamage },
                { ProjectileID.ThornBall, Do(Main.zenithWorld ? IgnoreWater : DontIgnoreWater, DefenseDamage) }, // Plantera bouncing thorn balls
                #endregion

                #region CATEGORY 6: ID-Static Immunity Frame changes
                {ProjectileID.AbigailCounter, Do(DefaultIDStaticIFrames)},
                {ProjectileID.Ale, Do(DefaultIDStaticIFrames)},
                {ProjectileID.AmberBolt, Do(DefaultIDStaticIFrames)},
                {ProjectileID.AmethystBolt, Do(DefaultIDStaticIFrames)},
                {ProjectileID.AshBallFalling, Do(DefaultIDStaticIFrames)},
                {ProjectileID.BallofFire, Do(DefaultIDStaticIFrames)},
                {ProjectileID.BallofFrost, Do(DefaultIDStaticIFrames)},
                {ProjectileID.Bananarang, Do(DefaultIDStaticIFrames)},
                {ProjectileID.Bat, Do(DefaultIDStaticIFrames)},
                {ProjectileID.BeeHive, Do(DefaultIDStaticIFrames)},
                {ProjectileID.Beenade, Do(DefaultIDStaticIFrames)},
                {ProjectileID.BlackCat, Do(DefaultIDStaticIFrames)},
                {ProjectileID.BloodArrow, Do(DefaultIDStaticIFrames)},
                {ProjectileID.BloodButcherer, Do(DefaultIDStaticIFrames)},
                {ProjectileID.BloodNautilusTears, Do(DefaultIDStaticIFrames)},
                {ProjectileID.BloodWater, Do(DefaultIDStaticIFrames)},
                {ProjectileID.BloodyMachete, Do(DefaultIDStaticIFrames)},
                {ProjectileID.Bomb, Do(DefaultIDStaticIFrames)},
                {ProjectileID.BombFish, Do(DefaultIDStaticIFrames)},
                {ProjectileID.Bone, Do(DefaultIDStaticIFrames)},
                {ProjectileID.BoneArrowFromMerchant, Do(DefaultIDStaticIFrames)},
                {ProjectileID.BoneDagger, Do(DefaultIDStaticIFrames)},
                {ProjectileID.BookOfSkullsSkull, Do(DefaultIDStaticIFrames)},
                {ProjectileID.BookStaffShot, Do(DefaultIDStaticIFrames)},
                {ProjectileID.Boulder, Do(DefaultIDStaticIFrames)},
                {ProjectileID.BoulderStaffOfEarth, Do(DefaultIDStaticIFrames)},
                {ProjectileID.BouncyBomb, Do(DefaultIDStaticIFrames)},
                {ProjectileID.BouncyBoulder, Do(DefaultIDStaticIFrames)},
                {ProjectileID.BouncyDynamite, Do(DefaultIDStaticIFrames)},
                {ProjectileID.BouncyGrenade, Do(DefaultIDStaticIFrames)},
                {ProjectileID.BoxingGlove, Do(DefaultIDStaticIFrames)},
                {ProjectileID.Bubble, Do(DefaultIDStaticIFrames)},
                {ProjectileID.Bunny, Do(DefaultIDStaticIFrames)},
                {ProjectileID.CannonballFriendly, Do(DefaultIDStaticIFrames)},
                {ProjectileID.CavelingGardener, Do(DefaultIDStaticIFrames)},
                {ProjectileID.Celeb2Rocket, Do(DefaultIDStaticIFrames)},
                {ProjectileID.Celeb2RocketExplosive, Do(DefaultIDStaticIFrames)},
                {ProjectileID.Celeb2RocketExplosiveLarge, Do(DefaultIDStaticIFrames)},
                {ProjectileID.Celeb2RocketLarge, Do(DefaultIDStaticIFrames)},
                {ProjectileID.Celeb2Weapon, Do(DefaultIDStaticIFrames)},
                {ProjectileID.ChainGuillotine, Do(DefaultIDStaticIFrames)},
                {ProjectileID.ChainKnife, Do(DefaultIDStaticIFrames)},
                {ProjectileID.ChargedBlasterCannon, Do(DefaultIDStaticIFrames)},
                {ProjectileID.ChargedBlasterLaser, Do(DefaultIDStaticIFrames)},
                {ProjectileID.ChargedBlasterOrb, Do(DefaultIDStaticIFrames)},
                {ProjectileID.ChlorophytePartisan, Do(DefaultIDStaticIFrames)},
                {ProjectileID.ClothiersCurse, Do(DefaultIDStaticIFrames)},
                {ProjectileID.ClusterMineI, Do(DefaultIDStaticIFrames)},
                {ProjectileID.ClusterMineII, Do(DefaultIDStaticIFrames)},
                {ProjectileID.ClusterSnowmanFragmentsI, Do(DefaultIDStaticIFrames)},
                {ProjectileID.ClusterSnowmanFragmentsII, Do(DefaultIDStaticIFrames)},
                {ProjectileID.CoinPortal, Do(DefaultIDStaticIFrames)},
                {ProjectileID.CopperCoin, Do(DefaultIDStaticIFrames)},
                {ProjectileID.CorruptSpray, Do(DefaultIDStaticIFrames)},
                {ProjectileID.CrimsandBallFalling, Do(DefaultIDStaticIFrames)},
                {ProjectileID.CrimsonHeart, Do(DefaultIDStaticIFrames)},
                {ProjectileID.CrimsonSpray, Do(DefaultIDStaticIFrames)},
                {ProjectileID.CrystalLeaf, Do(DefaultIDStaticIFrames)},
                {ProjectileID.CrystalLeafShot, Do(DefaultIDStaticIFrames)},
                {ProjectileID.CrystalPulse, Do(DefaultIDStaticIFrames)},
                {ProjectileID.CrystalPulse2, Do(DefaultIDStaticIFrames)},
                {ProjectileID.CrystalShard, Do(DefaultIDStaticIFrames)},
                {ProjectileID.CrystalStorm, Do(DefaultIDStaticIFrames)},
                {ProjectileID.CursedDartFlame, Do(DefaultIDStaticIFrames)},
                {ProjectileID.CursedFlameFriendly, Do(DefaultIDStaticIFrames)},
                {ProjectileID.CursedFlare, Do(DefaultIDStaticIFrames)},
                {ProjectileID.DD2PhoenixBow, Do(DefaultIDStaticIFrames)},
                {ProjectileID.DemonScythe, Do(DefaultIDStaticIFrames)},
                {ProjectileID.DiamondBolt, Do(DefaultIDStaticIFrames)},
                {ProjectileID.DirtBall, Do(DefaultIDStaticIFrames)},
                {ProjectileID.DirtBomb, Do(DefaultIDStaticIFrames)},
                {ProjectileID.DirtSpray, Do(DefaultIDStaticIFrames)},
                {ProjectileID.DirtStickyBomb, Do(DefaultIDStaticIFrames)},
                {ProjectileID.DripplerFlailExtraBall, Do(DefaultIDStaticIFrames)},
                {ProjectileID.DryadsWardCircle, Do(DefaultIDStaticIFrames)},
                {ProjectileID.DryBomb, Do(DefaultIDStaticIFrames)},
                {ProjectileID.DryMine, Do(DefaultIDStaticIFrames)},
                {ProjectileID.DryRocket, Do(DefaultIDStaticIFrames)},
                {ProjectileID.DrySnowmanRocket, Do(DefaultIDStaticIFrames)},
                {ProjectileID.Dynamite, Do(DefaultIDStaticIFrames)},
                {ProjectileID.EatersBite, Do(DefaultIDStaticIFrames)},
                {ProjectileID.EbonsandBallFalling, Do(DefaultIDStaticIFrames)},
                {ProjectileID.EighthNote, Do(DefaultIDStaticIFrames)},
                {ProjectileID.Electrosphere, Do(IDStaticIFrames(8))}, // Has an exception in Vanilla iframe code, uses 8 iframes
                {ProjectileID.ElectrosphereMissile, Do(DefaultIDStaticIFrames)},
                {ProjectileID.EnchantedBeam, Do(DefaultIDStaticIFrames)},
                {ProjectileID.Explosives, Do(DefaultIDStaticIFrames)},
                {ProjectileID.FallingStar, Do(DefaultIDStaticIFrames)},
                {ProjectileID.Flairon, Do(DefaultIDStaticIFrames)},
                {ProjectileID.FlamesTrap, Do(DefaultIDStaticIFrames)},
                {ProjectileID.FlowerPowPetal, Do(DefaultIDStaticIFrames)},
                {ProjectileID.FrostArrow, Do(DefaultIDStaticIFrames)},
                {ProjectileID.FrostDaggerfish, Do(DefaultIDStaticIFrames)},
                {ProjectileID.GasTrap, Do(DefaultIDStaticIFrames)},
                {ProjectileID.GelBalloon, Do(DefaultIDStaticIFrames)},
                {ProjectileID.Geode, Do(DefaultIDStaticIFrames)},
                {ProjectileID.GeyserTrap, Do(DefaultIDStaticIFrames)},
                {ProjectileID.GoldCoin, Do(DefaultIDStaticIFrames)},
                {ProjectileID.GolemFist, Do(DefaultIDStaticIFrames)},
                {ProjectileID.GreenLaser, Do(DefaultIDStaticIFrames)},
                {ProjectileID.Grenade, Do(DefaultIDStaticIFrames)},
                {ProjectileID.HallowSpray, Do(DefaultIDStaticIFrames)},
                {ProjectileID.HallowStar, Do(DefaultIDStaticIFrames)},
                {ProjectileID.HeatRay, Do(DefaultIDStaticIFrames)},
                {ProjectileID.HolyWater, Do(DefaultIDStaticIFrames)},
                {ProjectileID.HoneyBomb, Do(DefaultIDStaticIFrames)},
                {ProjectileID.HoneyGrenade, Do(DefaultIDStaticIFrames)},
                {ProjectileID.HoneyMine, Do(DefaultIDStaticIFrames)},
                {ProjectileID.HoneyRocket, Do(DefaultIDStaticIFrames)},
                {ProjectileID.HoneySnowmanRocket, Do(DefaultIDStaticIFrames)},
                {ProjectileID.HornetStinger, Do(DefaultIDStaticIFrames)},
                {ProjectileID.HoundiusShootiusFireball, Do(DefaultIDStaticIFrames)},
                {ProjectileID.IceBlock, Do(DefaultIDStaticIFrames)},
                {ProjectileID.IceBolt, Do(DefaultIDStaticIFrames)},
                {ProjectileID.IchorDart, Do(DefaultIDStaticIFrames)},
                {ProjectileID.JackOLantern, Do(DefaultIDStaticIFrames)},
                {ProjectileID.JavelinFriendly, Do(DefaultIDStaticIFrames)},
                {ProjectileID.Landmine, Do(DefaultIDStaticIFrames)},
                {ProjectileID.LaserMachinegun, Do(DefaultIDStaticIFrames)},
                {ProjectileID.LaserMachinegunLaser, Do(DefaultIDStaticIFrames)},
                {ProjectileID.LastPrism, Do(DefaultIDStaticIFrames)},
                {ProjectileID.LastPrismLaser, Do(IDStaticIFrames(5))}, // Has an exception in Vanilla iframe code, uses 5 iframes
                {ProjectileID.LavaBomb, Do(DefaultIDStaticIFrames)},
                {ProjectileID.LavaMine, Do(DefaultIDStaticIFrames)},
                {ProjectileID.LavaRocket, Do(DefaultIDStaticIFrames)},
                {ProjectileID.LavaSnowmanRocket, Do(DefaultIDStaticIFrames)},
                {ProjectileID.Leaf, Do(DefaultIDStaticIFrames)},
                {ProjectileID.LifeCrystalBoulder, Do(DefaultIDStaticIFrames)},
                {ProjectileID.MagicMissile, Do(IDStaticIFrames(8))}, // Has an exception in Vanilla iframe code, uses 8 iframes
                {ProjectileID.MagnetSphereBolt, Do(DefaultIDStaticIFrames)},
                {ProjectileID.MedusaHead, Do(DefaultIDStaticIFrames)},
                {ProjectileID.MedusaHeadRay, Do(DefaultIDStaticIFrames)},
                {ProjectileID.Meteor1, Do(IDStaticIFrames(5))}, // Has an exception in Vanilla iframe code, uses 5 iframes
                {ProjectileID.Meteor2, Do(IDStaticIFrames(5))}, // Has an exception in Vanilla iframe code, uses 5 iframes
                {ProjectileID.Meteor3, Do(IDStaticIFrames(5))}, // Has an exception in Vanilla iframe code, uses 5 iframes
                {ProjectileID.MinecartMechLaser, Do(DefaultIDStaticIFrames)},
                {ProjectileID.MiniBoulder, Do(DefaultIDStaticIFrames)},
                {ProjectileID.MiniMinotaur, Do(DefaultIDStaticIFrames)},
                {ProjectileID.MiniNukeMineI, Do(DefaultIDStaticIFrames)},
                {ProjectileID.MiniNukeMineII, Do(DefaultIDStaticIFrames)},
                {ProjectileID.MiniNukeRocketI, Do(DefaultIDStaticIFrames)},
                {ProjectileID.MiniNukeRocketII, Do(DefaultIDStaticIFrames)},
                {ProjectileID.MiniNukeSnowmanRocketI, Do(DefaultIDStaticIFrames)},
                {ProjectileID.MiniNukeSnowmanRocketII, Do(DefaultIDStaticIFrames)},
                {ProjectileID.MiniRetinaLaser, Do(DefaultIDStaticIFrames)},
                {ProjectileID.MiniSharkron, Do(DefaultIDStaticIFrames)},
                {ProjectileID.MolotovCocktail, Do(DefaultIDStaticIFrames)},
                {ProjectileID.MolotovFire, Do(DefaultIDStaticIFrames)},
                {ProjectileID.MolotovFire2, Do(DefaultIDStaticIFrames)},
                {ProjectileID.MolotovFire3, Do(DefaultIDStaticIFrames)},
                {ProjectileID.MonkStaffT3_AltShot, Do(DefaultIDStaticIFrames)},
                {ProjectileID.MudBall, Do(DefaultIDStaticIFrames)},
                {ProjectileID.Mushroom, Do(DefaultIDStaticIFrames)},
                {ProjectileID.MushroomSpray, Do(DefaultIDStaticIFrames)},
                {ProjectileID.NailFriendly, Do(IDStaticIFrames(1))}, // Has an exception in Vanilla iframe code, uses 1 iframe
                {ProjectileID.NebulaArcanumExplosionShot, Do(DefaultIDStaticIFrames)},
                {ProjectileID.NebulaArcanumExplosionShotShard, Do(DefaultIDStaticIFrames)},
                {ProjectileID.NebulaBlaze1, Do(IDStaticIFrames(5))}, // Has an exception in Vanilla iframe code, uses 5 iframes
                {ProjectileID.NebulaBlaze2, Do(IDStaticIFrames(5))}, // Has an exception in Vanilla iframe code, uses 5 iframes
                {ProjectileID.NettleBurstEnd, Do(DefaultIDStaticIFrames)},
                {ProjectileID.NettleBurstLeft, Do(DefaultIDStaticIFrames)},
                {ProjectileID.NettleBurstRight, Do(DefaultIDStaticIFrames)},
                {ProjectileID.NightBeam, Do(DefaultIDStaticIFrames)},
                {ProjectileID.NorthPoleSnowflake, Do(DefaultIDStaticIFrames)},
                {ProjectileID.NorthPoleSpear, Do(DefaultIDStaticIFrames)},
                {ProjectileID.NorthPoleWeapon, Do(DefaultIDStaticIFrames)},
                {ProjectileID.NurseSyringeHeal, Do(DefaultIDStaticIFrames)},
                {ProjectileID.NurseSyringeHurt, Do(DefaultIDStaticIFrames)},
                {ProjectileID.OrnamentFriendly, Do(DefaultIDStaticIFrames)},
                {ProjectileID.OrnamentStar, Do(DefaultIDStaticIFrames)},
                {ProjectileID.PaladinsHammerFriendly, Do(DefaultIDStaticIFrames)},
                {ProjectileID.PaperAirplaneA, Do(DefaultIDStaticIFrames)},
                {ProjectileID.PaperAirplaneB, Do(DefaultIDStaticIFrames)},
                {ProjectileID.PartyGirlGrenade, Do(DefaultIDStaticIFrames)},
                {ProjectileID.PearlSandBallFalling, Do(DefaultIDStaticIFrames)},
                {ProjectileID.PewMaticHornShot, Do(DefaultIDStaticIFrames)},
                {ProjectileID.Phantasm, Do(DefaultIDStaticIFrames)},
                {ProjectileID.PineNeedleFriendly, Do(DefaultIDStaticIFrames)},
                {ProjectileID.PlatinumCoin, Do(DefaultIDStaticIFrames)},
                {ProjectileID.PoisonDart, Do(DefaultIDStaticIFrames)},
                {ProjectileID.PoisonDartTrap, Do(DefaultIDStaticIFrames)},
                {ProjectileID.PoisonedKnife, Do(DefaultIDStaticIFrames)},
                {ProjectileID.PossessedHatchet, Do(DefaultIDStaticIFrames)},
                {ProjectileID.PrincessWeapon, Do(DefaultIDStaticIFrames)},
                {ProjectileID.ProximityMineI, Do(DefaultIDStaticIFrames)},
                {ProjectileID.ProximityMineII, Do(DefaultIDStaticIFrames)},
                {ProjectileID.ProximityMineIII, Do(DefaultIDStaticIFrames)},
                {ProjectileID.ProximityMineIV, Do(DefaultIDStaticIFrames)},
                {ProjectileID.PureSpray, Do(DefaultIDStaticIFrames)},
                {ProjectileID.PurificationPowder, Do(DefaultIDStaticIFrames)},
                {ProjectileID.PurpleLaser, Do(DefaultIDStaticIFrames)},
                {ProjectileID.PygmySpear, Do(DefaultIDStaticIFrames)},
                {ProjectileID.QuarterNote, Do(DefaultIDStaticIFrames)},
                {ProjectileID.RainbowFlare, Do(DefaultIDStaticIFrames)},
                {ProjectileID.RocketFireworksBoxBlue, Do(DefaultIDStaticIFrames)},
                {ProjectileID.RocketFireworksBoxGreen, Do(DefaultIDStaticIFrames)},
                {ProjectileID.RocketFireworksBoxRed, Do(DefaultIDStaticIFrames)},
                {ProjectileID.RocketFireworksBoxYellow, Do(DefaultIDStaticIFrames)},
                {ProjectileID.RocketI, Do(DefaultIDStaticIFrames)},
                {ProjectileID.RocketII, Do(DefaultIDStaticIFrames)},
                {ProjectileID.RocketIII, Do(DefaultIDStaticIFrames)},
                {ProjectileID.RocketIV, Do(DefaultIDStaticIFrames)},
                {ProjectileID.RocketSnowmanI, Do(DefaultIDStaticIFrames)},
                {ProjectileID.RocketSnowmanII, Do(DefaultIDStaticIFrames)},
                {ProjectileID.RocketSnowmanIII, Do(DefaultIDStaticIFrames)},
                {ProjectileID.RocketSnowmanIV, Do(DefaultIDStaticIFrames)},
                {ProjectileID.RollingCactus, Do(DefaultIDStaticIFrames)},
                {ProjectileID.RollingCactusSpike, Do(DefaultIDStaticIFrames)},
                {ProjectileID.RottenEgg, Do(DefaultIDStaticIFrames)},
                {ProjectileID.RubyBolt, Do(DefaultIDStaticIFrames)},
                {ProjectileID.SandBallFalling, Do(DefaultIDStaticIFrames)},
                {ProjectileID.SandSpray, Do(DefaultIDStaticIFrames)},
                {ProjectileID.SantaBombs, Do(DefaultIDStaticIFrames)},
                {ProjectileID.SantankMountRocket, Do(DefaultIDStaticIFrames)},
                {ProjectileID.SapphireBolt, Do(DefaultIDStaticIFrames)},
                {ProjectileID.ScarabBomb, Do(DefaultIDStaticIFrames)},
                {ProjectileID.ScutlixLaser, Do(DefaultIDStaticIFrames)},
                {ProjectileID.ScutlixLaserFriendly, Do(DefaultIDStaticIFrames)},
                {ProjectileID.SeedlerNut, Do(DefaultIDStaticIFrames)},
                {ProjectileID.SeedlerThorn, Do(DefaultIDStaticIFrames)},
                {ProjectileID.ShadowBeamFriendly, Do(DefaultIDStaticIFrames)},
                {ProjectileID.ShadowFlameKnife, Do(DefaultIDStaticIFrames)},
                {ProjectileID.ShellPileFalling, Do(DefaultIDStaticIFrames)},
                {ProjectileID.ShimmerArrow, Do(DefaultIDStaticIFrames)},
                {ProjectileID.ShimmerFlare, Do(DefaultIDStaticIFrames)},
                {ProjectileID.Shuriken, Do(DefaultIDStaticIFrames)},
                {ProjectileID.SiltBall, Do(DefaultIDStaticIFrames)},
                {ProjectileID.SilverBullet, Do(DefaultIDStaticIFrames)},
                {ProjectileID.SilverCoin, Do(DefaultIDStaticIFrames)},
                {ProjectileID.SkyFracture, Do(DefaultIDStaticIFrames)},
                {ProjectileID.SlushBall, Do(DefaultIDStaticIFrames)},
                {ProjectileID.SnowSpray, Do(DefaultIDStaticIFrames)},
                {ProjectileID.SolarCounter, Do(DefaultIDStaticIFrames)},
                {ProjectileID.SolarFlareRay, Do(DefaultIDStaticIFrames)},
                {ProjectileID.SoulDrain, Do(DefaultIDStaticIFrames)},
                {ProjectileID.SpearTrap, Do(DefaultIDStaticIFrames)},
                {ProjectileID.SpelunkerFlare, Do(DefaultIDStaticIFrames)},
                {ProjectileID.Spider, Do(DefaultIDStaticIFrames)},
                {ProjectileID.SpiderEgg, Do(DefaultIDStaticIFrames)},
                {ProjectileID.SpikyBall, Do(DefaultIDStaticIFrames)},
                {ProjectileID.SpikyBallTrap, Do(DefaultIDStaticIFrames)},
                {ProjectileID.SpiritFlame, Do(IDStaticIFrames(5))}, // Has an exception in Vanilla iframe code, uses 5 iframes
                {ProjectileID.SporeGas, Do(DefaultIDStaticIFrames)},
                {ProjectileID.SporeGas2, Do(DefaultIDStaticIFrames)},
                {ProjectileID.SporeGas3, Do(DefaultIDStaticIFrames)},
                {ProjectileID.SporeTrap, Do(DefaultIDStaticIFrames)},
                {ProjectileID.SporeTrap2, Do(DefaultIDStaticIFrames)},
                {ProjectileID.StarAnise, Do(DefaultIDStaticIFrames)},
                {ProjectileID.StarCannonStar, Do(DefaultIDStaticIFrames)},
                {ProjectileID.StardustCellMinion, Do(DefaultIDStaticIFrames)},
                {ProjectileID.StardustGuardianExplosion, Do(DefaultIDStaticIFrames)},
                {ProjectileID.StickyBomb, Do(DefaultIDStaticIFrames)},
                {ProjectileID.StickyDynamite, Do(DefaultIDStaticIFrames)},
                {ProjectileID.StickyGrenade, Do(DefaultIDStaticIFrames)},
                {ProjectileID.StormTigerGem, Do(DefaultIDStaticIFrames)},
                {ProjectileID.Stynger, Do(IDStaticIFrames(7))}, // Has an exception in Vanilla iframe code, uses 7 iframes
                {ProjectileID.StyngerShrapnel, Do(DefaultIDStaticIFrames)},
                {ProjectileID.TentacleSpike, Do(DefaultIDStaticIFrames)},
                {ProjectileID.ThornChakram, Do(DefaultIDStaticIFrames)},
                {ProjectileID.ThrowingKnife, Do(DefaultIDStaticIFrames)},
                {ProjectileID.ThunderSpear, Do(DefaultIDStaticIFrames)},
                {ProjectileID.ThunderSpearShot, Do(DefaultIDStaticIFrames)},
                {ProjectileID.TiedEighthNote, Do(DefaultIDStaticIFrames)},
                {ProjectileID.TitaniumStormShard, Do(DefaultIDStaticIFrames)},
                {ProjectileID.TopazBolt, Do(DefaultIDStaticIFrames)},
                {ProjectileID.ToxicBubble, Do(DefaultIDStaticIFrames)},
                {ProjectileID.ToxicCloud, Do(DefaultIDStaticIFrames)},
                {ProjectileID.ToxicCloud2, Do(DefaultIDStaticIFrames)},
                {ProjectileID.ToxicCloud3, Do(DefaultIDStaticIFrames)},
                {ProjectileID.ToxicFlask, Do(DefaultIDStaticIFrames)},
                {ProjectileID.Truffle, Do(DefaultIDStaticIFrames)},
                {ProjectileID.TruffleSpore, Do(DefaultIDStaticIFrames)},
                {ProjectileID.Typhoon, Do(IDStaticIFrames(6))}, // Has an exception in Vanilla iframe code, uses 6 iframes
                {ProjectileID.UFOLaser, Do(DefaultIDStaticIFrames)},
                {ProjectileID.UFOMinion, Do(DefaultIDStaticIFrames)},
                {ProjectileID.UnholyTridentFriendly, Do(DefaultIDStaticIFrames)},
                {ProjectileID.UnholyWater, Do(DefaultIDStaticIFrames)},
                {ProjectileID.VampireKnife, Do(DefaultIDStaticIFrames)},
                {ProjectileID.VenomDartTrap, Do(DefaultIDStaticIFrames)},
                {ProjectileID.ViciousPowder, Do(DefaultIDStaticIFrames)},
                {ProjectileID.VilePowder, Do(DefaultIDStaticIFrames)},
                {ProjectileID.VilethornBase, Do(DefaultIDStaticIFrames)},
                {ProjectileID.VilethornTip, Do(DefaultIDStaticIFrames)},
                {ProjectileID.VortexBeater, Do(DefaultIDStaticIFrames)},
                {ProjectileID.VortexBeaterRocket, Do(DefaultIDStaticIFrames)},
                {ProjectileID.VortexVortexLightning, Do(DefaultIDStaticIFrames)},
                {ProjectileID.VortexVortexPortal, Do(DefaultIDStaticIFrames)},
                {ProjectileID.Waffle, Do(DefaultIDStaticIFrames)},
                {ProjectileID.WandOfFrostingFrost, Do(DefaultIDStaticIFrames)},
                {ProjectileID.WandOfSparkingSpark, Do(DefaultIDStaticIFrames)},
                {ProjectileID.WaterBolt, Do(DefaultIDStaticIFrames)},
                {ProjectileID.WaterStream, Do(DefaultIDStaticIFrames)},
                {ProjectileID.Web, Do(DefaultIDStaticIFrames)},
                {ProjectileID.WetBomb, Do(DefaultIDStaticIFrames)},
                {ProjectileID.WetMine, Do(DefaultIDStaticIFrames)},
                {ProjectileID.WetRocket, Do(DefaultIDStaticIFrames)},
                {ProjectileID.WetSnowmanRocket, Do(DefaultIDStaticIFrames)},
                {ProjectileID.Wisp, Do(DefaultIDStaticIFrames)},
                {ProjectileID.WoodenBoomerang, Do(DefaultIDStaticIFrames)},
                {ProjectileID.Xenopopper, Do(DefaultIDStaticIFrames)},
                {ProjectileID.ZapinatorLaser, Do(DefaultIDStaticIFrames)},
                {ProjectileID.ZoologistStrikeGreen, Do(DefaultIDStaticIFrames)},
                {ProjectileID.ZoologistStrikeRed, Do(DefaultIDStaticIFrames)},

                #endregion
                
            };
        }

        internal static void UnloadTweaks()
        {
            currentTweaks?.Clear();
            currentTweaks = null;
        }
        #endregion

        #region SetDefaults (Projectile Tweaks Applied Here)
        internal static void SetDefaults_ApplyTweaks(Projectile proj)
        {
            // Do nothing if the tweaks database is not defined.
            if (currentTweaks is null)
                return;

            // Grab the tweaking or balancing to apply, if any. If nothing comes back, do nothing.
            bool needsTweaking = currentTweaks.TryGetValue(proj.type, out IProjectileTweak[] tweaks);
            if (!needsTweaking)
                return;

            // Apply all alterations sequentially, assuming they are relevant.
            foreach (IProjectileTweak tweak in tweaks)
                if (tweak.AppliesTo(proj))
                    tweak.ApplyTweak(proj);
        }
        #endregion

        #region Internal Structures

        // This function simply concatenates a bunch of Projectile Tweaks into an array.
        // It looks a lot nicer than constantly typing "new IProjectileTweak[]".
        internal static IProjectileTweak[] Do(params IProjectileTweak[] r) => r;

        // Only one applicability lambda.
        internal static bool IsAYoyo(Projectile proj) => proj.aiStyle == ProjAIStyleID.Yoyo;

        #region Projectile Tweak Definitions
        internal interface IProjectileTweak
        {
            bool AppliesTo(Projectile proj);
            void ApplyTweak(Projectile proj);
        }

        #region Built-In Armor Penetration
        internal class ArmorPenetrationDeltaRule : IProjectileTweak
        {
            internal readonly int delta = 0;

            public ArmorPenetrationDeltaRule(int d) => delta = d;
            public bool AppliesTo(Projectile proj) => true;
            public void ApplyTweak(Projectile proj) => proj.ArmorPenetration += delta;
        }
        internal static IProjectileTweak ArmorPenetrationDelta(int d) => new ArmorPenetrationDeltaRule(d);

        internal class ArmorPenetrationExactRule : IProjectileTweak
        {
            internal readonly int armorPen = 0;

            public ArmorPenetrationExactRule(int a) => armorPen = a;
            public bool AppliesTo(Projectile proj) => true;
            public void ApplyTweak(Projectile proj) => proj.ArmorPenetration = armorPen;
        }
        internal static IProjectileTweak ArmorPenetrationExact(int a) => new ArmorPenetrationExactRule(a);
        #endregion

        #region Defense Damage
        internal class DefenseDamageRule : IProjectileTweak
        {
            internal readonly bool flag = true;

            public DefenseDamageRule(bool dd) => flag = dd;
            public bool AppliesTo(Projectile proj) => true;
            public void ApplyTweak(Projectile proj) => proj.Calamity().DealsDefenseDamage = flag;
        }
        internal static IProjectileTweak DefenseDamage => new DefenseDamageRule(true);
        internal static IProjectileTweak NoDefenseDamage => new DefenseDamageRule(false);
        #endregion

        #region Extra Updates
        internal class ExtraUpdatesDeltaRule : IProjectileTweak
        {
            internal readonly int delta = 0;

            public ExtraUpdatesDeltaRule(int d) => delta = d;
            public bool AppliesTo(Projectile proj) => true;
            public void ApplyTweak(Projectile proj)
            {
                proj.extraUpdates += delta;
                if (proj.extraUpdates < 0)
                    proj.extraUpdates = 0;
            }
        }
        internal static IProjectileTweak ExtraUpdatesDelta(int d) => new ExtraUpdatesDeltaRule(d);

        internal class ExtraUpdatesExactRule : IProjectileTweak
        {
            internal readonly int newExtraUpdates = 0;

            public ExtraUpdatesExactRule(int eu) => newExtraUpdates = eu;
            public bool AppliesTo(Projectile proj) => true;
            public void ApplyTweak(Projectile proj)
            {
                proj.extraUpdates = newExtraUpdates;
                if (proj.extraUpdates < 0)
                    proj.extraUpdates = 0;
            }
        }
        internal static IProjectileTweak ExtraUpdatesExact(int eu) => new ExtraUpdatesExactRule(eu);

        // The MaxUpdates property is sometimes used in favor of the raw extraUpdates field.
        // Both are supported by Calamity Global Projectile Tweaks.
        internal class MaxUpdatesExactRule : IProjectileTweak
        {
            internal readonly int newMaxUpdates = 0;

            public MaxUpdatesExactRule(int mu) => newMaxUpdates = mu;
            public bool AppliesTo(Projectile proj) => true;
            public void ApplyTweak(Projectile proj)
            {
                proj.MaxUpdates = newMaxUpdates;
                if (proj.extraUpdates < 0)
                    proj.extraUpdates = 0;
            }
        }
        internal static IProjectileTweak MaxUpdatesExact(int mu) => new MaxUpdatesExactRule(mu);
        #endregion

        #region ID-Static Immunity Frames
        internal class IDStaticIFrameRule : IProjectileTweak
        {
            internal readonly int idStaticIFrameValue = -2;

            public IDStaticIFrameRule(int f) => idStaticIFrameValue = f;
            public bool AppliesTo(Projectile proj) => true;
            public void ApplyTweak(Projectile proj)
            {
                proj.usesLocalNPCImmunity = false;
                proj.localNPCHitCooldown = -2;
                proj.usesIDStaticNPCImmunity = true;
                proj.idStaticNPCHitCooldown = idStaticIFrameValue;
            }
        }
        internal static IProjectileTweak IDStaticIFrames(int f) => new IDStaticIFrameRule(f);
        internal static IProjectileTweak DefaultIDStaticIFrames => new IDStaticIFrameRule(10);
        #endregion

        #region Ignore Water
        internal class IgnoreWaterRule : IProjectileTweak
        {
            internal readonly bool flag = true;

            public IgnoreWaterRule(bool iw) => flag = iw;
            public bool AppliesTo(Projectile proj) => true;
            public void ApplyTweak(Projectile proj) => proj.ignoreWater = flag;
        }
        internal static IProjectileTweak IgnoreWater => new IgnoreWaterRule(true);
        internal static IProjectileTweak DontIgnoreWater => new IgnoreWaterRule(false);
        #endregion

        #region Local Immunity Frames
        internal class LocalIFrameRule : IProjectileTweak
        {
            internal readonly int localIFrameValue = -2;

            public LocalIFrameRule(int f) => localIFrameValue = f;
            public bool AppliesTo(Projectile proj) => true;
            public void ApplyTweak(Projectile proj)
            {
                proj.usesLocalNPCImmunity = true;
                proj.localNPCHitCooldown = localIFrameValue;
                proj.usesIDStaticNPCImmunity = false;
                proj.idStaticNPCHitCooldown = 0;
            }
        }
        internal static IProjectileTweak LocalIFrames(int f) => new LocalIFrameRule(f);
        internal static IProjectileTweak LocalIFramesOneHit = new LocalIFrameRule(-1);
        #endregion

        #region Piercing
        internal class PiercingDeltaRule : IProjectileTweak
        {
            internal readonly int delta = 0;

            public PiercingDeltaRule(int d) => delta = d;
            public bool AppliesTo(Projectile proj) => true;
            public void ApplyTweak(Projectile proj)
            {
                proj.penetrate += delta;
                if (proj.penetrate < 1)
                    proj.penetrate = 1;
                proj.maxPenetrate = proj.penetrate;
            }
        }
        internal static IProjectileTweak PiercingDelta(int p) => new PiercingDeltaRule(p);

        internal class PiercingExactRule : IProjectileTweak
        {
            internal readonly int newPenetrate = -1;

            public PiercingExactRule(int p) => newPenetrate = p;
            public bool AppliesTo(Projectile proj) => true;
            public void ApplyTweak(Projectile proj)
            {
                proj.penetrate = newPenetrate;
                if (proj.penetrate == 0)
                    proj.penetrate = 1;
                proj.maxPenetrate = proj.penetrate;
            }
        }
        internal static IProjectileTweak PiercingExact(int p) => new PiercingExactRule(p);
        internal static IProjectileTweak NoPiercing = new PiercingExactRule(1);
        internal static IProjectileTweak InfinitePiercing = new PiercingExactRule(-1);
        #endregion

        #region Point Blank
        internal class PointBlankRule : IProjectileTweak
        {
            public bool AppliesTo(Projectile proj) => true;
            public void ApplyTweak(Projectile proj)
                => proj.Calamity().pointBlankShotDuration = DefaultPointBlankDuration;
        }
        internal static IProjectileTweak PointBlank => new PointBlankRule();
        #endregion

        #region Scale
        internal class ScaleDeltaRule : IProjectileTweak
        {
            internal readonly float delta = 0;

            public ScaleDeltaRule(float d) => delta = d;
            public bool AppliesTo(Projectile proj) => true;
            public void ApplyTweak(Projectile proj)
            {
                proj.scale += delta;
                if (proj.scale < 0f)
                    proj.scale = 0f;
            }
        }
        internal static IProjectileTweak ScaleDelta(float d) => new ScaleDeltaRule(d);

        internal class ScaleExactRule : IProjectileTweak
        {
            internal readonly float newScale = 0;

            public ScaleExactRule(float s) => newScale = s;
            public bool AppliesTo(Projectile proj) => true;
            public void ApplyTweak(Projectile proj)
            {
                proj.scale = newScale;
                if (proj.scale < 0f)
                    proj.scale = 0f;
            }
        }
        internal static IProjectileTweak ScaleExact(float s) => new ScaleExactRule(s);

        internal class ScaleRatioRule : IProjectileTweak
        {
            internal readonly float ratio = 1f;

            public ScaleRatioRule(float f) => ratio = f;
            public bool AppliesTo(Projectile proj) => true;
            public void ApplyTweak(Projectile proj)
            {
                proj.scale *= ratio;
                if (proj.scale < 0f)
                    proj.scale = 0f;
            }
        }
        internal static IProjectileTweak ScaleRatio(float f) => new ScaleRatioRule(f);
        #endregion

        #region Tile Collide
        internal class TileCollideRule : IProjectileTweak
        {
            internal readonly bool flag = true;

            public TileCollideRule(bool tc) => flag = tc;
            public bool AppliesTo(Projectile proj) => true;
            public void ApplyTweak(Projectile proj) => proj.tileCollide = flag;
        }
        internal static IProjectileTweak TileCollide => new TileCollideRule(true);
        internal static IProjectileTweak NoTileCollide => new TileCollideRule(false);
        #endregion

        #region Time Left
        internal class TimeLeftDeltaRule : IProjectileTweak
        {
            internal readonly int delta = 0;

            public TimeLeftDeltaRule(int d) => delta = d;
            public bool AppliesTo(Projectile proj) => true;
            public void ApplyTweak(Projectile proj)
            {
                proj.timeLeft += delta;
                if (proj.timeLeft < 1)
                    proj.timeLeft = 1;
            }
        }
        internal static IProjectileTweak TimeLeftDelta(int d) => new TimeLeftDeltaRule(d);

        internal class TimeLeftExactRule : IProjectileTweak
        {
            internal readonly int newTimeLeft = 0;

            public TimeLeftExactRule(int t) => newTimeLeft = t;
            public bool AppliesTo(Projectile proj) => true;
            public void ApplyTweak(Projectile proj)
            {
                proj.timeLeft = newTimeLeft;
                if (proj.timeLeft < 1)
                    proj.timeLeft = 1;
            }
        }
        internal static IProjectileTweak TimeLeftExact(int t) => new TimeLeftExactRule(t);
        #endregion

        #region True Melee
        internal class TrueMeleeRule : IProjectileTweak
        {
            public bool AppliesTo(Projectile proj) => true;
            public void ApplyTweak(Projectile proj) => proj.DamageType = TrueMeleeDamageClass.Instance;
        }
        internal static IProjectileTweak TrueMelee => new TrueMeleeRule();

        internal class TrueMeleeNoSpeedRule : IProjectileTweak
        {
            public bool AppliesTo(Projectile proj) => true;
            public void ApplyTweak(Projectile proj) => proj.DamageType = TrueMeleeNoSpeedDamageClass.Instance;
        }
        internal static IProjectileTweak TrueMeleeNoSpeed => new TrueMeleeNoSpeedRule();
        #endregion

        #region Yoyo Stats
        internal class YoyoLifetimeRule : IProjectileTweak
        {
            internal readonly float newLifetime = -1f; // -1 is unlimited. Otherwise it's the lifetime in seconds

            public YoyoLifetimeRule(float l) => newLifetime = l;
            public bool AppliesTo(Projectile proj) => IsAYoyo(proj);
            public void ApplyTweak(Projectile proj) => ProjectileID.Sets.YoyosLifeTimeMultiplier[proj.type] = newLifetime;
        }
        internal static IProjectileTweak YoyoLifetime(float l) => new YoyoLifetimeRule(l);

        internal class YoyoRangeRule : IProjectileTweak
        {
            internal readonly float newMaxRange = 0f; // Range is measured in pixels

            public YoyoRangeRule(float r) => newMaxRange = r;
            public bool AppliesTo(Projectile proj) => IsAYoyo(proj);
            public void ApplyTweak(Projectile proj) => ProjectileID.Sets.YoyosMaximumRange[proj.type] = newMaxRange;
        }
        internal static IProjectileTweak YoyoRange(float r) => new YoyoRangeRule(r);

        internal class YoyoTopSpeedRule : IProjectileTweak
        {
            internal readonly float newTopSpeed = 0f;

            public YoyoTopSpeedRule(float s) => newTopSpeed = s;
            public bool AppliesTo(Projectile proj) => IsAYoyo(proj);
            public void ApplyTweak(Projectile proj) => ProjectileID.Sets.YoyosTopSpeed[proj.type] = newTopSpeed;
        }
        internal static IProjectileTweak YoyoTopSpeed(float r) => new YoyoTopSpeedRule(r);
        #endregion
        #endregion
        #endregion
    }
}
