using CalamityMod.Projectiles.Rogue;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Rogue
{
    public class NastyCholla : RogueWeapon
    {
        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 18;
            Item.damage = 10;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.useAnimation = 20;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTime = 20;
            Item.knockBack = 3f;
            Item.UseSound = SoundID.Item1;
            Item.value = CalamityGlobalItem.RarityWhiteBuyPrice;
            Item.rare = ItemRarityID.White;
            Item.shoot = ModContent.ProjectileType<NastyChollaBol>();
            Item.shootSpeed = 8f;
            Item.DamageType = RogueDamageClass.Instance;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.Calamity().StealthStrikeAvailable()) //setting the stealth strike
            {
                int spread = 3;
                for (int i = 0; i < 4; i++)
                {
                    Vector2 perturbedspeed = new Vector2(velocity.X + Main.rand.Next(-3, 4), velocity.Y + Main.rand.Next(-3, 4)).RotatedBy(MathHelper.ToRadians(spread));
                    int proj = Projectile.NewProjectile(source, position.X, position.Y, perturbedspeed.X, perturbedspeed.Y, type, damage, knockback, player.whoAmI);
                    if (proj.WithinBounds(Main.maxProjectiles))
                    {
                        Main.projectile[proj].originalDamage = damage;
                        Main.projectile[proj].Calamity().stealthStrike = true;
                    }
                    spread -= Main.rand.Next(1, 4);
                }
                return false;
            }
            return true;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ItemID.Cactus, 8).
                AddTile(TileID.WorkBenches).
                Register();
        }
    }
}
