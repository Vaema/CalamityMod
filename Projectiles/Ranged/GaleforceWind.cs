using System;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Dusts;
using CalamityMod.Items.Weapons.Ranged;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Ranged
{
    public class GaleforceWind : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Ranged";

        public Vector2 destination = Vector2.Zero;

        public PrimitiveTrail TrailDrawer = null;
        public Vector2[] TrailPositions = new Vector2[10];
        public float[] TrailWaveMultipliers = new float[10];
        public int direction = Main.rand.NextBool() ? -1 : 1;

        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 10;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 10;
            Projectile.height = 10;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 5;
            Projectile.timeLeft = 43;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 12;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            Projectile.localAI[0]++;
            // Home in on origin point.
            //Vector2 homeDirection = (destination - Projectile.Center).SafeNormalize(Vector2.UnitY);
            //Projectile.velocity = (Projectile.velocity * 12f + homeDirection * 20f) / (12f + 1f);

            Projectile.velocity = Projectile.velocity.RotatedBy(MathHelper.Pi / 20 * direction);

            //if (Projectile.velocity.Length() < 20f)
            //{
            //    Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitY) * 20f;
            //}
        }
        public override bool? CanHitNPC(NPC target)
        {
            if (target == Main.npc[(int)Projectile.ai[0]] && Projectile.localAI[0] < 12)
                return false;

            return true;
        }
        public override void Kill(int timeLeft)
        {
            for (int i = 0; i < 6; i++)
                Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, 91, 0f, 0f, 0, default, 0.5f);
        }
        public Color TrailColor(float completionRatio)
        {
            float opacity = Utils.GetLerpValue(0.7f, 0.3f, completionRatio, true) * Projectile.Opacity;
            Color color = Color.White;
            color.A = (byte)(int)(Utils.GetLerpValue(0.7f, 0f, completionRatio) * 64);
            return color;
        }

        public float TrailWidth(float completionRatio)
        {
            float widthInterpolant = Utils.GetLerpValue(0f, 0.25f, completionRatio, true) * Utils.GetLerpValue(0.7f, 0.9f, completionRatio, true);
            return MathHelper.SmoothStep(3f, 10f, widthInterpolant);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Projectile.rotation = Projectile.velocity.ToRotation() - MathHelper.PiOver2;

            if (TrailDrawer is null)
                TrailDrawer = new PrimitiveTrail(TrailWidth, TrailColor, null, GameShaders.Misc["CalamityMod:GaleforceArrowTrail"]);

            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition + Vector2.UnitY * Projectile.gfxOffY + ((Projectile.rotation - MathHelper.PiOver2).ToRotationVector2() * Projectile.height * 1.5f);
            Vector2 origin = texture.Size() * 0.5f;

            for (int i = -1; i <= 1; i += 2)
            {
                float wiggleSeed = (float)Math.Sin((-MathHelper.Pi / 6) + (Projectile.localAI[0] * MathHelper.Pi / 3));

                for (int oldPositions = 0; oldPositions < 10; oldPositions++)
                {
                    if (oldPositions == 0)
                    {
                        TrailWaveMultipliers[0] = wiggleSeed;
                    }
                    else
                    {
                        TrailWaveMultipliers[10 - oldPositions] = TrailWaveMultipliers[10 - oldPositions - 1];
                    }

                    if (Projectile.oldPos[oldPositions] != Vector2.Zero)
                    {
                        TrailPositions[oldPositions] = Projectile.oldPos[oldPositions] + ((Projectile.oldRot[oldPositions] + MathHelper.PiOver2 + (MathHelper.Pi / 2.5f * i)).ToRotationVector2() * 2f * (float)Math.Log(2 * oldPositions + 1, 1.36d) * TrailWaveMultipliers[oldPositions]);
                    }
                }
                Main.spriteBatch.EnterShaderRegion();
                GameShaders.Misc["CalamityMod:GaleforceArrowTrail"].SetShaderTexture(ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Trails/ScarletDevilStreak"));
                GameShaders.Misc["CalamityMod:GaleforceArrowTrail"].Apply();

                TrailDrawer.Draw(TrailPositions, Projectile.Size * 0.5f - Main.screenPosition, 6);
                Main.spriteBatch.ExitShaderRegion();

                Main.EntitySpriteDraw(texture, drawPosition, null, Projectile.GetAlpha(Color.White), Projectile.rotation, origin, Projectile.scale, 0, 0);
            }

            return false;
        }
    }
}
