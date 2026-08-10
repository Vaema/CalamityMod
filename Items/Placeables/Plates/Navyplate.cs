using CalamityMod.Items.Placeables.SunkenSea;
using CalamityMod.Items.Placeables.Walls;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.Plates;

public class Navyplate : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 100;
        ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<Cinderplate>();
    }

    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.Plates.Navyplate>());
        Item.value = Item.sellPrice(silver: 3);
        Item.rare = ItemRarityID.Orange;
    }

    public override void AddRecipes()
    {
        CreateRecipe(25).
            AddIngredient(ItemID.Obsidian, 25).
            AddIngredient<SeaPrism>().
            AddTile(TileID.Hellforge).
            Register();
        CreateRecipe().
            AddIngredient<NavyplateWall>(4).
            AddTile(TileID.WorkBenches).
            DisableDecraft().
            Register();
    }
}
