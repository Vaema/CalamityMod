using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Ranged
{
    public class SporeKnifeBud : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";
        public override string Texture => "CalamityMod/Projectiles/Summon/PlantationStaffTentacle";

        public bool Sticky = false;
        public static int Lifetime = 300;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.CultistIsResistantTo[Type] = true;
            Main.projFrames[Type] = 4;
        }
        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 50;
            Projectile.timeLeft = Lifetime;
        }
        public override bool? CanHitNPC(NPC target) => Projectile.timeLeft <= 285 && target.CanBeChasedBy(Projectile);

        public override void AI()
        {
            //Animation
            Projectile.frameCounter++;
            if (Projectile.frameCounter > 6)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
            }
            if (Projectile.frame >= Main.projFrames[Type])
                Projectile.frame = 0;
            //Rotation
            Projectile.spriteDirection = Projectile.direction = (Projectile.velocity.X > 0).ToDirectionInt();
            Projectile.rotation = Projectile.velocity.ToRotation() + (Projectile.spriteDirection == 1 ? 0f : MathHelper.Pi);
            if (!Sticky && Projectile.timeLeft <= Lifetime - 15)
            {
                CalamityUtils.HomeInOnNPC(Projectile, !Projectile.tileCollide, 450f, 6.5f, 20f);
                int dust = Dust.NewDust(Projectile.position - new Vector2(10, 10), 30, 30, DustID.JungleTorch, Projectile.velocity.X, Projectile.velocity.Y, 50, default, Main.rand.NextFloat(0.3f, 0.7f));
                Main.dust[dust].noGravity = true;
            }
            else if (Sticky)
            {
                Projectile.StickyProjAI(15);
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Sticky = true;
            target.AddBuff(BuffID.Poisoned, 60);
            for (int i = 0; i < 3; i++)
            {
                Vector2 smokeVel = Main.rand.NextVector2Unit() * Main.rand.NextVector2Circular(6f, 6f);
                Color smokeColor = Main.rand.NextBool(2) ? Color.GreenYellow : Color.Green;
                Particle smoke = new MediumMistParticle(Projectile.Center, smokeVel, smokeColor, Color.Black, Main.rand.NextFloat(0.2f, 0.4f), 250 - Main.rand.Next(60), 0.08f);
                GeneralParticleHandler.SpawnParticle(smoke);
            }
            for (int k = 0; k < 2; k++)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.MagicMirror, new Vector2(2, 2).RotatedByRandom(100) * Main.rand.NextFloat(0.5f, 0.8f), 0, Color.YellowGreen, Main.rand.NextFloat(0.4f, 0.9f));
                dust.noGravity = false;
                dust.alpha = Main.rand.Next(20, 30 + 1);
            }
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            Projectile.ModifyHitNPCSticky(6);
        }

        public override void OnKill(int timeLeft)
        {
            for (int d = 0; d < 7; ++d)
            {
                int idx = Dust.NewDust(Projectile.Center - Vector2.One * 4f, 25, 25, DustID.MagicMirror, 0f, -2f, 0, Color.YellowGreen, 0.4f);
                Dust dust = Main.dust[idx];
                dust.velocity /= 2f;
            }
        }
    }
}
