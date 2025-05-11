using System;
using System.Collections.Generic;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.Dusts;
using CalamityMod.Graphics.Primitives;
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

        public static int Radius => 160;

        public List<Vector2> points = [];

        public List<List<Vector2>> bolts = [];

        public override void SetDefaults()
        {
            Projectile.width = Radius * 2;
            Projectile.height = Radius * 2;
            Projectile.hostile = true;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.alpha = 255;
        }

        public override void OnSpawn(IEntitySource source)
        {
            RecalculateLists();
        }

        public override void AI()
        {
            bool validOwner = true;

            NPC n = Main.npc[(int)Projectile.ai[1] - 1];
            if (n == null || !n.active || n.life < 0)
            {
                validOwner = false;
            }
            else
            {
                Projectile.Center = n.Center;
            }

            if (Projectile.ai[0] % 5 == 0)
            {
                RecalculateLists();
            }
            Projectile.ai[2] = MathHelper.Lerp(0, Radius, Utils.GetLerpValue(0, 10, Projectile.ai[0], true));
            Projectile.ai[0]++;
            if (Projectile.ai[0] % 10 == 0)
            {
                SoundEngine.PlaySound(SoundID.DD2_LightningBugZap with { Pitch = 1.1f }, Projectile.Center);
            }
            Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(Projectile.ai[2], Projectile.ai[2]), DustID.Electric);
            if (Projectile.ai[0] < 40 && validOwner)
            {
                if (Projectile.alpha > 0)
                    Projectile.alpha -= 20;
                if (Projectile.alpha < 0)
                    Projectile.alpha = 0;
            }
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
            points.Clear();
            bolts.Clear();
            for (int i = 0; i < ringPoints; i++)
            {
                // The radius of the aura. Randomized to be electricky
                float rad = Projectile.ai[2] + Main.rand.Next(-20, 20);
                // Determinte the end point
                // This creates a circle with the given radius
                Vector2 end = Vector2.UnitY.RotatedBy(MathHelper.Lerp(0, MathHelper.TwoPi + MathHelper.ToRadians(15), (i + 1) / (float)ringPoints)) * rad;
                points.Add(Projectile.Center + end);
                
                // Randomly create more electric bolts that connect from the center to the ring
                if (Main.rand.NextBool(ringPoints / Main.rand.Next(minimumBolts, maximumBolts)))
                {
                    List<Vector2> bolt = [];
                    Vector2 start = Projectile.Center;
                    Vector2 boltEnd = Projectile.Center + end;

                    bolt.Add(start);
                    for (int j = 0; j < boltPoints; j++)
                    {
                        Vector2 dest = Vector2.Lerp(start, boltEnd, (j + 1) / (float)boltPoints);
                        Vector2 dif = dest - start;
                        Vector2 newPoint = start + dif.RotatedByRandom(MathHelper.ToRadians(10));
                        bolt.Add(newPoint);
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
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            if (info.Damage <= 0)
                return;

            target.AddBuff(ModContent.BuffType<StaticDischarge>(), 60);
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => Projectile.alpha <= 0 && CalamityUtils.CircularHitboxCollision(Projectile.Center, Projectile.ai[2], targetHitbox);

        public override bool PreDraw(ref Color lightColor)
        {
            Main.spriteBatch.EnterShaderRegion();
            GameShaders.Misc["CalamityMod:TeslaTrail"].SetShaderTexture(ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Trails/ZapTrail"));
            float opacity = 0.6f;
            PrimitiveRenderer.RenderTrail(points, new((float completion) => 4, (float completion) => Color.Cyan * Projectile.Opacity * opacity, smoothen: true, shader: GameShaders.Misc["CalamityMod:TeslaTrail"]));
            PrimitiveRenderer.RenderTrail(points, new((float completion) => 1, (float completion) => Color.White * Projectile.Opacity * opacity, smoothen: true, shader: GameShaders.Misc["CalamityMod:TeslaTrail"]));
            for (int i = 0; i < bolts.Count; i++)
            {
                List<Vector2> boltPoints = bolts[i];
                PrimitiveRenderer.RenderTrail(boltPoints, new((float completion) => 3, (float completion) => Color.Cyan * Projectile.Opacity * opacity, smoothen: true, shader: GameShaders.Misc["CalamityMod:TeslaTrail"]));
                PrimitiveRenderer.RenderTrail(boltPoints, new((float completion) => 1, (float completion) => Color.White * Projectile.Opacity * opacity, smoothen: true, shader: GameShaders.Misc["CalamityMod:TeslaTrail"]));
            }
            Main.spriteBatch.ExitShaderRegion();
            return false;
        }
    }
}
