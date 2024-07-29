using CalamityMod.NPCs;
using CalamityMod.NPCs.Providence;
using CalamityMod.Particles;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Boss
{
    public class SwirlingFire : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Boss";
        public ref float AngularTurnSpeed => ref Projectile.ai[0];
        public ref float Time => ref Projectile.ai[1];
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 10;
            Projectile.hostile = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.alpha = 255;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 180;
            CooldownSlot = ImmunityCooldownID.Bosses;
            Projectile.ai[0] = MathHelper.ToRadians(Main.rand.NextFloat(-3, 3));
        }

        public override void AI()
        {

            Projectile.maxPenetrate = (int)Providence.BossMode.Day;

            if (CalamityGlobalNPC.holyBoss != -1)
            {
                Projectile.maxPenetrate = (int)Main.npc[CalamityGlobalNPC.holyBoss].localAI[1];
            }

            if (CalamityGlobalNPC.doughnutBoss != -1)
            {
                if (Main.npc[CalamityGlobalNPC.doughnutBoss].active)
                {
                    if (Main.npc[CalamityGlobalNPC.doughnutBoss].Calamity().CurrentlyEnraged)
                        Projectile.maxPenetrate = (int)Providence.BossMode.Night;
                    else
                        Projectile.maxPenetrate = (int)Providence.BossMode.Day;
                }
            }

            Color c = ProvUtils.GetProjectileColor(Projectile.maxPenetrate, 255);

            GeneralParticleHandler.SpawnParticle(new GlowOrbParticle(Projectile.Center, Projectile.velocity / 2f, false, 10, 0.5f * Projectile.ai[2], c));
            GeneralParticleHandler.SpawnParticle(new MediumMistParticle(Projectile.Center, Vector2.Zero, Color.LightSlateGray, Color.DarkSlateGray, 0.5f * Projectile.ai[2], 150, Main.rand.NextFloat(-0.01f, 0.01f)));

            Projectile.ai[2] *= 0.98f;

            Projectile.velocity *= 0.97f;

            if (Projectile.ai[2] < 0.2f)
            {
                Projectile.Kill();
            }

            Projectile.velocity = Projectile.velocity.RotatedBy(AngularTurnSpeed);
            Time++;
        }
    }
}
