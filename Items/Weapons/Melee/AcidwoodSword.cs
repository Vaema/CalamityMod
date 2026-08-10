using CalamityMod.Items.Placeables.FurnitureAcidwood;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Melee;

public class AcidwoodSword : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Weapons.Melee";
    public override void SetDefaults()
    {
        Item.CloneDefaults(ItemID.ShadewoodSword); // Ash Wood > Acidwood > Shadewood
        Item.width = 36;
        Item.height = 40;
        Item.damage = 12;
        Item.useAnimation = Item.useTime = 18;
        Item.knockBack = 3f;
    }
    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient<Acidwood>(7).
            AddTile(TileID.WorkBenches).
            Register();
    }
}
