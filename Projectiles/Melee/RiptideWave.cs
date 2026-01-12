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

        public float RotationDirection = 1f;

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
            Projectile.aiStyle = -1;
        }

        public override void AI()
        {
            Projectile.rotation += MathHelper.Lerp(0.3f, 0f, Projectile.timeLeft / (float)Lifetime) * RotationDirection;

            float MaxSize = 135f * Projectile.ai[1];

            // Stay on top of the yoyo.
            if (Main.projectile[(int)Projectile.ai[0]].active)
                Projectile.Center = Vector2.Lerp(Projectile.Center, Main.projectile[(int)Projectile.ai[0]].Center + Main.projectile[(int)Projectile.ai[0]].velocity, 0.2f);

            // Expand the hitbox as lifespan goes on.
            int waveSize = (int)MathHelper.Lerp(MaxSize, 16, Projectile.timeLeft / (float)Lifetime);
            Projectile.ExpandHitboxBy(waveSize);

            // Fade out.
            if (Projectile.timeLeft <= 20)
                Projectile.alpha += (int)(12 * Projectile.ai[1]);
        }

        // Ensure knockback is always applied away from the player.
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers) => modifiers.HitDirectionOverride = (target.Center.X > Main.player[Projectile.owner].Center.X).ToDirectionInt();

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D waveTex = ModContent.Request<Texture2D>("CalamityMod/Particles/SeaFoam").Value;
            Rectangle frame = waveTex.Frame(1, 3, 0, (int)Projectile.ai[2]);
            Color Col = Color.Lerp(Color.DarkSlateBlue, Color.DeepSkyBlue, Projectile.ai[1]);
            Color waveColor = Lighting.GetColor((Projectile.Center / 16).ToPoint()).MultiplyRGBA(Color.Lerp(Color.Transparent, Col, Projectile.Opacity).MultiplyRGBA(new Color(1f, 1f, 1f, 0f)));
            float waveScale = Projectile.width / (float)waveTex.Width;
            Main.EntitySpriteDraw(waveTex, Projectile.Center - Main.screenPosition, frame, waveColor, Projectile.rotation, frame.Size() / 2f, waveScale, SpriteEffects.None);

            Color ringColor = Lighting.GetColor((Projectile.Center / 16).ToPoint()).MultiplyRGBA(Color.Lerp(Color.Transparent, Col.MultiplyRGBA(new Color(0.6f, 0.8f, 1f, 0f)), Projectile.Opacity));
            Texture2D ringTex = ModContent.Request<Texture2D>("CalamityMod/Particles/HollowCircleSoftEdge").Value;
            float ringScale = Projectile.width / (float)ringTex.Width;
            Main.EntitySpriteDraw(ringTex, Projectile.Center - Main.screenPosition, null, ringColor, Projectile.rotation, ringTex.Size() / 2f, ringScale, SpriteEffects.None);

            return false;
        }
    }
}
