using CalamityMod.Items.Placeables.FurnitureAcidwood;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Ranged;

public class AcidwoodBow : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Weapons.Ranged";
    public override void SetDefaults()
    {
        Item.CloneDefaults(ItemID.ShadewoodBow); // Ash Wood > Acidwood > Shadewood
        Item.width = 20;
        Item.height = 50;
        Item.damage = 9;
        Item.useAnimation = Item.useTime = 27;
    }

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient<Acidwood>(10).
            AddTile(TileID.WorkBenches).
            Register();
    }
}
