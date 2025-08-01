using CalamityMod.Items.Placeables.FurnitureAcidwood;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Melee
{
    public class AcidwoodSword : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Melee";
        public override void SetDefaults()
        {
            Item.damage = 11;
            Item.width = 36;
            Item.height = 40;
            Item.DamageType = DamageClass.Melee;
            Item.useAnimation = Item.useTime = 18;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 3f;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.useTurn = true;
            Item.value = CalamityGlobalItem.RarityWhiteBuyPrice;
            Item.rare = ItemRarityID.White;
        }
        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<Acidwood>(7).
                AddTile(TileID.WorkBenches).
                Register();
        }
    }
}
