using CalamityMod.Items.Placeables;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Tools
{
    public class AcidwoodPickaxe : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Tools";
        public override void SetDefaults()
        {
            Item.damage = 5;
            Item.knockBack = 1f;
            Item.useTime = 9;
            Item.useAnimation = 18;
            Item.pick = 40;

            Item.DamageType = DamageClass.Melee;
            Item.width = 36;
            Item.height = 36;
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
                AddIngredient<Acidwood>(15).
                AddTile(TileID.WorkBenches).
                Register();
        }
    }
}
