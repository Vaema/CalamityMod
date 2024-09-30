using System;
using CalamityMod.Items.Materials;
using CalamityMod.Projectiles.Magic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Magic
{
    public class CosmicRainbow : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Magic";
        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 64;
            Item.damage = 162;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 10;
            Item.useAnimation = Item.useTime = 10;
            Item.knockBack = 0.25f;
            Item.shoot = ModContent.ProjectileType<CosmicRainbowFront>();
            Item.shootSpeed = 18f;

            Item.UseSound = SoundID.Item67 with { Volume = 0.75f };
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.autoReuse = true;
            Item.noMelee = true;
            
            Item.value = CalamityGlobalItem.RarityRedBuyPrice;
            Item.rare = ItemRarityID.Red;
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback)
        {
            position = player.Center + (Vector2.Normalize(velocity) * Main.rand.NextFloat(-36f, 36f)).RotatedBy(MathHelper.PiOver2);
            velocity = Vector2.Normalize(Main.MouseWorld - position) * Item.shootSpeed;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ItemID.RainbowGun).
                AddIngredient(ItemID.LunarBar, 5).
                AddIngredient(ItemID.ShimmerBlock, 5).
                AddTile(TileID.LunarCraftingStation).
                Register();
        }
    }
}
