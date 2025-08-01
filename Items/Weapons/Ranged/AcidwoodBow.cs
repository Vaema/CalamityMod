using CalamityMod.Items.Placeables.FurnitureAcidwood;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Ranged
{
    public class AcidwoodBow : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Ranged";
        public override void SetDefaults()
        {
            Item.damage = 8;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 20;
            Item.height = 50;
            Item.useTime = 27;
            Item.useAnimation = 27;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 0f;
            Item.value = CalamityGlobalItem.RarityWhiteBuyPrice;
            Item.rare = ItemRarityID.White;
            Item.UseSound = SoundID.Item5;
            Item.autoReuse = true;
            Item.shoot = ProjectileID.WoodenArrowFriendly;
            Item.shootSpeed = 6.6f;
            Item.useAmmo = AmmoID.Arrow;
            Item.Calamity().canFirePointBlankShots = true;
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
