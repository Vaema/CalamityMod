using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables.Walls;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.FurnitureWulfrum;

public class ChargedWulfrumEnergyBarrier : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 100;
    }

    public override void SetDefaults() => Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.FurnitureWulfrum.ChargedWulfrumEnergyBarrier>());

    public override void AddRecipes()
    {
        CreateRecipe(25).
            AddIngredient(ItemID.Glass, 25).
            AddIngredient<EnergyCore>().
            AddTile(TileID.HeavyWorkBench).
            Register();
        CreateRecipe().
            AddIngredient<ChargedWulfrumEnergyBarrierWall>(4).
            AddTile(TileID.WorkBenches).
            DisableDecraft().
            Register();
    }
}
