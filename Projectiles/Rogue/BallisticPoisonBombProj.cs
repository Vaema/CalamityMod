using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Rogue;

public class BallisticPoisonBombProj : ModProjectile, ILocalizedModType
{
    public new string LocalizationCategory => "Projectiles.Rogue";
    public override void SetDefaults()
    {
        Projectile.width = 24;
        Projectile.height = 24;
        Projectile.friendly = true;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 200;
        Projectile.tileCollide = false;
        Projectile.DamageType = RogueDamageClass.Instance;
        Projectile.ignoreWater = true;
        Projectile.usesIDStaticNPCImmunity = true;
        Projectile.idStaticNPCHitCooldown = 10;
    }

    public override void AI()
    {
        if (Main.rand.NextBool(6))
        {
            Dust.NewDust(Projectile.position + Projectile.velocity, Projectile.width, Projectile.height, DustID.Demonite, Projectile.velocity.X * 0.5f, Projectile.velocity.Y * 0.5f);
        }
        Projectile.StickToTiles(true, false);
        if (Projectile.owner == Main.myPlayer && Projectile.timeLeft <= 3)
        {
            Projectile.tileCollide = false;
            Projectile.ai[1] = 0f;
            Projectile.alpha = 255;
            Projectile.ExpandHitboxBy(128);
        }
        Projectile.ai[0] += 1f;
        if (Projectile.ai[0] > 10f)
        {
            Projectile.ai[0] = 10f;
            if (Projectile.velocity.Y == 0f && Projectile.velocity.X != 0f)
            {
                Projectile.velocity.X *= 0.97f;
                if (Math.Abs(Projectile.velocity.X) < 0.01f)
                {
                    Projectile.velocity.X = 0f;
                    Projectile.netUpdate = true;
                }
            }
            Projectile.velocity.Y += 0.2f;
        }
        Projectile.rotation += Projectile.velocity.X * 0.1f;
    }

    public override void OnKill(int timeLeft)
    {
        Projectile.ExpandHitboxBy(128);
        SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);
        if (Projectile.owner == Main.myPlayer)
        {
            for (int s = 0; s < 3; s++)
            {
                Vector2 velocity = CalamityUtils.RandomVelocity(100f, 70f, 100f);
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, velocity, ModContent.ProjectileType<BallisticPoisonBombSpike>(), Projectile.damage, 0f, Projectile.owner);
            }
            int cloudAmt = Projectile.Calamity().stealthStrike ? Main.rand.Next(7, 10+1) : Main.rand.Next(3, 5+1);
            for (int c = 0; c < cloudAmt; c++)
            {
                Vector2 velocity = CalamityUtils.RandomVelocity(100f, 10f, 200f, 0.01f);
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, velocity, ModContent.ProjectileType<BallisticPoisonCloud>(), (int)(Projectile.damage * 0.6f), 0f, Projectile.owner, 0f, Projectile.Calamity().stealthStrike ? 1f : 0f);
            }
        }
        for (int d = 0; d < 5; d++)
        {
            int boom = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Demonite, 0f, 0f, 100, default, 2f);
            Main.dust[boom].velocity *= 3f;
            if (Main.rand.NextBool())
            {
                Main.dust[boom].scale = 0.5f;
                Main.dust[boom].fadeIn = 1f + (float)Main.rand.Next(10) * 0.1f;
            }
        }
        for (int d = 0; d < 9; d++)
        {
            int fire = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 100, default, 3f);
            Main.dust[fire].noGravity = true;
            Main.dust[fire].velocity *= 5f;
            fire = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.Torch, 0f, 0f, 100, default, 2f);
            Main.dust[fire].velocity *= 2f;
        }

        if (!Main.dedServ)
        {
            Vector2 goreSource = Projectile.Center;
            int goreAmt = 3;
            Vector2 source = new Vector2(goreSource.X - 24f, goreSource.Y - 24f);
            for (int goreIndex = 1; goreIndex <= goreAmt; goreIndex++)
            {
                float velocityMult = 0.33f * goreIndex;
                int type = Main.rand.Next(61, 64);
                int smoke = Gore.NewGore(Projectile.GetSource_Death(), source, Main.rand.NextVector2CircularEdge(2f, 2f), type, 1f);
                Gore gore = Main.gore[smoke];
                gore.velocity *= velocityMult;
            }
        }
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        target.AddBuff(BuffID.Venom, 180);
        Projectile.Kill();
    }
    public override void OnHitPlayer(Player target, Player.HurtInfo info)
    {
        target.AddBuff(BuffID.Venom, 180);
        Projectile.Kill();
    }
}
