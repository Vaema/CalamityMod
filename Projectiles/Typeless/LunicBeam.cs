using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.Dusts;
using CalamityMod.Items.Weapons.Typeless;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Projectiles.Typeless
{
    public class LunicBeam : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Typeless";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetStaticDefaults() => ProjectileID.Sets.CultistIsResistantTo[Type] = true;
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 8;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.penetrate = 1;
            Projectile.MaxUpdates = 3;
            Projectile.timeLeft = 120 * Projectile.MaxUpdates;
        }

        public override void AI()
        {
            if (Projectile.ai[0] == 0f)
            {
                for (int i = 0; i < 4; i++)
                {
                    Vector2 velocity = (MathHelper.TwoPi * i / 4f + Projectile.velocity.ToRotation() + MathHelper.PiOver4).ToRotationVector2() * 0.8f;
                    Particle cross = new GlowSparkParticle(Projectile.Center, velocity, false, 6, 0.015f, Color.DarkOrange, Vector2.One, true);
                    GeneralParticleHandler.SpawnParticle(cross);
                }
                Projectile.ai[0] = 1f;
            }

            // Find the closest NPC targetable
            Color trailColor = new Color(100, 30, 20);
            float range = 240f;
            int targetNPC = -1;
            foreach (NPC target in Main.ActiveNPCs)
            {
                if (!target.CanBeChasedBy(Projectile))
                    continue;

                float distance = Vector2.Distance(target.Center, Projectile.Center);
                if (distance < range && Collision.CanHit(Projectile, target))
                {
                    range = distance;
                    targetNPC = target.whoAmI;
                }
            }
            if (targetNPC > -1)
            {
                NPC target = Main.npc[targetNPC];
                Vector2 idealVelocity = Projectile.SafeDirectionTo(target.Center) * 12f;
                Projectile.velocity = (Projectile.velocity * 39f + idealVelocity) / 40f;
                Projectile.velocity = Projectile.velocity.MoveTowards(idealVelocity, 1f);
                trailColor = Color.Lerp(new Color(100, 30, 20), Color.Indigo, Utils.GetLerpValue(240f, 0f, Vector2.Distance(Projectile.Center, target.Center), true));
            }
            Lighting.AddLight(Projectile.Center, trailColor.ToVector3() * 0.5f);

            Dust trail = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<LightDust>(), Projectile.velocity * 0.05f);
            trail.noGravity = true;
            trail.scale = Main.rand.NextFloat(1f, 1.6f);
            trail.color = trailColor;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => target.AddBuff(ModContent.BuffType<MarkedforDeath>(), 480);

        public override void OnHitPlayer(Player target, Player.HurtInfo info) => target.AddBuff(ModContent.BuffType<MarkedforDeath>(), 480);

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(LunicEye.ImpactSound, Projectile.Center);
            float offset = Main.rand.NextFloat(MathHelper.TwoPi);
            for (int i = 0; i < 4; i++)
            {
                Vector2 velocity = (MathHelper.TwoPi * i / 4f + offset).ToRotationVector2() * 0.5f;
                Particle outline = new AltSparkParticle(Projectile.Center + velocity * 50f, velocity, false, 12, 2f, Color.Black);
                GeneralParticleHandler.SpawnParticle(outline);
                Particle cross = new SparkParticle(Projectile.Center + velocity * 50f, velocity, false, 12, 1f, Color.Indigo);
                GeneralParticleHandler.SpawnParticle(cross);
            }
            for (int i = 0; i < 25; i++)
            {
                Vector2 velocity = (MathHelper.TwoPi * i / 25f).ToRotationVector2() * 6f;
                Dust ring = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<VoidDust>(), velocity);
                ring.noGravity = true;
                ring.scale = Main.rand.NextFloat(1.6f, 1.8f);
                ring.color = Color.Indigo * 0.5f;
            }
        }
    }
}
