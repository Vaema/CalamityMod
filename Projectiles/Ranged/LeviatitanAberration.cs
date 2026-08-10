using CalamityMod.Buffs.StatDebuffs;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Ranged;

public class LeviatitanAberration : ModProjectile, ILocalizedModType
{
    public new string LocalizationCategory => "Projectiles.Ranged";
    public override string Texture => "CalamityMod/Projectiles/Summon/GastricBelcher";
    public override void SetStaticDefaults()
    {
        Main.projFrames[Type] = 4;
        ProjectileID.Sets.CultistIsResistantTo[Type] = true;
    }
    public override void SetDefaults()
    {
        Projectile.width = 40;
        Projectile.height = 40;
        Projectile.aiStyle = ProjAIStyleID.Arrow;
        Projectile.friendly = true;
        Projectile.tileCollide = false;
        Projectile.DamageType = DamageClass.Ranged;
        Projectile.penetrate = 1;
        Projectile.timeLeft = 600;
        AIType = ProjectileID.Bullet;
    }

    public override void AI()
    {
        CalamityUtils.HomeInOnNPC(Projectile, true, 500f, 16f, 8f);

        //Rotation
        Projectile.spriteDirection = Projectile.direction = (Projectile.velocity.X > 0).ToDirectionInt();
        Projectile.rotation = Projectile.velocity.ToRotation() + (Projectile.spriteDirection == 1 ? 0f : MathHelper.Pi) + MathHelper.ToRadians(360) * Projectile.direction;

        //Update frames
        if (Projectile.frameCounter++ % 6 == 0)
        {
            Projectile.frame++;
        }
        if (Projectile.frame >= Main.projFrames[Type])
        {
            Projectile.frame = 0;
        }
        for (int i = 0; i < 3; i++)
        {
            int bloodDust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height / 2, DustID.Blood, 0f, 0f, 100, default, 1f);
            Main.dust[bloodDust].noGravity = true;
            Main.dust[bloodDust].velocity *= 0.5f;
            Main.dust[bloodDust].velocity += Projectile.velocity * 0.1f;
        }
    }
    public override void OnSpawn(IEntitySource source)
    {
        for (int i = 0; i < 36; i++)
        {
            Dust minion = Dust.NewDustPerfect(Projectile.Center, DustID.MoonBoulder);
            minion.velocity = (MathHelper.TwoPi * i / 36f).ToRotationVector2() * 9f;
            minion.scale = Main.rand.NextFloat(1.4f, 1.6f);
            minion.noGravity = true;
        }
    }
    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        target.AddBuff(ModContent.BuffType<ArmorCrunch>(), 120);
        SoundEngine.PlaySound(SoundID.NPCDeath1 with { Volume = SoundID.NPCDeath1.Volume * 0.5f }, Projectile.Center);
    }
    public override void OnKill(int timeLeft)
    {
        for (int d = 0; d < 15; ++d)
        {
            int idx = Dust.NewDust(Projectile.Center - Vector2.One * 10f, 50, 50, DustID.Blood, 0f, -2f, 0, default, 1f);
            Dust dust = Main.dust[idx];
            dust.velocity /= 2f;
        }
    }
}
