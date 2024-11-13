using System.Collections.Generic;
using CalamityMod.Projectiles.Typeless;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod.Systems.Collections
{
    public sealed class GrenadeResistIDList : ModSystem
    {
        public static IList<int> List { get; private set; }

        public override void OnModLoad()
        {
            List =
            [
                ProjectileID.Grenade,
                ProjectileID.StickyGrenade,
                ProjectileID.BouncyGrenade,
                ProjectileID.Bomb,
                ProjectileID.StickyBomb,
                ProjectileID.BouncyBomb,
                ProjectileID.Dynamite,
                ProjectileID.StickyDynamite,
                ProjectileID.BouncyDynamite,
                ProjectileID.Explosives,
                ProjectileID.ExplosiveBunny,
                ProjectileID.PartyGirlGrenade,
                ProjectileID.BombFish,
                ProjectileID.Beenade,
                ProjectileID.Bee,
                ProjectileID.GiantBee,
                ProjectileType<AeroExplosive>(),
                ProjectileID.ScarabBomb,
                ProjectileID.TNTBarrel
            ];
        }

        public override void Unload() => List = null;

        public static bool Includes(int projectileType) => List.Contains(projectileType);
    }
}
