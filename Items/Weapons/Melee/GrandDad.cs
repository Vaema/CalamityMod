using CalamityMod.Items.BaseItems;
using CalamityMod.Items.Materials;
using CalamityMod.Projectiles.Melee;
using CalamityMod.Rarities;
using CalamityMod.Tiles.Furniture.CraftingStations;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Melee
{
    public class GrandDad : CustomUseProjItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Melee";
        public override void SetDefaults()
        {
            Item.width = 124;
            Item.height = 124;
            Item.damage = 2777;
            Item.DamageType = TrueMeleeDamageClass.Instance;
            Item.useAnimation = 77;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = 77;
            Item.useTurn = true;
            Item.knockBack = 7f;
            Item.autoReuse = true;
            Item.value = CalamityGlobalItem.RarityRedBuyPrice;
            Item.rare = ItemRarityID.Red;

            Item.channel = true;
            Item.shoot = ModContent.ProjectileType<GrandDadHoldout>();
            Item.noUseGraphic = true;
            Item.noMelee = true;
            Item.useStyle = ItemUseStyleID.Shoot;
        }

        public override bool MeleePrefix() => true;
        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<GrandGuardian>().
                AddIngredient<TwistingNether>(3).
                AddTile(TileID.LunarCraftingStation).
                Register();
        }
    }
}
