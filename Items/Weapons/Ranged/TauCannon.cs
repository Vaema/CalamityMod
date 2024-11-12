using CalamityMod.Items.Materials;
using CalamityMod.Projectiles.Ranged;
using CalamityMod.Rarities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Ranged
{
    public class TauCannon : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Ranged";

        public override void SetDefaults()
        {
            CalamityGlobalItem modItem = Item.Calamity();

            Item.damage = 620;
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = Item.useAnimation = 180;
            Item.shoot = ModContent.ProjectileType<TauCannonHoldout>();
            Item.shootSpeed = 15f;
            Item.knockBack = 4f;

            Item.width = 146;
            Item.height = 52;
            Item.noMelee = true;
            Item.channel = true;
            Item.noUseGraphic = true;
            Item.value = CalamityGlobalItem.RarityPureGreenBuyPrice;
            Item.rare = ModContent.RarityType<PureGreen>();
            Item.useStyle = ItemUseStyleID.Shoot;

            modItem.UsesCharge = true;
            modItem.MaxCharge = 200f;
        }

        public override bool AltFunctionUse(Player player) => false;

        public override bool CanUseItem(Player player) => player.ownedProjectileCounts[Item.shoot] == 0 && Item.Calamity().Charge > 0;

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<ArcNovaDiffuser>().
                AddIngredient<MysteriousCircuitry>(10).
                AddIngredient<DubiousPlating>(15).
                AddIngredient<AstralBar>(10).
                AddIngredient<RuinousSoul>(2).
                AddTile(TileID.LunarCraftingStation).
                Register();
        }
    }
}
