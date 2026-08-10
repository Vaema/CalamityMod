using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.Dusts;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Rogue;

public class UrchinIrradiation : ModProjectile, ILocalizedModType
{
    public new string LocalizationCategory => "Projectiles.Rogue";
    public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

    public override void SetDefaults()
    {
        Projectile.width = 45;
        Projectile.height = 45;
        Projectile.friendly = true;
        Projectile.ignoreWater = true;
        Projectile.tileCollide = false;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 60;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
        Projectile.DamageType = RogueDamageClass.Instance;
    }

    public override void AI()
    {
        Lighting.AddLight(Projectile.Center, 0f, 0.4f, 0.15f);

        if (Projectile.timeLeft == 60)
        {
            for (int i = 0; i < 40; i++)
            {
                int randDust;
                switch (Main.rand.Next(4))
                {
                    case 0:
                    case 1:
                        randDust = DustID.Water;
                        break;
                    case 2:
                    default:
                        randDust = (int)CalamityDusts.SulphurousSeaAcid;
                        break;
                    case 3:
                        randDust = DustID.GemEmerald;
                        break;
                }

                Dust burst = Dust.NewDustPerfect(Projectile.Center, randDust, Main.rand.NextVector2Circular(7f, 7f), 100, Scale: 1.5f);
                if (randDust == DustID.GemEmerald)
                    burst.scale *= 0.7f;
                burst.noGravity = true;
            }

            MediumMistParticle mist = new(Projectile.Center, Vector2.Zero, new Color(105, 255, 122), Color.Green, 1.2f, 200f, 0.15f * Main.rand.NextBool().ToDirectionInt());
            GeneralParticleHandler.SpawnParticle(mist);
        }
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => target.AddBuff(ModContent.BuffType<Irradiated>(), 360);
    public override void OnHitPlayer(Player target, Player.HurtInfo info) => target.AddBuff(ModContent.BuffType<Irradiated>(), 360);
}
