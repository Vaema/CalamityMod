using CalamityMod.Items.Placeables;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Ranged
{
    public class DriftwoodBow : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Ranged";
        public override void SetDefaults()
        {
            Item.damage = 10;
            Item.DamageType = DamageClass.Ranged;
            Item.width = 22;
            Item.height = 42;
            Item.useTime = 28;
            Item.useAnimation = 28;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 0f;
            Item.value = CalamityGlobalItem.RarityWhiteBuyPrice;
            Item.rare = ItemRarityID.White;
            Item.UseSound = SoundID.Item5;
            Item.autoReuse = true;
            Item.shoot = ProjectileID.WoodenArrowFriendly;
            Item.shootSpeed = 6.6f;
            Item.useAmmo = AmmoID.Arrow;
            Item.Calamity().canFirePointBlankShots = true;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            bool surface = player.Center.Y < Main.worldSurface * 16.0;
            bool GetEffects = ((Main.raining && surface) || player.dripping || (player.wet && !player.lavaWet && !player.honeyWet));
            if (GetEffects)
            {
                for (int i = 0; i <= 18; i++)
                {
                    Dust dust = Dust.NewDustPerfect(position + velocity * 3, 160, velocity.RotatedByRandom(MathHelper.ToRadians(19f)) * Main.rand.NextFloat(0.8f, 3.8f), 0, default, Main.rand.NextFloat(1.2f, 1.6f));
                    dust.noGravity = true;
                }
                Item.useTime = 23;
                Item.useAnimation = 23;
                Item.knockBack = 1f;
                Item.shootSpeed = 8.6f;
            }
            else
            {
                Item.useTime = 28;
                Item.useAnimation = 28;
                Item.knockBack = 0f;
                Item.shootSpeed = 6.6f;
            }
            Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
            return false;
        }
        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<Driftwood>(10).
                AddTile(TileID.WorkBenches).
                Register();
        }
    }
}
