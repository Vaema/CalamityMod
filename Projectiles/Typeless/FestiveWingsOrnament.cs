using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Typeless;

public class FestiveWingsOrnament : ModProjectile, ILocalizedModType
{
    public new string LocalizationCategory => "Projectiles.Typeless";
    public override string Texture => "Terraria/Images/Projectile_" + ProjectileID.OrnamentFriendly;

    internal static readonly SoundStyle JingleSound = new("CalamityMod/Sounds/Item/FestiveJingle") { Volume = 0.25f, PitchVariance = 0.5f };

    public override void SetStaticDefaults() => Main.projFrames[Type] = Main.projFrames[ProjectileID.OrnamentFriendly];

    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 22;
        Projectile.friendly = true;
        Projectile.timeLeft = 420;
    }

    public override void OnSpawn(IEntitySource source)
    {
        Projectile.frame = Main.rand.Next(Main.projFrames[Type]);

        // Vanilla's sprite sheet is slightly messed up so the "center" is the bottom
        SoundEngine.PlaySound(JingleSound, Projectile.Bottom);

        Player owner = Main.player[Projectile.owner];
        if (owner == null || !owner.active)
            return;

        Vector2 direction = owner.SafeDirectionTo(Projectile.Bottom);
        Projectile.rotation = direction.ToRotation() + MathHelper.PiOver2;

        int totalPoints = (int)Utils.Remap(Vector2.Distance(Projectile.Bottom, owner.Center), 320f, 512f, 8f, 13f);
        Vector2[] trailPoints = new Vector2[totalPoints + 1];
        trailPoints[0] = owner.Center;
        trailPoints[totalPoints] = Projectile.Bottom;
        for (int i = 1; i < totalPoints; i++)
        {
            trailPoints[i] = Vector2.Lerp(owner.Center, Projectile.Bottom, i / (float)totalPoints) + Main.rand.NextVector2Circular(12f, 12f) + direction.RotatedBy(MathHelper.PiOver2) * Main.rand.NextFloat(-48f, 48f);
        }

        int dustType = DustID.GemRuby - Projectile.frame;
        for (int i = 0; i < totalPoints; i++)
        {
            for (int d = 0; d < 8; d++)
            {
                Dust trail = Dust.NewDustPerfect(Vector2.Lerp(trailPoints[i], trailPoints[i + 1], d / 8f), dustType);
                trail.noGravity = true;
                trail.noLight = true;
            }
        }
    }

    public override void AI()
    {
        Lighting.AddLight(Projectile.Bottom, 0.5f, 0.5f, 0.5f);

        if (Projectile.timeLeft <= 120)
        {
            Projectile.velocity.Y += 0.75f;
            if (Projectile.velocity.Y > 16f)
                Projectile.velocity.Y = 16f;
        }

        for (int i = 0; i < Main.maxPlayers; i++)
        {
            Player player = Main.player[i];
            if (Vector2.Distance(player.Center, Projectile.Bottom) < 32f && player.wingTimeMax > 0f)
            {
                player.wingTime = MathHelper.Clamp(player.wingTime + 75f, 0f, player.wingTimeMax);
                SoundEngine.PlaySound(SoundID.Item29 with { Volume = 0.25f }, player.Center);
                Projectile.Kill();

                int dustType = DustID.GemRuby - Projectile.frame;
                for (int d = 0; d < 36; d++)
                {
                    Dust ring = Dust.NewDustPerfect(Projectile.Bottom, dustType);
                    ring.velocity = (MathHelper.TwoPi * d / 36f).ToRotationVector2() * 6f;
                    ring.noGravity = true;
                    ring.noLight = true;
                }
                break;
            }
        }
    }

    public override bool OnTileCollide(Vector2 oldVelocity)
    {
        SoundEngine.PlaySound(SoundID.Item27, Projectile.Bottom);
        int dustType = DustID.GemRuby - Projectile.frame;
        for (int i = 0; i < 20; i++)
        {
            Dust shatter = Dust.NewDustDirect(Projectile.position, Projectile.width, Projectile.height, dustType);
            shatter.noLight = true;
            shatter.scale = 0.8f;
        }
        return true;
    }
}
