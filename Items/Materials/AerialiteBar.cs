using CalamityMod.Items.Placeables.Ores;
using CalamityMod.Tiles;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Materials;

public class AerialiteBar : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Materials";
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 25;
        ItemID.Sets.SortingPriorityMaterials[Type] = 69; // Hellstone
    }

    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<AerialiteBarTile>());
        Item.value = Item.sellPrice(silver: 30);
        Item.rare = ItemRarityID.Orange;
    }
    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient<AerialiteOre>(3).
            AddTile(TileID.Furnaces).
            Register();
    }
}
