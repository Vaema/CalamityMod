using CalamityMod.Buffs.DamageOverTime;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Magic;

public class PlagueFang : ModProjectile, ILocalizedModType
{
    public new string LocalizationCategory => "Projectiles.Magic";
    public override void SetDefaults()
    {
        Projectile.width = 16;
        Projectile.height = 16;
        Projectile.friendly = true;
        Projectile.DamageType = DamageClass.Magic;
        Projectile.alpha = 255;
        Projectile.penetrate = 6;
        Projectile.timeLeft = 300;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 10;
    }

    public override void AI()
    {
        Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        if (Projectile.alpha > 0)
        {
            Projectile.alpha -= 50;
        }
        if (Projectile.alpha < 0)
        {
            Projectile.alpha = 0;
        }
        if (Projectile.alpha == 0)
        {
            int plagued = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.PoisonStaff, Projectile.velocity.X, Projectile.velocity.Y, 100, default, 1f);
            Main.dust[plagued].noGravity = true;
            Main.dust[plagued].velocity *= 0.6f;
            Main.dust[plagued].velocity -= Projectile.velocity * 0.4f;

            int venomed = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.VenomStaff, Projectile.velocity.X, Projectile.velocity.Y, 100, default, 1f);
            Main.dust[venomed].noGravity = true;
            Main.dust[venomed].velocity *= 0.2f;
            Main.dust[venomed].velocity -= Projectile.velocity * 0.4f;
        }
    }

    public override Color? GetAlpha(Color lightColor)
    {
        if (Projectile.alpha > 0)
        {
            return Color.Transparent;
        }
        return null;
    }

    public override void OnKill(int timeLeft)
    {
        SoundEngine.PlaySound(SoundID.Dig, Projectile.Center);
        for (int i = 0; i < 7; i++)
        {
            int killPlague = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.PoisonStaff, 0f, 0f, 100, default, 1f);
            Main.dust[killPlague].noGravity = true;
            Main.dust[killPlague].velocity *= 1.2f;
            Main.dust[killPlague].velocity -= Projectile.oldVelocity * 0.3f;

            int killVenom = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, DustID.VenomStaff, 0f, 0f, 100, default, 1f);
            Dust dust = Main.dust[killVenom];
            dust.noGravity = true;
            dust.velocity *= 1.2f;
            dust.velocity -= Projectile.oldVelocity * 0.3f;
        }
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        //Doze - I gave this long debuff infliction times due to the lack of weapons that inflict debuffs for a decent time
        //Most common vanilla debuffs have a way to inflict them for 15, 20, or even 30 seconds
        //Poison and Venom staff in vanilla do 30 seconds, so this should too as it's a spiritual successor to them.
        target.AddBuff(ModContent.BuffType<Plague>(), CalamityUtils.SecondsToFrames(30));
    }
}
