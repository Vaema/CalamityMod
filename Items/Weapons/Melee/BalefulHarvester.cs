using CalamityMod.Items.BaseItems;
using CalamityMod.Projectiles.Melee;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Melee
{
    public class BalefulHarvester : CustomUseProjItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Melee";

        public override void SetDefaults()
        {
            Item.width = 92;
            Item.height = 106;
            Item.damage = 120;
            Item.DamageType = TrueMeleeDamageClass.Instance;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.useAnimation = Item.useTime = 80;
            Item.useTurn = true;
            Item.knockBack = 8f;
            Item.UseSound = null;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<BalefulHarvesterHoldout>();
            Item.noMelee = true;
            Item.channel = true;
            Item.noUseGraphic = true;
            Item.value = CalamityGlobalItem.RarityRedBuyPrice;
            Item.rare = ItemRarityID.Red;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ItemID.TheHorsemansBlade).
                AddIngredient(ItemID.FragmentStardust, 12).
                AddTile(TileID.LunarCraftingStation).
                Register();
        }
    }
}
