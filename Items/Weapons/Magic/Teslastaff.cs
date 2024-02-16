using CalamityMod.Items.Materials;
using CalamityMod.Projectiles.Magic;
using CalamityMod.Rarities;
using CalamityMod.Sounds;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Magic
{
    public class Teslastaff : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Magic";
        public override void SetStaticDefaults()
        {
            Item.staff[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 38;
            Item.damage = 166;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 10;
            Item.useTime = 10;
            Item.useAnimation = 10;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.UseSound = CommonCalamitySounds.LightningSound with { Pitch = 1.1f };
            Item.noMelee = true;
            Item.channel = true;
            Item.knockBack = 0f;
            Item.value = CalamityGlobalItem.Rarity12BuyPrice;
            Item.rare = ModContent.RarityType<Turquoise>();
            Item.Calamity().donorItem = true;
            Item.shoot = ModContent.ProjectileType<Teslabeam>();
            Item.shootSpeed = 30f;
            Item.reuseDelay = 60;
        }

        public override bool CanUseItem(Player player) => player.ownedProjectileCounts[Item.shoot] <= 0;

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ItemID.ThunderStaff).
                AddRecipeGroup("AnyCopperBar", 20).
                AddIngredient<ArmoredShell>(2).
                AddIngredient<CoreofSunlight>(6).
                AddTile(TileID.LunarCraftingStation).
                Register();
        }
    }
}
