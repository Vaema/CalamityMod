using CalamityMod.Items.Materials;
using CalamityMod.Projectiles.Ranged;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Ranged
{
    public class Shredder : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Ranged";
        public override void SetStaticDefaults()
        {
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[Item.type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 56;
            Item.height = 24;
            Item.damage = 31;
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 4;
            Item.useAnimation = 24;
            Item.reuseDelay = 20;
            Item.useLimitPerAnimation = 8;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 1.5f;
            Item.value = CalamityGlobalItem.Rarity11BuyPrice;
            Item.rare = ItemRarityID.Purple;
            Item.UseSound = SoundID.Item31;
            Item.autoReuse = true;
            Item.shoot = ProjectileID.Bullet;
            Item.shootSpeed = 9f;
            Item.useAmmo = AmmoID.Bullet;
            Item.Calamity().canFirePointBlankShots = true;
        }

        public override Vector2? HoldoutOffset() => new Vector2(-5, 0);

        public override bool AltFunctionUse(Player player) => true;

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            int bulletAmt = 4;
            if (player.altFunctionUse == 2)
            {
                velocity *= 2f;
                Vector2 newPosition = position + velocity.SafeNormalize(Vector2.UnitY) * 50f;
                Vector2 cachedVelocity = velocity;
                for (int index = 0; index < bulletAmt; index++)
                {
                    velocity += new Vector2(Main.rand.Next(-30, 31) * 0.05f, Main.rand.Next(-30, 31) * 0.05f);
                    int shot = Projectile.NewProjectile(source, newPosition, velocity, type, damage, knockback, player.whoAmI);
                    Main.projectile[shot].timeLeft = 180;
                    velocity = cachedVelocity;
                }
            }
            else
            {
                Vector2 newPosition = position + velocity.SafeNormalize(Vector2.UnitY) * 50f;
                Vector2 cachedVelocity = velocity;
                for (int index = 0; index < bulletAmt; index++)
                {
                    velocity += new Vector2(Main.rand.Next(-30, 31) * 0.05f, Main.rand.Next(-30, 31) * 0.05f);
                    Projectile.NewProjectile(source, newPosition, velocity, ModContent.ProjectileType<ChargedBlast>(), damage, knockback, player.whoAmI);
                    velocity = cachedVelocity;
                }
            }
            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<FrostbiteBlaster>().
                AddIngredient<BulletFilledShotgun>().
                AddIngredient(ItemID.LunarBar, 5).
                AddTile(TileID.LunarCraftingStation).
                Register();
        }
    }
}
