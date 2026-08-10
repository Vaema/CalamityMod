using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Buffs.StatDebuffs;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Rogue;

public class DeepWounderProjectile : ModProjectile, ILocalizedModType
{
    public new string LocalizationCategory => "Projectiles.Rogue";
    public override string Texture => "CalamityMod/Items/Weapons/Rogue/DeepWounder";

    public NPC StuckTo;
    public Vector2 StuckOffset = Vector2.Zero;

    public override void SetDefaults()
    {
        Projectile.width = 30;
        Projectile.height = 30;
        Projectile.friendly = true;
        Projectile.ignoreWater = true;
        Projectile.penetrate = 3;
        Projectile.timeLeft = 600;
        Projectile.DamageType = RogueDamageClass.Instance;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 20;
    }

    public override void AI()
    {
        Projectile.rotation += (StuckTo == null ? 0.3f : 0.6f) * Projectile.direction;

        if (Projectile.Calamity().stealthStrike)
        {
            int spriteWidth = 52;
            int spriteHeight = 48;

            for (int i = 0; i < 10; i++)
            {
                int dust = Dust.NewDust(Projectile.position, spriteWidth, spriteHeight, DustID.Water, Projectile.velocity.X * 0.1f, Projectile.velocity.Y * 0.1f, 0, default, 1f);
                Main.dust[dust].noGravity = true;
            }

            if (StuckTo != null)
            {
                if (!StuckTo.CanBeChasedBy(Projectile))
                    Projectile.Kill();

                Projectile.Center = StuckTo.Center + StuckOffset;
                Projectile.velocity = Vector2.Zero;

                if (Projectile.timeLeft % 5 == 0)
                {
                    Vector2 waterVelocity = (Projectile.Center - StuckTo.Center).SafeNormalize(Vector2.UnitX).RotatedByRandom(MathHelper.Pi / 6f) * 8f;
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, waterVelocity, ModContent.ProjectileType<DeepWounderWater>(), (int)(Projectile.damage * 0.1f), 1, Projectile.owner, 0, 0);
                }
            }
        }
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        target.AddBuff(ModContent.BuffType<HeavyBleeding>(), 120);
        target.AddBuff(ModContent.BuffType<CrushDepth>(), 120);
        if (Projectile.Calamity().stealthStrike)
        {
            if (StuckTo == null)
            {
                StuckTo = target;
                StuckOffset = Projectile.Center - target.Center;
            }
        }
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo info)
    {
        target.AddBuff(ModContent.BuffType<ArmorCrunch>(), 120);
        target.AddBuff(ModContent.BuffType<CrushDepth>(), 120);
        if (Projectile.Calamity().stealthStrike)
        {
            target.AddBuff(ModContent.BuffType<HeavyBleeding>(), 120);
        }
    }

    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        Collision.HitTiles(Projectile.position + Projectile.velocity, Projectile.velocity, Projectile.width, Projectile.height);
        SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
        Projectile.Kill();
        return false;
    }
}
