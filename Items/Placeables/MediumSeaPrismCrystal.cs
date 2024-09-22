using CalamityMod.Items.Placeables.Walls;
using CalamityMod.Projectiles.Typeless;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables
{
    public class MediumSeaPrismCrystal : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 15;
            ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<SeaPrism>();
        }

        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.SunkenSea.MediumSeaPrismCrystal>());
            Item.ammo = AmmoID.Sand;
            Item.notAmmo = true;
        }
    }
}
