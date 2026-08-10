using Microsoft.Xna.Framework;
using CalamityMod.Particles;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria;
using CalamityMod.NPCs.SupremeCalamitas;

namespace CalamityMod.Projectiles.Typeless;

public class SCalAltarArenaVisual : ModProjectile, ILocalizedModType
{
    public new string LocalizationCategory => "Projectiles.Typeless";
    public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

    private const int VisualDuration = 900;
    private Particle topLine;
    private Particle bottomLine;
    private Particle leftLine;
    private Particle rightLine;
    public override void SetStaticDefaults() => ProjectileID.Sets.DrawScreenCheckFluff[Type] = 10000;

    public override void SetDefaults()
    {
        Projectile.width = Projectile.height = 2;
        Projectile.tileCollide = false;
        Projectile.ignoreWater = true;
        Projectile.timeLeft = VisualDuration;
    }

    public override void AI()
    {
        // Spawn in the border lines
        if (topLine == null)
        {
            // Arena is smaller in Death Mode
            float arenaHalfLength = Projectile.ai[0] == 1f ? 1000f : 1250f;
            Vector2 topLeft = Projectile.Center + new Vector2(-arenaHalfLength, -arenaHalfLength);
            Vector2 topRight = Projectile.Center + new Vector2(arenaHalfLength, -arenaHalfLength);
            Vector2 bottomLeft = Projectile.Center + new Vector2(-arenaHalfLength, arenaHalfLength);
            Vector2 bottomRight = Projectile.Center + new Vector2(arenaHalfLength, arenaHalfLength);

            topLine = new BloomLineVFX(topLeft, topRight - topLeft, 1f, Color.Red, VisualDuration, true, true);
            bottomLine = new BloomLineVFX(bottomLeft, bottomRight - bottomLeft, 1f, Color.Red, VisualDuration, true, true);
            leftLine = new BloomLineVFX(topLeft, bottomLeft - topLeft, 1f, Color.Red, VisualDuration, true, true);
            rightLine = new BloomLineVFX(topRight, bottomRight - topRight, 1f, Color.Red, VisualDuration, true, true);
            GeneralParticleHandler.SpawnParticle(topLine);
            GeneralParticleHandler.SpawnParticle(bottomLine);
            GeneralParticleHandler.SpawnParticle(leftLine);
            GeneralParticleHandler.SpawnParticle(rightLine);
        }

        if (NPC.AnyNPCs(ModContent.NPCType<SupremeCalamitas>()))
            Projectile.Kill();
    }

    public override void OnKill(int timeLeft)
    {
        topLine.Time = VisualDuration - 30;
        bottomLine.Time = VisualDuration - 30;
        leftLine.Time = VisualDuration - 30;
        rightLine.Time = VisualDuration - 30;
    }
}
