using System.Collections.Generic;
using CalamityMod.Graphics.Primitives;
using CalamityMod.NPCs.ExoMechs.Ares;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Typeless
{
    public class AuricLandMineExplosion : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Typeless";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public List<List<Vector2>> lightningTrails = new List<List<Vector2>>();
        public static int lightningCount = 15;
        public static int totalPoints = 10;

        public override void SetDefaults()
        {
            Projectile.width = 500;
            Projectile.height = 500;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 20;
            Projectile.tileCollide = false;
            Projectile.hostile = true;
            Projectile.friendly = true;
        }

        public override void AI()
        {
            if (Projectile.ai[0] % 4 == 0)
            {
                SoundEngine.PlaySound(AresGaussNuke.NukeExplosionSound);
                lightningTrails.Clear();
                for (int i = 0; i < lightningCount; i++)
                {
                    List<Vector2> points = new List<Vector2>();
                    for (int j = 0; j < totalPoints; j++)
                    {
                        float radians = MathHelper.TwoPi / lightningCount;
                        if (j == 0)
                        {
                            points.Add(Projectile.Center + Main.rand.NextVector2Circular(20, 20));
                        }
                        else
                        {
                            Vector2 newPoint = new Vector2();
                            Vector2 jtolookfor = j > 1 ? points[j - 2] : Projectile.Center;
                            float baseDist = j == totalPoints - 1 ? 20 : Main.rand.Next(60, 120) * (1 + (20 - Projectile.timeLeft) / 15);
                            newPoint = points[j - 1] + (jtolookfor.DirectionTo(points[j - 1]) * baseDist).RotatedByRandom(MathHelper.PiOver2);
                            points.Add(newPoint);
                        }
                    }
                    lightningTrails.Add(points);
                }
            }
            Projectile.ai[0]++;
            Projectile.damage = 40000; // fixed damage 
            Projectile.CritChance = 0;

            // D O Y O U L I K E D U S T B O Y O 
            for (int l = 0; l < 20; l++)
            {
                Vector2 rand = Vector2.UnitX.RotatedByRandom(MathHelper.TwoPi);
                int extraDust = Dust.NewDust(Projectile.Center, 0, 0, DustID.Electric, 0, 0, 150, default, 1.2f);
                Main.dust[extraDust].velocity = rand * Main.rand.NextFloat(-40, 40f);
            }
        }

        public override bool CanHitPlayer(Player target)
        {
            var yeetVec = Vector2.Normalize(target.Center - Projectile.Center);
            target.velocity += yeetVec * (target.noKnockback ? 20f : 40f);
            return true;
        }

        internal float WidthFunction(float completionRatio) => MathHelper.Clamp(CalamityUtils.Convert01To010(completionRatio * 2), 0.2f, 1f) * 4f;
        internal Color ColorFunction(float completionRatio) => new Color(123, 205, 237); // Auric blue

        internal float BackgroundWidthFunction(float completionRatio) => WidthFunction(completionRatio) * 2f;
        internal Color BackgroundColorFunction(float completionRatio) => ColorFunction(completionRatio) * 0.5f;

        public override bool PreDraw(ref Color lightColor)
        {
            if (lightningTrails.Count <= 0)
                return false;

            Main.spriteBatch.EnterShaderRegion(BlendState.Additive);
            GameShaders.Misc["CalamityMod:TeslaTrail"].SetShaderTexture(ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Trails/ZapTrail"));
            foreach (List<Vector2> points in lightningTrails)
            {
                PrimitiveRenderer.RenderTrail(points, new(WidthFunction, ColorFunction, smoothen: false, shader: GameShaders.Misc["CalamityMod:TeslaTrail"]), 60);
                PrimitiveRenderer.RenderTrail(points, new(BackgroundWidthFunction, BackgroundColorFunction, smoothen: false, shader: GameShaders.Misc["CalamityMod:TeslaTrail"]), 60);
            }

            Main.spriteBatch.ExitShaderRegion();
            return false;
        }
    }
}
