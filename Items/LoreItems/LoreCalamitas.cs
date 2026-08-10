using CalamityMod.Items.Placeables.Furniture.Trophies;
using CalamityMod.Rarities;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.LoreItems;

[LegacyName("KnowledgeCalamitas")]
public class LoreCalamitas : LoreItem
{
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();
    }

    public override void SetDefaults()
    {
        Item.width = 20;
        Item.height = 20;
        Item.consumable = false;
        Item.rare = ModContent.RarityType<CalamityRed>();
    }

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient<SupremeCalamitasTrophy>().
            AddTile(TileID.Bookcases).
            Register();
    }
}
