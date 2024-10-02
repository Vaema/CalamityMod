using CalamityMod.Items.Materials;
using CalamityMod.Projectiles.Magic;
using CalamityMod.Rarities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Magic
{
    public class DivineRetribution : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Magic";
        public override void SetStaticDefaults()
        {
            Item.staff[Item.type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 66;
            Item.height = 88;
            Item.damage = 48;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 15;
            Item.useTime = 12;
            Item.useAnimation = 12;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 3.5f;

            Item.value = CalamityGlobalItem.RarityTurquoiseBuyPrice;
            Item.rare = ModContent.RarityType<Turquoise>();
            Item.Calamity().donorItem = true;

            Item.UseSound = SoundID.Item73;
            Item.autoReuse = true;
            Item.shootSpeed = 25f;
            Item.shoot = ModContent.ProjectileType<DivineRetributionSpear>();
        }


        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // 5 projectiles total
            for (int i = -2; i < 3; i++)
            {
                Vector2 newPos = new Vector2(player.ClampedMouseWorld().X + Main.rand.NextFloat(8f, 64f) * i, player.MountedCenter.Y + Main.rand.NextFloat(640f, 800f));
                Vector2 newVel = (player.ClampedMouseWorld() + Main.rand.NextVector2CircularEdge(4f, 4f) - newPos).SafeNormalize(Vector2.Zero) * velocity.Length() * Main.rand.NextFloat(1f, 1.25f);
                float velScale = 1f + Main.rand.NextFloat(0.02f, 0.08f) * i;
                Projectile.NewProjectile(source, newPos, newVel, type, damage, knockback, player.whoAmI, velScale);
            }
            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<UndinesRetribution>().
                AddIngredient<DivineGeode>(8).
                AddIngredient<UnholyEssence>(10).
                AddTile(TileID.LunarCraftingStation).
                Register();
        }
    }
}
