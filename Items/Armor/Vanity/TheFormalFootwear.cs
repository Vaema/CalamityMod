using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Armor.Vanity;

[AutoloadEquip(EquipType.Legs)]
public class TheFormalFootwear : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Armor.Vanity";
    public override void SetDefaults()
    {
        Item.width = 28;
        Item.height = 20;
        Item.vanity = true;
        Item.rare = ItemRarityID.Blue;
        Item.Calamity().donorItem = true;
    }

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient(ItemID.Silk, 5).
            AddIngredient(ItemID.Leather, 2).
            AddRecipeGroup("AnyGoldBar", 1).
            AddIngredient(ItemID.BlueDye, 1).
            AddTile(TileID.Loom).
            Register();
    }
}
