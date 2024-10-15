using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Systems.Collections
{
    /// <summary>
    /// A <see cref="ModSystem"/> which contains a <see cref="IList{T}"/> that has the Projectile IDs of all projectiles who are buffed Dungeon projectiles.
    /// </summary>
    public sealed class BuffedDungeonProjectilesList : ModSystem
    {
        public static IList<int> List { get; private set; }

        public override void OnModLoad()
        {
            List =
            [
                ProjectileID.PaladinsHammerHostile,
                ProjectileID.ShadowBeamHostile,
                ProjectileID.InfernoHostileBolt,
                ProjectileID.InfernoHostileBlast,
                ProjectileID.LostSoulHostile,
                ProjectileID.SniperBullet,
                ProjectileID.RocketSkeleton,
                ProjectileID.BulletDeadeye,
                ProjectileID.Shadowflames
            ];
        }

        public override void Unload() => List = null;

        /// <summary>
        /// A shorthand method to check if this projectile is a buffed Dungeon projectile.
        /// </summary>
        public static bool IsProjectileBuffed(Projectile proj) => List.Contains(proj.type);
    }
}
