using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.NPCs.SunkenSea;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
namespace CalamityMod.Projectiles.Enemy
{
    public class AnglerFlash : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Enemy";

        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetDefaults()
        {
            Projectile.width = 100;
            Projectile.height = 100;
            Projectile.hostile = true;
            Projectile.alpha = 255;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 30;
            Projectile.localNPCHitCooldown = -1;
            Projectile.usesLocalNPCImmunity = true;
        }

        public override void AI()
        {
            if (Projectile.ai[0] == 0)
            {
                GeneralParticleHandler.SpawnParticle(new BloomParticle(Projectile.Center, Vector2.Zero, Color.LightCyan, 1, 3, 30, true));
                Projectile.ai[0] = 1;
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => CalamityUtils.CircularHitboxCollision(Projectile.Center, 100, targetHitbox);

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            if (info.Damage <= 0)
                return;

            target.AddBuff(ModContent.BuffType<Eutrophication>(), 120);
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<Eutrophication>(), 120);
        }

        public override bool? CanHitNPC(NPC target)
        {
            if (LazarusLampfish.PredatorList.Contains(target.type))
            {
                return true;
            }
            return null;
        }
    }
}
