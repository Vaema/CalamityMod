using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Projectiles.BaseProjectiles;
using CalamityMod.Projectiles.Ranged;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Melee.Spears;

public class EarthenPikeSpear : BaseSpearProjectile
{
    public override LocalizedText DisplayName => CalamityUtils.GetItemName<EarthenPike>();
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
        Projectile.localNPCHitCooldown = 8;
    }

    public override float InitialSpeed => 3f;
    public override float ReelbackSpeed => 2.4f;
    public override float ForwardSpeed => 0.4f;
    public override void ExtraBehavior()
    {
        Projectile.localAI[0] += 1f;
        if (Projectile.localAI[0] >= 6f)
        {
            Projectile.localAI[0] = 0f;
            if (Main.myPlayer == Projectile.owner)
            {
                float randomVelocity = Main.rand.NextFloat(1.085f, 1.115f);
                int proj = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.velocity * randomVelocity,
                    ModContent.ProjectileType<FossilShard>(), (int)(Projectile.damage * 0.5), Projectile.knockBack * 0.2f, Projectile.owner, 0f, 1f);
                if (proj.WithinBounds(Main.maxProjectiles))
                    Main.projectile[proj].DamageType = DamageClass.Melee;
            }
        }
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => target.AddBuff(ModContent.BuffType<ArmorCrunch>(), 300);
}
