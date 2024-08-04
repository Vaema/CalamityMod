using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Ranged
{
    public class TauCannonPortal : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Ranged";

        public override string Texture => "CalamityMod/ExtraTextures/GreyscaleVortex";

        private float Scale01
        {
            get
            {
                if (Projectile.timeLeft > 360)
                    return Utils.GetLerpValue(420f, 360f, Projectile.timeLeft, true);

                if (Projectile.timeLeft >= 60 && Projectile.timeLeft <= 360)
                    return 1f;

                if (Projectile.timeLeft < 60)
                    return Utils.GetLerpValue(0f, 60f, Projectile.timeLeft, true);

                return 0f;
            }
        }

        public override void SetDefaults()
        {
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.width = Projectile.height = 153;
            Projectile.timeLeft = 420;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
            => CalamityUtils.CircularHitboxCollision(Projectile.Center, 256f * Scale01, targetHitbox);

        public override void AI()
        {
            Projectile.rotation += MathHelper.ToRadians(5f) * Scale01;
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) => modifiers.SourceDamage *= 2f;

        public override void OnKill(int timeLeft)
        {
            if (Main.myPlayer == Projectile.owner)
            {
                int randomBoltAmount = Main.rand.Next(8, 13);
                float starterAngle = Main.rand.NextFloat(MathHelper.TwoPi);
                for (int i = 0; i < randomBoltAmount; i++)
                {
                    float angle = starterAngle + (MathHelper.TwoPi / randomBoltAmount * i);
                    Vector2 velocity = angle.ToRotationVector2() * 15f;
                    Projectile.NewProjectileDirect(
                        Projectile.GetSource_FromThis(),
                        Projectile.Center,
                        velocity,
                        ModContent.ProjectileType<TauCannonBolt>(),
                        Projectile.damage,
                        Projectile.knockBack,
                        Projectile.owner);
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Vector2 drawPosition = Projectile.Center - Main.screenPosition;
            Vector2 anchorPoint = texture.Size() * 0.5f;

            Main.EntitySpriteDraw(texture, drawPosition, null, Color.Pink * Scale01, Projectile.rotation, anchorPoint, Scale01 * 0.375f, SpriteEffects.None);

            return false;
        }
    }
}
