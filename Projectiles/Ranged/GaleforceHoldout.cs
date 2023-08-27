using System;
using CalamityMod.Items.Weapons.Magic;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;


namespace CalamityMod.Projectiles.Ranged
{
    //Holdout, but invisible. It may as well be named "GaleforceHandler"
    public class GaleforceHoldout : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Ranged";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public static float MaxCharge = 50;
        public static int ShotProjectiles = 5;

        public ref float Charge => ref Projectile.ai[0];
        public float ChargeProgress => MathHelper.Clamp(Charge, 0, MaxCharge) / MaxCharge;
        public float FullChargeProgress => MathHelper.Clamp(Charge, 0, MaxCharge * 1.5f) / (MaxCharge * 1.5f);
        public float Spread => MathHelper.PiOver2 * (1 - (float)Math.Pow(ChargeProgress, 1.5) * 0.95f);

        public Player Owner => Main.player[Projectile.owner];



        public override void SetDefaults()
        {
            Projectile.width = 38;
            Projectile.height = 68;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
        }

        public override bool? CanDamage()
        {
            return false;
        }

        public override bool ShouldUpdatePosition()
        {
            return false;
        }

        public override void AI()
        {
            if (Owner.channel)
            {
                Projectile.timeLeft = 2;
                Owner.itemTime = 48;
                Owner.itemAnimation = 48;
                Owner.heldProj = Projectile.whoAmI;
            }

            float pointingRotation = (Owner.Calamity().mouseWorld - Owner.MountedCenter).ToRotation();
            Projectile.Center = Owner.MountedCenter + pointingRotation.ToRotationVector2() * 40f;

            Charge++;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            float angle = (Owner.Calamity().mouseWorld - Owner.MountedCenter).ToRotation();

            Effect effect = Filters.Scene["CalamityMod:SpreadTelegraph"].GetShader().Shader;
            effect.Parameters["centerOpacity"].SetValue(0.7f);
            effect.Parameters["mainOpacity"].SetValue((float)Math.Sqrt(ChargeProgress));
            effect.Parameters["halfSpreadAngle"].SetValue(Spread / 2f);
            effect.Parameters["edgeColor"].SetValue(Color.Cyan.ToVector3());
            effect.Parameters["centerColor"].SetValue(Color.DarkCyan.ToVector3());
            effect.Parameters["edgeBlendLength"].SetValue(0.07f);
            effect.Parameters["edgeBlendStrength"].SetValue(8f);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, effect, Main.GameViewMatrix.TransformationMatrix);
            
            Texture2D texture = ModContent.Request<Texture2D>("CalamityMod/Projectiles/InvisibleProj").Value;

            Main.EntitySpriteDraw(texture, Owner.MountedCenter - Main.screenPosition, null, Color.White, angle, new Vector2(texture.Width / 2f, texture.Height/2f), 700f, 0, 0);

            Main.spriteBatch.End();
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);

            float mainAngle = (Projectile.Center - Owner.MountedCenter).ToRotation();
            Texture2D arrowTexture = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Ranged/GaleforceArrow").Value;

            for (int i = 0; i < ShotProjectiles; i++)
            {
                float angleOffset = MathHelper.Lerp(Spread * -0.5f, Spread * 0.5f, i / ((float)ShotProjectiles - 1));
                float direction = mainAngle + angleOffset;
                Vector2 displacement = (direction + MathHelper.Pi / 12f).ToRotationVector2(); //Fixes some rotational offset with positions for the arrows
                Main.EntitySpriteDraw(arrowTexture, Owner.MountedCenter - Main.screenPosition + (displacement * 26f), null, Color.White, direction - MathHelper.PiOver2, new Vector2(texture.Width / 2f, texture.Height / 2f), 1f, 0, 0);

            }

            return false;
        }

        public override void Kill(int timeLeft)
        {
            float mainAngle = (Projectile.Center - Owner.MountedCenter).ToRotation();

            SoundEngine.PlaySound(SoundID.Item5 with { Volume = SoundID.Item167.Volume * 0.4f + 0.2f * ChargeProgress }, Owner.MountedCenter);

            for (int i = 0; i < ShotProjectiles; i++)
            {
                float angleOffset = MathHelper.Lerp(Spread * -0.5f, Spread * 0.5f, i / ((float)ShotProjectiles - 1));
                Vector2 direction = (mainAngle + angleOffset).ToRotationVector2();

                if (Owner.whoAmI == Main.myPlayer)
                {
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Owner.MountedCenter + direction * 30f, direction * 20f, ModContent.ProjectileType<GaleforceArrow>(), Projectile.damage, Projectile.knockBack, Owner.whoAmI, ChargeProgress);
                }

                Particle pulse = new DirectionalPulseRing(Owner.MountedCenter + direction * 44f, Vector2.Zero, Color.Cyan, new Vector2(0.5f, 1f), direction.ToRotation(), 0.04f, 0.2f, 30);
                GeneralParticleHandler.SpawnParticle(pulse);
            }

        }
    }
}
