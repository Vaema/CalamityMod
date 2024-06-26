using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Projectiles.BaseProjectiles;
using CalamityMod.Projectiles.Melee;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Melee.MaceFlails
{
    public class RemsRevengeProj : BaseMaceFlailProjectile
    {
        public override int AssociatedItemID => ModContent.ItemType<RemsRevenge>();
        public override float SpinHitboxRadius => 80f;
        public override float SpinVisualRadius => 48f;
        public override float LaunchSpeed => 24f;
        public override int LaunchLifespan => 30;
        public override float MaxDropRange => 960f;
        public override float MaxRetractSpeed => 27f;
        public override float RetractAcceleration => 4.5f;

        public ref float LaunchedHitCounter => ref Projectile.ai[2];

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 26;
            base.SetDefaults();
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<WitherDebuff>(), 240);

            if (CurrentFlailState != FlailState.Spinning)
                LaunchedHitCounter++;

            if (LaunchedHitCounter < 6f)
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), target.Center, Vector2.Zero, ModContent.ProjectileType<RemsRevengeExplosion>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
            if (LaunchedHitCounter > 3f)
            {
                CurrentFlailState = FlailState.ForcedRetracting;
                StateTimer = 0f;
                Projectile.netUpdate = true;
            }
        }
    }
}
