using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables.SunkenSea;
using CalamityMod.Projectiles.Magic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Magic
{
    public class AquamarineStaff : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Magic";
        public override void SetStaticDefaults()
        {
            Item.staff[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 82;
            Item.height = 84;
            Item.damage = 17;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 3;
            Item.useTime = 22;
            Item.useAnimation = 22;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 2.5f;
            Item.value = CalamityGlobalItem.RarityGreenBuyPrice;
            Item.rare = ItemRarityID.Green;
            Item.UseSound = SoundID.DD2_SkyDragonsFuryShot;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<AquamarineBolt>();
            Item.shootSpeed = 14f;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            int parent = Projectile.NewProjectile(source, position + velocity*2, velocity, type, damage, knockback, player.whoAmI);
            int child = Projectile.NewProjectile(source, position + velocity*2, velocity, type, damage, knockback, player.whoAmI, 1, 0, parent);
            Main.projectile[child].penetrate = -1;
            Main.projectile[child].tileCollide = false;
            Main.projectile[child].scale = 0.75f;

            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ItemID.AmethystStaff).
                AddIngredient<PearlShard>(3).
                AddIngredient<SeaPrism>(5).
                AddIngredient<Navystone>(25).
                AddTile(TileID.Anvils).
                Register();
            CreateRecipe().
                AddIngredient(ItemID.TopazStaff).
                AddIngredient<PearlShard>(3).
                AddIngredient<SeaPrism>(5).
                AddIngredient<Navystone>(25).
                AddTile(TileID.Anvils).
                Register();
        }
    }
}
