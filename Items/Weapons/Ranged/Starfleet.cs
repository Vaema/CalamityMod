using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables.Ores;
using CalamityMod.Projectiles.Ranged;
using CalamityMod.Rarities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Ranged
{
    public class Starfleet : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Ranged";
        public override void SetDefaults()
        {
            Item.width = 76;
            Item.height = 36;
            Item.damage = 888;
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 70;
            Item.useAnimation = 70;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 15f;
            Item.value = CalamityGlobalItem.RarityPureGreenBuyPrice;
            Item.rare = ModContent.RarityType<PureGreen>();
            Item.UseSound = SoundID.Item92;
            Item.channel = true;
            Item.noUseGraphic = true;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<PlasmaBlast>();
            Item.shootSpeed = 12f;
            Item.useAmmo = AmmoID.FallenStar;
        }
        // Holdout projectile is spawned when holding the item, so using the item does nothing
        public override bool CanUseItem(Player player) => false;
        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ItemID.SuperStarCannon).
                AddIngredient<StarSputter>().
                AddIngredient<RuinousSoul>(4).
                AddTile(TileID.MythrilAnvil).
                Register();
        }
    }
}
