using CalamityMod.Buffs.DamageOverTime;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityMod.Dusts;
using CalamityMod.Particles;

namespace CalamityMod.Projectiles.Rogue;

public class ToxicantTwisterDust : ModProjectile, ILocalizedModType
{
    public new string LocalizationCategory => "Projectiles.Rogue";
    public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
    public NPC targetedNPC;

    public override void SetStaticDefaults() => ProjectileID.Sets.CultistIsResistantTo[Type] = true;
    public override void SetDefaults()
    {
        Projectile.width = 6;
        Projectile.height = 6;
        Projectile.friendly = true;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.MaxUpdates = 4;
        Projectile.timeLeft = 180;
        Projectile.DamageType = RogueDamageClass.Instance;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 30;
        Projectile.noEnchantmentVisuals = true;
    }

    public override void AI()
    {
        Player Owner = Main.player[Projectile.owner];
        float targetDist = Vector2.Distance(Owner.Center, Projectile.Center);
        if (Projectile.ai[1] % 2 == 0 && targetDist < 1400)
        {
            Particle spark = new GlowSparkParticle(Projectile.Center + Projectile.velocity * Main.rand.NextFloat(-2, -1), -Projectile.velocity * 0.3f, false, 5, 0.04f, Color.Lerp(Color.Green, Color.Chartreuse, 0.8f) * 0.65f, new Vector2(1, 0.3f), true, false, 1.5f);
            GeneralParticleHandler.SpawnParticle(spark);
        }

        Projectile.rotation += 0.2f;

        targetedNPC = (Projectile.ai[1] > 90) ? Projectile.Center.ClosestNPCAt(1200) : null;
        float moveSpeed = Utils.GetLerpValue(200, 0, Projectile.timeLeft) * 0.5f;
        if (targetedNPC == null)
        {
            Vector2 position = (Owner.ClampedMouseWorld() + ((new Vector2(0, -250).RotatedBy(Projectile.rotation * 0.2f)).RotatedBy(MathHelper.ToRadians(90f) * Projectile.ai[2])));
            Vector2 moveToMouse = (position - Projectile.Center).SafeNormalize(Vector2.UnitX);
            if (Projectile.velocity.Length() < 8)
                Projectile.velocity += moveToMouse * (0.7f);
            else
                Projectile.velocity *= 0.9f;
        }
        else
        {
            CalamityUtils.HomeInOnSelectedNPC(Projectile, targetedNPC, true, moveSpeed, 13, 0.98f);
            if (Projectile.ai[1] % 2 == 0)
                Projectile.timeLeft++; // Lasts longer if it has a target
        }
        if (targetedNPC != null && targetedNPC.life <= 0)
            targetedNPC = null;
        
        Projectile.ai[1]++;
    }

    public override bool? CanHitNPC(NPC target) => target == targetedNPC ? null : false;
    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => target.AddBuff(ModContent.BuffType<SulphuricPoisoning>(), 180);
    public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => CalamityUtils.CircularHitboxCollision(Projectile.Center, 40, targetHitbox);
    public override void OnKill(int timeLeft)
    {
        for (int i = 0; i <= 2; i++)
        {
            Dust dust = Dust.NewDustPerfect(Projectile.Center, (int)CalamityDusts.SulphurousSeaAcid, Projectile.velocity.RotatedByRandom(MathHelper.ToRadians(15f)) * Main.rand.NextFloat(0.3f, 1.8f), 0, default, Main.rand.NextFloat(1.3f, 1.8f));
            dust.noGravity = true;
        }
    }
}
