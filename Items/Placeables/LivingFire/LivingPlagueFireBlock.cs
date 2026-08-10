using CalamityMod.Items.Materials;
using CalamityMod.Tiles.LivingFire;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.LivingFire;

public class LivingPlagueFireBlock : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 100;
    }

    public override void SetDefaults() => Item.DefaultToPlaceableTile(ModContent.TileType<LivingPlagueFireBlockTile>());

    public override void PostUpdate()
    {
        Lighting.AddLight((int)((Item.position.X + Item.width / 2) / 16f), (int)((Item.position.Y + Item.height / 2) / 16f), 0f, 1f, 0.2f);
    }

    public override void AddRecipes()
    {
        CreateRecipe(20).
            AddIngredient(ItemID.LivingFireBlock, 20).
            AddIngredient<PlagueCellCanister>().
            AddTile(TileID.CrystalBall).
            Register();
    }
}
