using System;
using CalamityMod.Items.Materials;
using CalamityMod.Projectiles.Ranged;
using CalamityMod.Rarities;
using CalamityMod.Tiles.Furniture.CraftingStations;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Ranged;

public class UniversalGenesis : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Weapons.Ranged";

    public override void SetDefaults()
    {
        Item.width = 158;
        Item.height = 60;
        Item.damage = 180;
        Item.DamageType = DamageClass.Ranged;
        Item.useAnimation = Item.useTime = 26;
        Item.knockBack = 6.5f;
        Item.useAmmo = AmmoID.Bullet;
        Item.shoot = ProjectileID.Bullet;
        Item.shootSpeed = 20f;
        Item.autoReuse = true;

        Item.noMelee = true;
        Item.UseSound = SoundID.Item38;
        Item.useStyle = ItemUseStyleID.Shoot;
        Item.value = CalamityGlobalItem.RarityDarkBlueBuyPrice;
        Item.rare = ModContent.RarityType<CosmicPurple>();
        Item.Calamity().donorItem = true;
    }

    public override Vector2? HoldoutOffset() => new Vector2(-50f, -8f);

    public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
    {
        Vector2 shootVelocity = velocity;
        Vector2 shootDirection = shootVelocity.SafeNormalize(Vector2.UnitX * player.direction);
        Vector2 gunTip = position + shootDirection * Item.scale * 100f;
        gunTip.Y -= 10f;
        float tightness = 1f;
        if (type == ProjectileID.Bullet)
            type = ModContent.ProjectileType<UniversalGenesisStarcaller>();
        for (float i = -tightness * 5f; i <= tightness * 5f; i += tightness * 2f)
        {
            Vector2 perturbedSpeed = shootVelocity.RotatedBy(MathHelper.ToRadians(i));
            Projectile.NewProjectile(source, gunTip, perturbedSpeed, type, damage, knockback, player.whoAmI);
        }

        // Stars from above
        float speed = Item.shootSpeed;
        int starAmt = 5;
        int starDmg = (int)(damage * 0.5f);
        for (int i = 0; i < starAmt; i++)
        {
            Vector2 spawnPos = new Vector2(player.Center.X + (Main.rand.Next(201) * -player.direction) + (Main.mouseX + Main.screenPosition.X - player.position.X), player.MountedCenter.Y - 600f);
            spawnPos.X = (spawnPos.X + player.Center.X) / 2f + Main.rand.Next(-200, 201);
            spawnPos.Y -= 100 + i;

            Vector2 vel = new Vector2(Main.mouseX + Main.screenPosition.X - spawnPos.X, MathF.Abs(Main.mouseY + Main.screenPosition.Y - spawnPos.Y));
            if (vel.Y < 20f)
                vel.Y = 20f;
            vel = vel.SafeNormalize(Vector2.UnitY) * speed;
            vel.X += Main.rand.NextFloat(-0.6f, 0.6f);
            vel.Y += Main.rand.NextFloat(-0.6f, 0.6f);

            int star = Projectile.NewProjectile(source, spawnPos, vel, ModContent.ProjectileType<UniversalGenesisStar>(), starDmg, knockback, player.whoAmI, i, 1f);
            Main.projectile[star].extraUpdates = 2;
        }
        return false;
    }

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient<ConferenceCall>().
            AddIngredient(ItemID.StarCloak, 3).
            AddIngredient<CosmiliteBar>(5).
            AddIngredient<AscendantSpiritEssence>(4).
            AddTile<CosmicAnvil>().
            Register();
    }
}
