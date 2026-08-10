using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Rogue;

public class ApoctolithExplosion : ModProjectile, ILocalizedModType
{
    public new string LocalizationCategory => "Projectiles.Rogue";
    public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 300;
        Projectile.friendly = true;
        Projectile.DamageType = RogueDamageClass.Instance;
        Projectile.tileCollide = false;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 20;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = -1;
    }

    public override void AI()
    {
        if (Projectile.ai[0] == 0f)
        {
            float screenShakePower = 7 * Utils.GetLerpValue(1300f, 0f, Projectile.Distance(Main.LocalPlayer.Center), true);
            Main.LocalPlayer.SetScreenshake(screenShakePower);

            CustomPulse blastRing1 = new(Projectile.Center, Vector2.Zero, Color.Blue, "CalamityMod/Particles/FlameExplosion", Vector2.One, Main.rand.NextFloat(-10, 10), 0.05f, 0.35f, 15);
            GeneralParticleHandler.SpawnParticle(blastRing1);

            CustomPulse blastRing2 = new(Projectile.Center, Vector2.Zero, Color.Blue, "CalamityMod/Particles/FlameExplosion", new(1f, 0.5f), 0, 0.05f, 0.35f, 20);
            GeneralParticleHandler.SpawnParticle(blastRing2);

            Projectile.ai[0] = 1f;
        }
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => target.AddBuff(ModContent.BuffType<CrushDepth>(), 240);
    public override void OnHitPlayer(Player target, Player.HurtInfo info) => target.AddBuff(ModContent.BuffType<CrushDepth>(), 240);
}
