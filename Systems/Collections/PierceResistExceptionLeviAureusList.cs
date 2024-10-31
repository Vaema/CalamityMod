using System.Collections.Generic;
using CalamityMod.Projectiles.Magic;
using CalamityMod.Projectiles.Ranged;
using CalamityMod.Projectiles.Rogue;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod.Systems.Collections
{
    public sealed class PierceResistExceptionLeviAureusList : ModSystem
    {
        public static IList<int> List { get; private set; }

        public override void OnModLoad()
        {
            List =
            [
                ProjectileID.NettleBurstEnd,
                ProjectileID.NettleBurstLeft,
                ProjectileID.NettleBurstRight,
                ProjectileType<AnahitasArpeggioNote>(),
                ProjectileType<AtlantisSpear>(),
                ProjectileType<AuroraFire>(),
                ProjectileType<BallisticPoisonCloud>(),
                ProjectileType<DuststormCloudHitbox>()
            ];
        }

        public override void Unload() => List = null;

        public static bool Includes(int projectileType) => List.Contains(projectileType);
    }
}
