using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Systems.Collections
{
    /// <summary>
    /// A <see cref="ModSystem"/> that contains a <see cref="IList{T}"/> of all Projectile Types that receive Post-DoG Event Buff
    /// </summary>
    public sealed class EventProjectileBuffList : ModSystem
    {
        public static IList<int> List { get; private set; }

        public override void OnModLoad()
        {
            List =
            [
                ProjectileID.FlamingWood,
                ProjectileID.GreekFire1,
                ProjectileID.GreekFire2,
                ProjectileID.GreekFire3,
                ProjectileID.FlamingScythe,
                ProjectileID.FlamingArrow,
                ProjectileID.PineNeedleHostile,
                ProjectileID.OrnamentHostile,
                ProjectileID.OrnamentHostileShrapnel,
                ProjectileID.FrostWave,
                ProjectileID.FrostShard,
                ProjectileID.Missile,
                ProjectileID.Present,
                ProjectileID.Spike,
                ProjectileID.BulletDeadeye,
                ProjectileID.EyeLaser,
                ProjectileID.Nail,
                ProjectileID.DrManFlyFlask
            ];
        }

        public override void Unload() => List = null;

        public static bool IsEventBuffProjectile(Projectile projectile) => List.Contains(projectile.type);
    }
}
