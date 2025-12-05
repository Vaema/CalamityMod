using CalamityMod.Tiles.DraedonStructures;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.DraedonStructures
{
    public class AgedSecurityChest : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<AgedSecurityChestTile>());
            Item.value = Item.sellPrice(silver: 1); // Non-standard Draedon's furniture: uses Chest prices
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<RustedPlating>(8).
                AddRecipeGroup("IronBar", 2).
                AddTile(TileID.Anvils).
                Register();
        }
    }
}
