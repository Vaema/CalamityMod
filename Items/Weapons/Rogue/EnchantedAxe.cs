using CalamityMod.Items.Materials;
using CalamityMod.Projectiles.Rogue;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Rogue
{
    public class EnchantedAxe : RogueWeapon
    {
        public override void SetDefaults()
        {
            Item.width = 40;
            Item.height = 36;
            Item.damage = 19;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.useAnimation = Item.useTime = 19;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.knockBack = 1f;
            Item.UseSound = SoundID.Item1;
            Item.autoReuse = true;
            Item.maxStack = 1;
            Item.rare = ItemRarityID.Orange;
            Item.value = CalamityGlobalItem.RarityOrangeBuyPrice;
            Item.shoot = ModContent.ProjectileType<EnchantedAxeProj>();
            Item.shootSpeed = 30f;
            Item.DamageType = RogueDamageClass.Instance;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.Calamity().StealthStrikeAvailable())
            {
                int p = Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, 0f, 1f);
                if (p.WithinBounds(Main.maxProjectiles))
                    Main.projectile[p].Calamity().stealthStrike = true;
                return false;
            }
            return true;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<IronFrancisca>().
                AddIngredient(ItemID.FallenStar, 5).
                AddIngredient<PearlShard>(10).
                AddIngredient(ItemID.Bone, 30).
                AddTile(TileID.Anvils).
                Register();

            CreateRecipe().
                AddIngredient<LeadTomahawk>().
                AddIngredient(ItemID.FallenStar, 5).
                AddIngredient<PearlShard>(10).
                AddIngredient(ItemID.Bone, 30).
                AddTile(TileID.Anvils).
                Register();
        }
    }
}
