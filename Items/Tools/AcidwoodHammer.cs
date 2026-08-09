using CalamityMod.Items.Placeables.FurnitureAcidwood;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Tools
{
    public class AcidwoodHammer : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Tools";
        public override void SetDefaults()
        {
            Item.CloneDefaults(ItemID.ShadewoodHammer); // Ash Wood > Acidwood > Shadewood
            Item.width = 40;
            Item.height = 40;
            Item.damage = 8;
            Item.useTime = 8; // Due to vanilla tool tweaks; otherwise apply 40% hammer power
        }
        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<Acidwood>(8).
                AddTile(TileID.WorkBenches).
                Register();
        }
    }
}
