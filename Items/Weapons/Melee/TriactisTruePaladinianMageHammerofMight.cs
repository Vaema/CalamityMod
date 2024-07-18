using CalamityMod.Items.Materials;
using CalamityMod.Projectiles.Melee;
using CalamityMod.Rarities;
using CalamityMod.Tiles.Furniture.CraftingStations;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Melee
{
    [LegacyName("TriactisTruePaladinianMageHammerofMightMelee")]
    public class TriactisTruePaladinianMageHammerofMight : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Melee";

        public int FlareCount = 0;

        public override void SetDefaults()
        {
            Item.width = 168;
            Item.height = 168;
            Item.damage = 5000;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.autoReuse = true;
            Item.useAnimation = Item.useTime = 36;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 14f;
            Item.UseSound = SoundID.Item1;
            Item.DamageType = DamageClass.MeleeNoSpeed;
            Item.shoot = ModContent.ProjectileType<TriactisHammerProj>();
            Item.shootSpeed = 25f;

            Item.value = CalamityGlobalItem.RarityHotPinkBuyPrice;
            Item.rare = ModContent.RarityType<HotPink>();
            Item.Calamity().devItem = true;
        }

        public override void UpdateInventory(Player player)
        {
            // Reset flares if switched to a different instance of the same weapon (switching to other weapons/tools is fair game)
            if (player.ActiveItem() != Item && player.ActiveItem().type == Type)
                FlareCount = 0;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<GalaxySmasher>().
                AddIngredient(ItemID.SoulofMight, 30).
                AddIngredient<ShadowspecBar>(5).
                AddTile<DraedonsForge>().
                Register();
        }
    }
}
