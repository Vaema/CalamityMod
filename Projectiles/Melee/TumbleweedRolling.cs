using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Projectiles.Melee;

public class TumbleweedRolling : ModProjectile, ILocalizedModType
{
    public new string LocalizationCategory => "Projectiles.Melee";
    public override string Texture => "CalamityMod/Projectiles/Melee/MaceFlails/TumbleweedFlail";

    public static int Lifetime = 120;
    public static int Rolltime = 30; // How long it remains gravity-defiant if it doesn't hit tiles

    public ref float RollState => ref Projectile.ai[0]; // 1f: falling

    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 42;
        Projectile.friendly = true;
        Projectile.penetrate = 6;
        Projectile.timeLeft = Lifetime;
        Projectile.DamageType = DamageClass.MeleeNoSpeed;
        Projectile.usesLocalNPCImmunity = true;
        Projectile.localNPCHitCooldown = 15;
    }

    public override void AI()
    {
        Projectile.rotation += Projectile.velocity.X * 0.05f;

        if (Projectile.timeLeft < Lifetime - Rolltime)
            RollState = 1f;

        if (RollState == 1f && Projectile.velocity.Y < 10f)
            Projectile.velocity.Y += 0.6f;

        Dust sand = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Sand, Alpha: 100, Scale: Main.rand.NextFloat(0.6f, 1.2f));
        sand.noGravity = RollState != 1f;
        sand.velocity = Projectile.velocity * 0.5f;
    }

    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        if (RollState != 1f)
            RollState = 1f;

        Projectile.penetrate--;
        Projectile.numHits++;

        if (oldVelocity.Y != Projectile.velocity.Y)
            Projectile.velocity.Y = MathHelper.Clamp(oldVelocity.Y * -0.5f * Projectile.penetrate, -16f, -2f);

        Point scanAreaStart = Projectile.TopLeft.ToTileCoordinates();
        Point scanAreaEnd = Projectile.BottomRight.ToTileCoordinates();
        Projectile.CreateImpactExplosion(2, Projectile.Center, ref scanAreaStart, ref scanAreaEnd, Projectile.width, out bool causedShockwaves);
        Projectile.CreateImpactExplosion2_FlailTileCollision(Projectile.Center, causedShockwaves, Projectile.velocity);
        TumbleImpactEffects();
        return false;
    }

    public override void OnKill(int timeLeft) => TumbleImpactEffects();

    public void TumbleImpactEffects()
    {
        float impactIntensity = 1f - Projectile.numHits * 0.08f;
        SoundEngine.PlaySound(SoundID.NPCDeath15 with { Volume = impactIntensity }, Projectile.Center);
        for (int i = 0; i < (int)(8 * impactIntensity); i++)
        {
            Dust tumbleDust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.Sand, Alpha: 100, Scale: 1.2f);
            tumbleDust.velocity *= 3f;
            if (Main.rand.NextBool())
            {
                tumbleDust.scale = 0.5f;
                tumbleDust.fadeIn = Main.rand.NextFloat(1f, 1.1f);
            }

            tumbleDust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.UnusedBrown, Alpha: 100, Scale: 1.7f);
            tumbleDust.noGravity = true;
            tumbleDust.velocity *= 5f;

            tumbleDust = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, DustID.UnusedBrown, Alpha: 100, Scale: 1f);
            tumbleDust.velocity *= 2f;
        }
    }
}
