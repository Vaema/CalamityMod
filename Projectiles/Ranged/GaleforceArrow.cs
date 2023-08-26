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
using System.Collections.Generic;
using Terraria.Chat;
using Microsoft.Xna.Framework.Input;
using Terraria.Localization;

namespace CalamityMod.Projectiles.Ranged
{
    public class GaleforceArrow : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Ranged";

        public PrimitiveTrail TrailDrawer = null;
        public Vector2[] TrailPositions = new Vector2[10];

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
            Projectile.arrow = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.Calamity().pointBlankShotDuration = CalamityGlobalProjectile.DefaultPointBlankDuration;
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);
            SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
            return true;
        }

        public override void AI()
        {
            Vector2 destination = Projectile.Center;
            bool locatedTarget = false;

            // Find a target.
            for (int i = 0; i < Main.maxNPCs; i++)
            {
                float extraDistance = (Main.npc[i].width / 2) + (Main.npc[i].height / 2);
                if (!Main.npc[i].CanBeChasedBy(Projectile, false) || !Projectile.WithinRange(Main.npc[i].Center, 340f + extraDistance))
                    continue;

                Vector2 coneHomeCheck = (Main.npc[i].Center - Projectile.Center).SafeNormalize(Vector2.Zero) + Projectile.velocity.SafeNormalize(Vector2.Zero);
                if (Collision.CanHit(Projectile.Center, 1, 1, Main.npc[i].Center, 1, 1) && (Math.Abs(coneHomeCheck.X) > 1.15f || Math.Abs(coneHomeCheck.Y) > 1.15f)) 
                {
                    destination = Main.npc[i].Center;
                    locatedTarget = true;
                    break;
                }
            }

            if (locatedTarget)
            {
                // Home in on the target.
                Vector2 homeDirection = (destination - Projectile.Center).SafeNormalize(Vector2.UnitY);
                Projectile.velocity = (Projectile.velocity * 12f + homeDirection * 20f) / (12f + 1f);
            }

            if (Projectile.velocity.Length() < 20f)
            {
                Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitY) * 20f;
            }
        }

        public override void Kill(int timeLeft)
        {

        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {

        }
        public Color TrailColor(float completionRatio)
        {
            float opacity = Utils.GetLerpValue(0.7f, 0.0f, completionRatio, true) * Projectile.Opacity;
            Color color = Color.White;
            color.A = (byte)(int)(Utils.GetLerpValue(0.7f, 0f, completionRatio) * 255);
            return color;
        }

        public float TrailWidth(float completionRatio)
        {
            float widthInterpolant = Utils.GetLerpValue(0f, 0.25f, completionRatio, true) * Utils.GetLerpValue(0.8f, 0.9f, completionRatio, true);
            return MathHelper.SmoothStep(3f, 5f, widthInterpolant);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Projectile.rotation = (float)Math.Atan2((double)Projectile.velocity.Y, (double)Projectile.velocity.X) - MathHelper.ToRadians(90);

            if (TrailDrawer is null)
                TrailDrawer = new PrimitiveTrail(TrailWidth, TrailColor, null, GameShaders.Misc["CalamityMod:GaleforceArrowTrail"]);

            Texture2D texture = ModContent.Request<Texture2D>(Texture).Value;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition + Vector2.UnitY * Projectile.gfxOffY + ((Projectile.rotation - MathHelper.PiOver2).ToRotationVector2() * Projectile.height * 1.5f);
            Vector2 origin = texture.Size() * 0.5f;

            for (int i = -1; i <= 1; i += 2)
            {
                for (int oldPositions = 0; oldPositions < 10; oldPositions++)
                {

                    if (Projectile.oldPos[oldPositions] != Vector2.Zero)
                    {
                        TrailPositions[oldPositions] = Projectile.oldPos[oldPositions] + ((Projectile.oldRot[oldPositions] + MathHelper.PiOver2 + (MathHelper.Pi/2.5f * i)).ToRotationVector2() * 2f * oldPositions);
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
