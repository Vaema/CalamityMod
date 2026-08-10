using CalamityMod.Projectiles.Magic;
using CalamityMod.Projectiles.Melee;
using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Magic;

public class CosmicRainbow : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Weapons.Magic";
    public override void SetDefaults()
    {
        Item.width = 26;
        Item.height = 64;
        Item.damage = 117;
        Item.DamageType = DamageClass.Magic;
        Item.mana = 10;
        Item.useAnimation = Item.useTime = 10;
        Item.knockBack = 0.25f;
        Item.shoot = ModContent.ProjectileType<CosmicRainbowFront>();
        Item.shootSpeed = 18f;

        Item.UseSound = SoundID.Item67 with { Volume = 0.7f };
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.autoReuse = true;
        Item.noMelee = true;
        
        Item.value = CalamityGlobalItem.RarityPurpleBuyPrice;
        Item.rare = ItemRarityID.Purple;
    }

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        Vector2 rainbowPos = player.Center + (Vector2.Normalize(velocity) * Main.rand.NextFloat(-36f, 36f)).RotatedBy(MathHelper.PiOver2);
        // 14NOV2024: Ozzatron: clamped mouse position unnecessary, only used for direction
        Vector2 rainbowVel = Vector2.Normalize(Main.MouseWorld - rainbowPos) * Item.shootSpeed;
        Projectile.NewProjectile(source, rainbowPos, rainbowVel, type, damage, knockback, Main.myPlayer);

        double rotationOffset = Math.Sin((Main.GameUpdateCount / 60f) * (MathHelper.Pi / 1.5f)) * 0.4f;
        Projectile star = Projectile.NewProjectileDirect(source, position, velocity.RotatedBy(rotationOffset), ModContent.ProjectileType<PrismaticWave>(), damage, knockback, Main.myPlayer, 0f, Main.rand.Next(12), 1f);
        star.DamageType = DamageClass.Magic;
        star.scale = 0.7f;
        return false;
    }

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient(ItemID.RainbowGun).
            AddIngredient(ItemID.LunarBar, 5).
            AddIngredient(ItemID.CrystalShard, 10).
            AddIngredient(ItemID.SoulofLight, 10).
            AddTile(TileID.LunarCraftingStation).
            Register();
    }
}
