using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.SunkenSea;

public class LimeCoral : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 100;
        ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<MagentaCoral>();
    }

    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.SunkenSea.LimeCoral>());
        Item.rare = ItemRarityID.Blue;
    }

    //public override void AddRecipes()
    //{
    //    CreateRecipe(4).
    //        AddIngredient(ItemID.PinkPearl).
    //        AddTile(TileID.HeavyWorkBench).
    //        Register();
    //}
}
