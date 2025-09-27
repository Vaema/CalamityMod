using CalamityMod.Items.Materials;
using CalamityMod.Projectiles.Ranged;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Ranged
{
    public class BulletFilledShotgun : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Ranged";
        public override void SetStaticDefaults()
        {
        }
        public override void SetDefaults()
        {
            Item.width = 64;
            Item.height = 24;
            Item.damage = 1;
            Item.DamageType = DamageClass.Ranged;
            Item.useAnimation = Item.useTime = 75;
            Item.knockBack = 0.25f;
            Item.shoot = ModContent.ProjectileType<BouncingShotgunPellet>();
            Item.shootSpeed = 18f;

            Item.useAmmo = AmmoID.Bullet;
            Item.UseSound = SoundID.Item38;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.autoReuse = true;
            Item.noMelee = true;

            Item.value = CalamityGlobalItem.RarityOrangeBuyPrice;
            Item.rare = ItemRarityID.Orange;
            Item.Calamity().donorItem = true;
        }

        public override Vector2? HoldoutOffset() => new Vector2(-7, 0);

        public override bool CanUseItem(Player player) => CalamityGlobalItem.HasEnoughAmmo(player, Item, 5);

        // Disable vanilla ammo consumption
        public override bool CanConsumeAmmo(Item ammo, Player player) => false;

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            type = Item.shoot;
            position += player.itemRotation.ToRotationVector2() * player.direction * 48f;
            int bulletAmt = 40;
            for (int i = 0; i < bulletAmt; i++)
            {
                float newSpeedX = velocity.X + Main.rand.NextFloat(-15f, 15f);
                float newSpeedY = velocity.Y + Main.rand.NextFloat(-15f, 15f);
                Projectile.NewProjectile(source, position.X, position.Y, newSpeedX, newSpeedY, type, damage, knockback, player.whoAmI);
            }

            // Consume 5 ammo per shot
            CalamityGlobalItem.ConsumeAdditionalAmmo(player, Item, 5);
            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ItemID.MusketBall, 100).
                AddRecipeGroup("IronBar", 7).
                AddIngredient<AerialiteBar>(3).
                AddTile(TileID.Anvils).
                Register();
        }
    }
}
