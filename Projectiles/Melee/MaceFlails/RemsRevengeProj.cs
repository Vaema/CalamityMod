using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Projectiles.BaseProjectiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Melee.MaceFlails;

public class RemsRevengeProj : BaseMaceFlailProjectile
{
    public override int AssociatedItemID => ModContent.ItemType<RemsRevenge>();
    public override int SpinIFrames => 12;
    public override float SpinSpeed => 16f;
    public override float SpinHitboxRadius => 96f;
    public override float SpinVisualRadius => 64f;
    public override int AfterimageLength => 8;
    public override float LaunchSpeed => 30f;
    public override int LaunchLifespan => 24;
    public override float MaxDropRange => 960f;
    public override float MaxRetractSpeed => 36f;
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

        if (CurrentFlailState != FlailState.Spinning && CurrentFlailState != FlailState.Dropping)
            LaunchedHitCounter++;

        // While spinning, the projectile center won't always be where the target is, so it needs to be moved
        UpdateDamageKB(out float damageMult, out float kbMult);
        if (LaunchedHitCounter <= 5f)
            Projectile.NewProjectile(Projectile.GetSource_FromThis(), CurrentFlailState == FlailState.Spinning ? target.Center : Projectile.Center, Vector2.Zero, ModContent.ProjectileType<RemsRevengeExplosion>(), (int)(Projectile.damage * damageMult), Projectile.knockBack * kbMult, Projectile.owner);
        if (LaunchedHitCounter >= 4f)
        {
            CurrentFlailState = FlailState.ForcedRetracting;
            StateTimer = 0f;
            Projectile.netUpdate = true;
        }
    }
}
