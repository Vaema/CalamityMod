using System.Collections.Generic;
using CalamityMod.Projectiles.Magic;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Systems.Collections
{
    /// <summary>
    /// A <see cref="ModSystem"/> which contains a <see cref="IList{T}"/> which contains multiple <see cref="IList{T}"/>, containing projectiles which share static immunity frames.
    /// </summary>
    public sealed class SharedStaticIFrames : ModSystem
    {
        public static IList<IList<int>> List { get; private set; }

        public override void OnModLoad()
        {
            List =
            [
                [
                    ProjectileID.Bee,
                    ProjectileID.GiantBee
                ],
                [
                    ProjectileID.VilethornBase,
                    ProjectileID.VilethornTip
                ],
                [
                    ProjectileID.QuarterNote,
                    ProjectileID.EighthNote,
                    ProjectileID.TiedEighthNote
                ],
                [
                    ProjectileID.NorthPoleWeapon,
                    ProjectileID.NorthPoleSpear,
                    ProjectileID.NorthPoleSnowflake,
                ],
                [
                    ProjectileID.SporeTrap,
                    ProjectileID.SporeTrap2,
                    ProjectileID.SporeGas,
                    ProjectileID.SporeGas2,
                    ProjectileID.SporeGas3
                ],
                [
                    ModContent.ProjectileType<AstralCrystal>(),
                    ModContent.ProjectileType<AstralCrystalInvisibleExplosion>()
                ],
            ];
        }

        public override void Unload() => List = null;

        /// <summary>
        /// A shorthand method to check if this projectile shares static iframes.
        /// </summary>
        public static bool Includes(int projType)
        {
            for (int i = 0; i < List.Count; i++)
            {
                if (List[i].Contains(projType)) return true;
            }
            return false;
        }

        /// <summary>
        /// A shorthand method to get which list contains the given type. Returns -1 if none contain it.
        /// </summary>
        public static int IncludesAt(int projType)
        {
            for (int i = 0; i < List.Count; i++)
            {
                if (List[i].Contains(projType)) return i;
            }
            return -1;
        }
    }
}
