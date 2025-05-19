using CalamityMod.Items.Materials;
using CalamityMod.Projectiles.Magic;
using CalamityMod.Rarities;
using CalamityMod.Tiles.Furniture.CraftingStations;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Magic
{
    public class Fabstaff : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Magic";
        public override void SetStaticDefaults()
        {
            Item.staff[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 84;
            Item.height = 84;
            Item.damage = 140;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 35;
            Item.useAnimation = Item.useTime = 20;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 5f;

            Item.value = CalamityGlobalItem.RarityHotPinkBuyPrice;
            Item.rare = ModContent.RarityType<HotPink>();
            Item.Calamity().devItem = true;

            Item.UseSound = SoundID.Item60;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<FabRay>();
            Item.shootSpeed = 13.5f;
        }


        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ItemID.RainbowRod).
                AddIngredient<ShadowspecBar>(5).
                AddIngredient<Necroplasm>(10).
                AddTile<DraedonsForge>().
                Register();
        }
    }
}
