using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.Furniture
{
    public class CrimsonEffigy : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";

        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.Furniture.CrimsonEffigy>());
            Item.value = Item.sellPrice(gold: 2);
            Item.rare = ItemRarityID.Orange;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<CorruptionEffigy>().
                AddTile(TileID.TinkerersWorkbench).
                AddCondition(Condition.InGraveyard).
                Register()
                .DisableDecraft();
        }
    }
}
