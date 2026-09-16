using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Patches;

/// <summary>
/// Exact early broad reject for Calamity's BrimstoneBarrage. Its audited
/// Colliding override is a radius-(10 * scale) circle against the target
/// rectangle. A rectangle outside the circle's bounding square cannot collide,
/// so Projectile.Damage may skip the remaining CanHit/collision path. Every
/// candidate touching the square stays on the original Calamity path.
/// </summary>
internal static class BrimstoneBarrageCollisionFastPatch
{
    private static int projectileType = -1;
    private static bool enabled;
    private static bool available;
    private static string compatibility = "content-not-resolved";
    private static long checks;
    private static long rejected;
    private static long passed;

    public static bool Enabled => enabled;
    public static bool Available => available;
    public static string Compatibility => compatibility;
    public static string Mode => enabled ? "exact-circle-bounds-before-original" : "calamity-original";
    public static long Checks => checks;
    public static long Rejected => rejected;
    public static long Passed => passed;

    public static void Load(Mod mod)
    {
        enabled = false;
        available = false;
        compatibility = "content-not-resolved";
        projectileType = -1;
        ResetCounters();
    }

    public static void PostSetupContent()
    {
        if (!ModLoader.TryGetMod("CalamityMod", out Mod? calamity) ||
            !calamity.TryFind("BrimstoneBarrage", out ModProjectile barrage))
        {
            available = false;
            compatibility = "missing-calamity-brimstonebarrage";
            return;
        }

        projectileType = barrage.Type;
        available = true;
        compatibility = "audited-calamity-2.2.2-circle-radius-10xscale";
    }

    public static void Unload()
    {
        enabled = false;
        available = false;
        compatibility = "unloaded";
        projectileType = -1;
        ResetCounters();
    }

    public static void SetEnabled(bool value)
    {
        enabled = value && available;
        ResetCounters();
    }

    public static void ResetCounters()
    {
        checks = 0;
        rejected = 0;
        passed = 0;
    }

    /// <summary>
    /// Returns true when this projectile is the audited target. reject is true
    /// only when the original circular collision is mathematically impossible.
    /// </summary>
    public static bool TryHandle(Projectile projectile, NPC npc, out bool reject)
    {
        reject = false;
        if (!enabled || projectile.type != projectileType)
            return false;

        checks++;
        float radius = 10f * projectile.scale;
        if (radius <= 0f)
        {
            rejected++;
            reject = true;
            return true;
        }

        Rectangle target = new(
            (int)(npc.position.X + npc.netOffset.X),
            (int)(npc.position.Y + npc.netOffset.Y),
            npc.width,
            npc.height);
        if (npc.type == NPCID.SolarCrawltipedeTail)
            target.Inflate(8, 8);

        Vector2 center = projectile.Center;
        reject = center.X + radius < target.Left ||
                 center.X - radius > target.Right ||
                 center.Y + radius < target.Top ||
                 center.Y - radius > target.Bottom;
        if (reject)
            rejected++;
        else
            passed++;
        return true;
    }
}
