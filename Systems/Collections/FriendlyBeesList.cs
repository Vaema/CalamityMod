using System.Collections.Generic;
using CalamityMod.Projectiles.Rogue;
using CalamityMod.Projectiles.Summon;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod.Systems.Collections
{
    /// <summary>
    /// A <see cref="ModSystem"/> that contains a <see cref="IList{T}"/> that has all the Projectile IDs of all friendly bees.
    /// </summary>
    public sealed class FriendlyBeesList : ModSystem
    {
        public static IList<int> List { get; private set; }

        public override void OnModLoad()
        {
            List =
            [
                ProjectileID.GiantBee,
                ProjectileID.Bee,
                ProjectileID.Wasp,
                ProjectileType<PlaguenadeBee>(),
                ProjectileType<PlaguePrincess>(),
                ProjectileType<BabyPlaguebringer>(),
                ProjectileType<PlagueBeeSmall>()
            ];
        }

        public override void Unload() => List = null;

        /// <summary>
        /// A shorthand method to check if a projectile is a friendly bee.
        /// </summary>
        public static bool Includes(int projType) => List.Contains(projType);
    }
}
