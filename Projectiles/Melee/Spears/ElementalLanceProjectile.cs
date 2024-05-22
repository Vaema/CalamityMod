using System;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Projectiles.BaseProjectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Melee.Spears
{
    public class ElementalLanceProjectile : BaseSpearProjectile
    {
        public override LocalizedText DisplayName => CalamityUtils.GetItemName<ElementalLance>();

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 40;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.timeLeft = 90;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.ownerHitCheck = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 6;
        }

        public override float InitialSpeed => 3f;
        public override float ReelbackSpeed => 1.1f;
        public override float ForwardSpeed => 0.5f;
        public override Action<Projectile> EffectBeforeReelback => (proj) =>
        {
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.velocity.SafeNormalize(Vector2.UnitY) * 16f, ModContent.ProjectileType<SpatialSpear>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
        };

        public override void ExtraBehavior()
        {
            if (Main.rand.NextBool(5))
            {
                int idx = Dust.NewDust(Projectile.position + Projectile.velocity, Projectile.width, Projectile.height, DustID.RainbowTorch, Projectile.direction * 2, 0f, 150, new Color(Main.DiscoR, Main.DiscoG, Main.DiscoB), 1f);
                Main.dust[idx].noGravity = true;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<ElementalMix>(), 60);
        }
    }
}
