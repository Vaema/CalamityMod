using CalamityMod.Items.Placeables;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Tools
{
    public class AcidwoodAxe : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Tools";
        public override void SetDefaults()
        {
            Item.damage = 8;
            Item.knockBack = 3f;
            Item.useTime = 16;
            Item.useAnimation = 23;
            Item.axe = 55 / 5;

            Item.DamageType = DamageClass.Melee;
            Item.width = 40;
            Item.height = 32;
            Item.useTurn = true;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.value = CalamityGlobalItem.RarityWhiteBuyPrice;
            Item.rare = ItemRarityID.White;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
        }
        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<Acidwood>(10).
                AddTile(TileID.WorkBenches).
                Register();
        }
    }
}
