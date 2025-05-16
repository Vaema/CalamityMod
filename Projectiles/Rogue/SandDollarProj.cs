using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Projectiles.Rogue
{
    public class SandDollarProj : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";
        public override string Texture => "CalamityMod/Items/Weapons/Rogue/SandDollar";

        public ref float Timer => ref Projectile.ai[0];
        public ref float BounceCheck => ref Projectile.ai[1];
        private const int Pierce = 2;
        public override void SetDefaults()
        {
            Projectile.width = 30;
            Projectile.height = 28;
            Projectile.DamageType = RogueDamageClass.Instance;
            Projectile.friendly = true;
            Projectile.penetrate = Pierce;
            Projectile.timeLeft = 600;
            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.idStaticNPCHitCooldown = 25;
        }

        public override void AI()
        {
            Timer++;
            Projectile.rotation += BounceCheck > 0f ? 0.35f : 0.2f;
            // Bouncing resets the timer for use in fading in its glow
            if (Timer >= 30f || BounceCheck > 0f)
            {
                if (BounceCheck > 0f && Projectile.Calamity().stealthStrike)
                    Projectile.velocity *= 0.975f;
                else
                {
                    Projectile.velocity.X *= BounceCheck > 0f ? 0.985f : 0.998f;
                    Projectile.velocity.Y += 0.0375f;
                }
            }
            if (Projectile.velocity.Y > 8f)
                Projectile.velocity.Y = 8f;

            // Random dust after bouncing
            if (BounceCheck > 0f || Projectile.Calamity().stealthStrike)
            {
                Vector2 dustPos = Projectile.Center + Main.rand.NextVector2CircularEdge(20f, 15f);
                Vector2 dustVel = Utils.DirectionTo(Projectile.Center, dustPos).RotatedBy(MathHelper.PiOver2) * 2f;
                Dust dust = Dust.NewDustPerfect(dustPos, DustID.Sand, dustVel, Scale: 0.75f);
                dust.noGravity = true;
            }
        }
        public override bool? CanDamage() => BounceCheck > 0f && Projectile.Calamity().stealthStrike ? false : null;

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            // Should only bounce if it hasn't bounced before, hasn't pierced an enemy, and is hitting a floor (moving downwards)
            if (BounceCheck == 0f && (Projectile.penetrate == Pierce || Projectile.Calamity().stealthStrike) && Projectile.velocity.Y != oldVelocity.Y && oldVelocity.Y > 0f)
            {
                BounceCheck = 1f;
                Timer = 0f;
                Projectile.penetrate = 1;
                Projectile.velocity.Y = -oldVelocity.Y;
                if (Projectile.velocity.Y < -4f)
                    Projectile.velocity.Y = -4f;

                // Summon a duststorm on stealth strikes
                if (Projectile.Calamity().stealthStrike && Main.myPlayer == Projectile.owner)
                {
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<SandDollarStealth>(), Projectile.damage / 4, 1f, Projectile.owner, Projectile.whoAmI);
                }
                return false;
            }
            return true;
        }

        public override void OnKill(int timeLeft)
        {
            for (int s = 0; s < 8; s++)
            {
                Dust dust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Sand, Scale: 0.9f);
                dust.noGravity = true;
            }

            if (BounceCheck > 0f && Main.myPlayer == Projectile.owner)
            {
                for (int i = 1; i <= 3; i++)
                {
                    Vector2 velocity = -Vector2.UnitY.RotatedByRandom(MathHelper.Pi) * Main.rand.NextFloat(3f, 4.5f);
                    int p = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, velocity, ModContent.ProjectileType<SandDollarFrag>(), (int)(Projectile.damage * 0.4f), 0f, Projectile.owner, i);
                    Main.projectile[p].rotation = Main.rand.NextFloat(MathHelper.TwoPi);
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Projectile[Type].Value;
            Vector2 stretch = new Vector2(MathHelper.Lerp(1.2f, 0.8f, (float)Math.Abs(Math.Sin(Projectile.rotation))), MathHelper.Lerp(0.85f, 1.15f, (float)Math.Abs(Math.Sin(Projectile.rotation))));
            Color drawColor = BounceCheck > 0f ? Color.Lerp(lightColor, Color.White, MathHelper.Clamp(Timer / 60f, 0f, 0.5f)) : lightColor;

            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, drawColor, Projectile.rotation, tex.Size() / 2f, stretch, SpriteEffects.None);
            return false;
        }
    }
}
