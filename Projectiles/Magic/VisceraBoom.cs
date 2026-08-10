using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Items.Weapons.Magic;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Magic;

public class VisceraBoom : ModProjectile, ILocalizedModType
{
    public new string LocalizationCategory => "Projectiles.Magic";
    public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 250;
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Magic;
        Projectile.penetrate = -1;
        Projectile.timeLeft = Viscera.BoomLifetime;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
    }
    public override void AI()
    {
        // Visual effects
        if (Projectile.ai[0] == 0f)
        {
            SoundStyle hitSound = new("CalamityMod/Sounds/NPCKilled/PerfLargeDeath");
            SoundEngine.PlaySound(hitSound with { Volume = 0.5f }, Projectile.Center);
            for (int i = 0; i <= 14; i++)
            {
                BloodParticle blood = new BloodParticle(Projectile.Center, new Vector2(15, 15).RotatedByRandom(100) * Main.rand.NextFloat(0.1f, 0.9f) + new Vector2(0, -7), 60, Main.rand.NextFloat(0.4f, 0.65f), (!ChildSafety.Disabled ? Color.CornflowerBlue : Color.Red));
                GeneralParticleHandler.SpawnParticle(blood);
            }
            Particle bloodsplosion = new CustomPulse(Projectile.Center, Vector2.Zero, (!ChildSafety.Disabled ? Color.CornflowerBlue : Color.DarkRed), "CalamityMod/Particles/DetailedExplosion", Vector2.One, Main.rand.NextFloat(-15f, 15f), 0.16f, 0.87f, (int)(Viscera.BoomLifetime * 0.38f), false);
            GeneralParticleHandler.SpawnParticle(bloodsplosion);
            Particle bloodsplosion2 = new CustomPulse(Projectile.Center, Vector2.Zero, (!ChildSafety.Disabled ? Color.CornflowerBlue : new Color(255, 32, 32)), "CalamityMod/Particles/DustyCircleHardEdge", Vector2.One, Main.rand.NextFloat(-15f, 15f), 0.03f, 0.155f, Viscera.BoomLifetime);
            GeneralParticleHandler.SpawnParticle(bloodsplosion2);

            Projectile.ai[0] = 1f;
        }

        for (int i = 0; i <= 2; i++)
        {
            float speed = Projectile.ai[1] > 0 ? 25 : 15;
            Dust dust = Dust.NewDustPerfect(Projectile.Center, (!ChildSafety.Disabled ? DustID.Cloud : (Main.rand.NextBool() ? 60 : DustID.Blood)));
            dust.scale = Main.rand.NextFloat(1f, 2f) * Utils.GetLerpValue(0, Viscera.BoomLifetime, Projectile.timeLeft, true);
            dust.velocity = new Vector2(speed, speed).RotatedByRandom(100) * Main.rand.NextFloat(0.1f, 0.9f);
            dust.noGravity = true;
        }
    }
    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        target.AddBuff(ModContent.BuffType<BurningBlood>(), 150);
        target.AddBuff(ModContent.BuffType<Laceration>(), 150);
    }

    public override void OnHitPlayer(Player target, Player.HurtInfo info)
    {
        target.AddBuff(ModContent.BuffType<BurningBlood>(), 150);
        target.AddBuff(ModContent.BuffType<Laceration>(), 150);
    }
}
