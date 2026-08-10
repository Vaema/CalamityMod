using CalamityMod.Items.Weapons.Rogue;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Rogue;

// 18NOV2024: Ozzatron: Nanoblack zero-point energy strikes are very similar to Direct Strikes, but aren't a subclass.
// This is because they maintain a consistent offset and have visuals instead of having customized knockback.
public class NanoblackStrike : ModProjectile, ILocalizedModType
{
    public new string LocalizationCategory => "Projectiles.Rogue";
    public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

    internal bool InvalidTarget => !((int)Projectile.ai[0]).WithinBounds(Main.maxNPCs);
    internal ref float visualXOffset => ref Projectile.ai[1];
    internal ref float visualYOffset => ref Projectile.ai[2];

    public override void SetDefaults()
    {
        Projectile.width = 2;
        Projectile.height = 2;
        Projectile.friendly = true;
        Projectile.DamageType = RogueDamageClass.Instance;
        Projectile.penetrate = 1;
        Projectile.extraUpdates = 0;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.alpha = 255;
        Projectile.timeLeft = 2;
    }

    // On-spawn, the strikes are Rogue class for damage inheritance reasons.
    // Once they exist, they immediately swap to classless, as they are intentionally similar to Direct Strikes and shouldn't proc effects.
    public override bool PreAI()
    {
        Projectile.DamageType = DamageClass.Generic;
        return true;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => ProduceImpactParticles(target);
    public override void OnHitPlayer(Player target, Player.HurtInfo info) => ProduceImpactParticles(target);

    private void ProduceImpactParticles(Entity target)
    {
        Vector2 visualsPos = Projectile.Center + new Vector2(Projectile.ai[1], Projectile.ai[2]);
        float sparkSpeed = 2f;
        // Spark particles invert depending on the scythe's flight direction at the moment their parent tesselations were created.
        float baseRot = Projectile.spriteDirection * MathHelper.PiOver2;
        float scale = 0.014f;
        Color color = NanoblackReaper.ZeroPointImpactColor;

        // Spawn a triangle of three sparks.
        for (int i = 0; i < 3; ++i)
        {
            float rot = baseRot + i * NanoblackReaper.TwoPiOver3;
            Vector2 sparkVel = sparkSpeed * rot.ToRotationVector2();
            Vector2 squashStretch = new(1f, 0.3f);
            Particle spark = new GlowSparkParticle(visualsPos, sparkVel, false, 11, scale, color, squashStretch, true, false, 1.25f);
            GeneralParticleHandler.SpawnParticle(spark);
        }
    }
}
