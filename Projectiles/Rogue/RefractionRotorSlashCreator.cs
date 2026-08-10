using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Rogue;

public class RefractionRotorSlashCreator : ModProjectile, ILocalizedModType
{
    public new string LocalizationCategory => "Projectiles.Rogue";
    public NPC Target => Main.npc[(int)Projectile.ai[0]];
    public float SlashDirection
    {
        get
        {
            if (Projectile.ai[1] > MathHelper.Pi)
                return Main.rand.NextFloatDirection();
            return Projectile.ai[1] + Main.rand.NextFloatDirection() * 0.2f;
        }
    }

    public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

    public override void SetDefaults()
    {
        Projectile.width = 2;
        Projectile.height = 2;
        Projectile.ignoreWater = false;
        Projectile.tileCollide = false;
        Projectile.penetrate = -1;
        Projectile.timeLeft = 45;
        Projectile.MaxUpdates = 2;
        Projectile.noEnchantmentVisuals = true;
    }

    public override void AI()
    {
        if (Main.myPlayer == Projectile.owner && Projectile.timeLeft % 20 == 19)
        {
            float maxOffset = Target.width * 0.4f;
            if (maxOffset > 300f)
                maxOffset = 300f;
            Vector2 baseDirection = Vector2.UnitX;
            if (Projectile.timeLeft <= 20)
                baseDirection = -Vector2.UnitY;

            Vector2 spawnOffset = SlashDirection.ToRotationVector2();
            spawnOffset *= Main.rand.NextFloatDirection() * maxOffset;
            Vector2 sliceVelocity = spawnOffset.SafeNormalize(Vector2.UnitY) * 0.1f;
            for (int i = 0; i < 2; i++)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromAI(), Projectile.Center + spawnOffset, sliceVelocity, ModContent.ProjectileType<RefractionSlash>(), (int)(Projectile.damage * 0.5), 0f, Projectile.owner);
            }
        }
    }

    public override bool? CanDamage() => false;
}
