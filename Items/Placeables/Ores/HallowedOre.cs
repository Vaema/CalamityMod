using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.Ores;

public class HallowedOre : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 100;
        ItemID.Sets.SortingPriorityMaterials[Type] = 89; // Hallowed Bar
    }

    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.Ores.HallowedOre>());
        Item.value = Item.sellPrice(silver: 12);
        Item.rare = ItemRarityID.Pink;
    }

    public override void AddRecipes()
    {
        Recipe r = Recipe.Create(ItemID.HallowedBar);
        r.AddIngredient<HallowedOre>(4).
        AddTile(TileID.AdamantiteForge).
        Register();
    }
}
