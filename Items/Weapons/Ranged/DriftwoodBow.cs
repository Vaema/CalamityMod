using CalamityMod.Items.Placeables.FurnitureDriftwood;
using Microsoft.Xna.Framework;
using Terraria;
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

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            if (IsPlayerInContactWithWater(player))
            {
                velocity *= 1.3f;
                knockback += 1f;

                for (int i = 0; i <= 18; i++)
                {
                    Dust dust = Dust.NewDustPerfect(position + velocity * 3f, 160, velocity.RotatedByRandom(MathHelper.ToRadians(19f)) * Main.rand.NextFloat(0.8f, 3.8f), Scale: Main.rand.NextFloat(1.2f, 1.6f));
                    dust.noGravity = true;
                }
            }
        }

        public override float UseSpeedMultiplier(Player player) => IsPlayerInContactWithWater(player) ? 1.2f : 1f;

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<Driftwood>(10).
                AddTile(TileID.WorkBenches).
                Register();
        }

        private static bool IsPlayerInContactWithWater(Player player)
        {
            bool surface = player.Center.Y < Main.worldSurface * 16.0;
            return (Main.raining && surface) || player.dripping || (player.wet && !player.lavaWet && !player.honeyWet);
        }
    }
}
