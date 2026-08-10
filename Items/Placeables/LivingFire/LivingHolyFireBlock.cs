using CalamityMod.Items.Materials;
using CalamityMod.Tiles.LivingFire;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.LivingFire;

public class LivingHolyFireBlock : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 100;
    }

    public override void SetDefaults() => Item.DefaultToPlaceableTile(ModContent.TileType<LivingHolyFireBlockTile>());

    public override void PostUpdate()
    {
        Lighting.AddLight((int)((Item.position.X + Item.width / 2) / 16f), (int)((Item.position.Y + Item.height / 2) / 16f), 1f, 1f, 0f);
    }

    public override void AddRecipes()
    {
        CreateRecipe(20).
            AddIngredient(ItemID.LivingFireBlock, 20).
            AddIngredient<UnholyEssence>().
            AddTile(TileID.CrystalBall).
            Register();
    }
}
