using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Melee
{
    public class RiptideWave : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Melee";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public const int Lifetime = 30;
        public const int MaxSize = 176;

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = Lifetime;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            // Constantly stay on top of the yoyo.
            if (Main.projectile[(int)Projectile.ai[0]].active)
                Projectile.Center = Main.projectile[(int)Projectile.ai[0]].Center + Main.projectile[(int)Projectile.ai[0]].velocity;

            // Expand the hitbox as lifespan goes on.
            int waveSize = (int)MathHelper.Lerp(MaxSize, 16, Projectile.timeLeft / (float)Lifetime);
            Projectile.ExpandHitboxBy(waveSize);

            // Fade out.
            if (Projectile.timeLeft <= 20)
                Projectile.alpha += 12;
        }

        // Ensure knockback is always applied away from the player.
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) => modifiers.HitDirectionOverride = (target.Center.X > Main.player[Projectile.owner].Center.X).ToDirectionInt();

        public override bool PreDraw(ref Color lightColor)
        {
            Main.spriteBatch.SetBlendState(BlendState.Additive);

            Texture2D waveTex = ModContent.Request<Texture2D>("CalamityMod/Particles/SeaFoam").Value;
            Rectangle frame = waveTex.Frame(1, 3, 0, (int)Projectile.ai[2]);
            Color waveColor = new Color(61, 119, 148) * Projectile.Opacity;
            float waveScale = Projectile.width / (float)waveTex.Width;
            Main.EntitySpriteDraw(waveTex, Projectile.Center - Main.screenPosition, frame, waveColor, Projectile.ai[1], frame.Size() / 2f, waveScale, SpriteEffects.None);

            Texture2D ringTex = ModContent.Request<Texture2D>("CalamityMod/Particles/HollowCircleSoftEdge").Value;
            float ringScale = Projectile.width / (float)ringTex.Width;
            Main.EntitySpriteDraw(ringTex, Projectile.Center - Main.screenPosition, null, waveColor, 0f, ringTex.Size() / 2f, ringScale, SpriteEffects.None);

            Main.spriteBatch.SetBlendState(BlendState.AlphaBlend);
            return false;
        }
    }
}
