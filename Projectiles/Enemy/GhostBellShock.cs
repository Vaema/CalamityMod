using System.Collections.Generic;
using System.IO;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Graphics.Primitives;
using CalamityMod.NPCs.SunkenSea;
using CalamityMod.Sounds;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Enemy
{
    public class GhostBellShock : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Enemy";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        // Max aura size not accounting for babies
        public static int MaxDefaultRadius => 160;

        // How much does each baby boost the size of the aura
        public static int RadiusBoostPerBaby => 10;

        // Stores the ring points
        public List<Vector2> points = [];

        // Stores the bolts which connect from the center to the ring
        public List<List<Vector2>> bolts = [];

        // Stores the bolts which connect to babies
        public List<List<Vector2>> jellyBolts = [];

        // Generic timer
        public ref float Timer => ref Projectile.ai[0];
        // The jelly
        public ref float Parent => ref Projectile.ai[1];
        // Size of the aura
        public ref float CurrentRadius => ref Projectile.ai[2];
        // How many valid babies exist
        public ref float CurrentBabies => ref Projectile.localAI[0];

        public override void SetDefaults()
        {
            Projectile.width = MaxDefaultRadius * 2;
            Projectile.height = MaxDefaultRadius * 2;
            Projectile.hostile = true;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.alpha = 255;
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            CurrentBabies = reader.ReadSingle();
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(CurrentBabies);
        }

        public override void OnSpawn(IEntitySource source)
        {
            RecalculateLists();
        }

        public override void AI()
        {
            bool validOwner = true;
            int impactMoment = GhostBell.ElectrifyingPhaseDischarge;

            NPC n = Main.npc[(int)Parent - 1];
            if (n == null || !n.active || n.life < 0)
            {
                validOwner = false;
            }
            else
            {
                Projectile.Center = n.Center;
            }
            // Recalculate the prims to animate the lightning
            if (Projectile.ai[0] % 5 == 0)
            {
                RecalculateLists();
            }
            // Scale the aura size. More babies increases the size of the aura
            CurrentRadius = MathHelper.Lerp(0, MaxDefaultRadius + (CurrentBabies * RadiusBoostPerBaby), Utils.GetLerpValue(impactMoment, impactMoment + 10, Timer, true));
            if (Timer == 0 && CurrentBabies > 0)
            {
                SoundEngine.PlaySound(CommonCalamitySounds.LightningSound with { Pitch = 1f }, Projectile.Center);
            }
            Timer++;
            float pitch = (CurrentBabies > 0 && Timer < impactMoment) ? 1f : 0.4f;
            if (Timer % 10 == 0 && (Timer >= impactMoment || CurrentBabies > 0))
            {
                SoundEngine.PlaySound(SoundID.DD2_LightningBugZap with { Pitch = pitch }, Projectile.Center);
            }
            if (Timer == impactMoment)
            {
                SoundEngine.PlaySound(CommonCalamitySounds.LightningSound, Projectile.Center);
            }
            if (CurrentBabies > 0 || Timer >= impactMoment)
                Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(CurrentRadius, CurrentRadius), DustID.Electric);
            // Become opaque 
            if (Timer < (40 + impactMoment) && validOwner)
            {
                if (Projectile.alpha > 0)
                    Projectile.alpha -= 20;
                if (Projectile.alpha < 0)
                    Projectile.alpha = 0;
            }
            // become transparent and die
            else
            {
                Projectile.alpha += 20;
                if (Projectile.alpha >= 255)
                {
                    Projectile.Kill();
                }
            }
        }

        public void RecalculateLists()
        {
            int ringPoints = 60; // How many times can the ring bend
            int boltPoints = 12; // How many times can a bolt bend
            int minimumBolts = 6; // Minimum number of bolts
            int maximumBolts = 12; // Maximum number of bolts
            int minimumSplits = 0; // Minimum number of split bolts
            int maximumSplits = 2; // Maximum number of split bolts
            points.Clear();
            bolts.Clear();
            jellyBolts.Clear();
            for (int i = 0; i < ringPoints; i++)
            {
                // The radius of the aura. Randomized to be electricky
                float rad = CurrentRadius + Main.rand.Next(-20, 20);
                // Determinte the end point
                // This creates a circle with the given radius
                Vector2 end = Vector2.UnitY.RotatedBy(MathHelper.Lerp(0, MathHelper.TwoPi + MathHelper.ToRadians(15), (i + 1) / (float)ringPoints)) * rad;
                points.Add(Projectile.Center + end);
                
                // Randomly create more electric bolts that connect from the center to the ring
                if (Main.rand.NextBool(ringPoints / Main.rand.Next(minimumBolts, maximumBolts)))
                {
                    List<Vector2> bolt = [];
                    Vector2 start = Projectile.Center;
                    Vector2 boltEnd = Projectile.Center + end.RotatedByRandom(MathHelper.PiOver4);

                    bolt.Add(start);
                    for (int j = 0; j < boltPoints; j++)
                    {
                        Vector2 dest = Vector2.Lerp(start, boltEnd, (j + 1) / (float)boltPoints);
                        Vector2 dif = dest - start;
                        Vector2 newPoint = start + dif.RotatedByRandom(MathHelper.ToRadians(10));
                        bolt.Add(newPoint);

                        // UNUSED code that occasionally gives bolts their own smaller bolts
                        /*if (Main.rand.NextBool(boltPoints / Main.rand.Next(minimumSplits + 1, maximumSplits + 1)) && j > boltPoints / 2)
                        {
                            Main.NewText("Made a bolt");
                            List<Vector2> boltSplit = [];
                            boltSplit.Add(newPoint);
                            for (int k = 0; k < boltPoints; k++)
                            {
                                Vector2 destSplit = Vector2.Lerp(newPoint, Projectile.Center + end.RotatedBy(MathHelper.PiOver4), (k + 1) / (float)boltPoints);
                                Vector2 difSplit = destSplit - newPoint;
                                Vector2 newPointSplit = newPoint + difSplit.RotatedByRandom(MathHelper.ToRadians(10));
                                boltSplit.Add(newPointSplit);
                            }
                            boltSplit.Add(Projectile.Center + end.RotatedBy(MathHelper.PiOver4));
                            bolts.Add(boltSplit);
                        }*/
                    }
                    bolt.Add(boltEnd);
                    bolts.Add(bolt);
                }

                // Re-add the first point so that the trail can form a full circle
                if (i == ringPoints - 1)
                {
                    points.Add(points[0]);
                }
            }

            // Count how many baby jellies exist and tie lightning to them
            CurrentBabies = 0;
            foreach (NPC n in Main.ActiveNPCs)
            {
                if (n.type != ModContent.NPCType<BabyGhostBell>())
                    continue;
                if (n.Distance(Projectile.Center) > 400)
                    continue;

                CurrentBabies++;

                List<Vector2> bolt = [];
                Vector2 start = Projectile.Center;
                Vector2 boltEnd = n.Center;

                bolt.Add(start);
                for (int i = 0; i < boltPoints; i++)
                {
                    Vector2 dest = Vector2.Lerp(start, boltEnd, (i + 1) / (float)boltPoints);
                    Vector2 dif = dest - start;
                    Vector2 newPoint = start + dif.RotatedByRandom(MathHelper.ToRadians(4));
                    bolt.Add(newPoint);
                }
                bolt.Add(boltEnd);
                jellyBolts.Add(bolt);
            }

            Projectile.netUpdate = true;
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            if (info.Damage <= 0)
                return;

            target.AddBuff(ModContent.BuffType<StaticDischarge>(), 60);
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => Projectile.alpha <= 0 && CalamityUtils.CircularHitboxCollision(Projectile.Center, CurrentRadius, targetHitbox);

        public override bool PreDraw(ref Color lightColor)
        {
            Main.spriteBatch.EnterShaderRegion();
            GameShaders.Misc["CalamityMod:TeslaTrail"].SetShaderTexture(ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Trails/ZapTrail"));
            float opacity = 0.6f;
            float jellyOpacity = 0.2f;
            if (Timer >= GhostBell.ElectrifyingPhaseDischarge)
            {
                // The aura
                PrimitiveRenderer.RenderTrail(points, new((float completion) => 4, (float completion) => Color.Cyan * Projectile.Opacity * opacity, smoothen: true, shader: GameShaders.Misc["CalamityMod:TeslaTrail"]));
                PrimitiveRenderer.RenderTrail(points, new((float completion) => 1, (float completion) => Color.White * Projectile.Opacity * opacity, smoothen: true, shader: GameShaders.Misc["CalamityMod:TeslaTrail"]));
                // Bolts inside of the aura
                for (int i = 0; i < bolts.Count; i++)
                {
                    List<Vector2> boltPoints = bolts[i];
                    PrimitiveRenderer.RenderTrail(boltPoints, new((float completion) => 3, (float completion) => Color.Cyan * Projectile.Opacity * opacity, smoothen: true, shader: GameShaders.Misc["CalamityMod:TeslaTrail"]));
                    PrimitiveRenderer.RenderTrail(boltPoints, new((float completion) => 1, (float completion) => Color.White * Projectile.Opacity * opacity, smoothen: true, shader: GameShaders.Misc["CalamityMod:TeslaTrail"]));
                }
            }
            // Harmless bolts connecting to babies
            for (int i = 0; i < jellyBolts.Count; i++)
            {
                List<Vector2> boltPoints = jellyBolts[i];
                PrimitiveRenderer.RenderTrail(boltPoints, new((float completion) => 2, (float completion) => Color.Cyan * jellyOpacity, smoothen: true, shader: GameShaders.Misc["CalamityMod:TeslaTrail"]));
                PrimitiveRenderer.RenderTrail(boltPoints, new((float completion) => 1, (float completion) => Color.White * jellyOpacity, smoothen: true, shader: GameShaders.Misc["CalamityMod:TeslaTrail"]));
            }
            Main.spriteBatch.ExitShaderRegion();
            return false;
        }
    }
}
