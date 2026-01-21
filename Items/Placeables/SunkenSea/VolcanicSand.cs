using CalamityMod.Projectiles.Typeless;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.SunkenSea
{
    public class VolcanicSand : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 100;
            ItemID.Sets.SandgunAmmoProjectileData[Type] = new(ModContent.ProjectileType<VolcanicSandBallGun>(), 0);
        }

        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.SunkenSea.VolcanicSand>());
            Item.ammo = AmmoID.Sand;
            Item.notAmmo = true;
        }
    }
}
