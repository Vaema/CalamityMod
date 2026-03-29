using CalamityMod.Particles;
using CalamityMod.Utilities.Daybreak;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Rogue
{
    public class EclipseFragment : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";
        public static int lifetime => 1200;
        Color? color = null;
        public override void SetStaticDefaults() => ProjectileID.Sets.CultistIsResistantTo[Type] = true;
        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.penetrate = 5;
            Projectile.timeLeft = lifetime;
            Projectile.DamageType = RogueDamageClass.Instance;
            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.idStaticNPCHitCooldown = 10;
            Projectile.MaxUpdates = 2;
            Projectile.stopsDealingDamageAfterPenetrateHits = true;
        }

        public override void AI()
        {
            Projectile.rotation += Projectile.direction * 0.02f;
            if (Projectile.ai[0] < -1)
            {
                Projectile.penetrate = 1;
                Projectile.damage = Projectile.originalDamage;
                Projectile.stopsDealingDamageAfterPenetrateHits = false;
                var target = Projectile.FindTargetWithinRange(4000);
                if (target is not null)
                {
                    Projectile.velocity += Projectile.DirectionTo(target.Center) * 2;
                    Projectile.velocity *= 0.95f;
                    color ??= Color.Lerp(Color.OrangeRed, new Color(255, 191, 73), Main.rand.NextFloat());
                    GeneralParticleHandler.SpawnParticle(new SparkParticle(Projectile.Center, Projectile.velocity * 0.001f, false, 10, 1, color.Value));
                }
            }

            if (Projectile.ai[0] == 0f || Projectile.ai[2] > 0)
            {
                if (Projectile.timeLeft < (lifetime - Projectile.ai[1]) && Projectile.ai[2] >= 0)
                {
                    Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero);
                    Projectile.velocity *= Projectile.ai[2];   
                    Projectile.ai[2]--;
                }
                if (Projectile.ai[2] > 0 && Projectile.velocity.Length() > 1)
                {

                    color ??= Color.Lerp(Color.OrangeRed, new Color(255, 191, 73), Main.rand.NextFloat());
                    GeneralParticleHandler.SpawnParticle(new SparkParticle(Projectile.Center, Projectile.velocity * 0.001f, false, 10, 1, color.Value));
                }
            }
            else
            {
                if (Main.projectile.IndexInRange((int)Projectile.ai[0] - 1) && Main.projectile[(int)Projectile.ai[0] - 1].active)
                {
                    if (Projectile.timeLeft > 100)
                        Projectile.timeLeft = 100;
                    Projectile.Center = Vector2.Lerp(Projectile.Center, Main.projectile[(int)Projectile.ai[0] - 1].Center, (1 - (Projectile.timeLeft / 100f)));
                    Projectile.velocity *= 0f;
                    if (Projectile.Distance(Main.projectile[(int)Projectile.ai[0] - 1].Center) < 16)
                    {
                        Main.projectile[(int)Projectile.ai[0] - 1].ai[2]++;
                        Main.projectile[(int)Projectile.ai[0] - 1].netUpdate = true;
                        Projectile.active = false;
                    }
                }
            }
        }
        public override bool? CanHitNPC(NPC target)
        {
            if (Projectile.ai[0] < -1 && target.Calamity().IsArmored())
                return false;
            return null;
        }

        public override void OnKill(int timeLeft)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
                Projectile.NewProjectile(Projectile.GetSource_Death(),Projectile.Center,Vector2.Zero,ModContent.ProjectileType<EclipseStealthBoom>(),Projectile.damage,Projectile.knockBack,Projectile.owner);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            using (Main.spriteBatch.Scope())
            {
                Main.spriteBatch.Begin(default, BlendState.NonPremultiplied, null, null, null, null, Main.Transform);
                Main.EntitySpriteDraw(TextureAssets.Projectile[Type].Value, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, TextureAssets.Projectile[Type].Size() * 0.5f, Projectile.scale, 0);
                Main.spriteBatch.End();
            }
            return false;
        }
    }
}
