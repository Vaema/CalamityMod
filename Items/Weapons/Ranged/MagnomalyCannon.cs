using CalamityMod.Items.Materials;
using CalamityMod.Items.Weapons.Typeless;
using CalamityMod.Projectiles.Ranged;
using CalamityMod.Rarities;
using CalamityMod.Tiles.Furniture.CraftingStations;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Ranged
{
    public class MagnomalyCannon : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Ranged";
        public override void SetDefaults()
        {
            Item.width = 84;
            Item.height = 30;
            Item.damage = 279;
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 15;
            Item.useAnimation = 15;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 9.5f;
            Item.UseSound = SoundID.Item11;
            Item.value = CalamityGlobalItem.RarityVioletBuyPrice;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<MagnomalyRocket>();
            Item.shootSpeed = 15f;
            Item.useAmmo = AmmoID.Rocket;
            Item.rare = ModContent.RarityType<BurnishedAuric>();
        }

        public override Vector2? HoldoutOffset()
        {
            return new Vector2(-30, -10);
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) => type = Item.shoot;

        public override bool CanConsumeAmmo(Item ammo, Player player)
        {
            if (Main.rand.Next(0, 100) < 66)
                return false;
            return true;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<ThePack>().
                AddIngredient<ScorchedEarth>().
                AddIngredient(ItemID.ElectrosphereLauncher).
                AddIngredient<MiracleMatter>().
                AddTile<DraedonsForge>().
                Register();
        }
    }
}
