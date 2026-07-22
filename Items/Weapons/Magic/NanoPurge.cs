using CalamityMod.Projectiles.Magic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityMod.Items.Materials;
using CalamityMod.Rarities;

namespace CalamityMod.Items.Weapons.Magic
{
    [LegacyName("Purge")]
    public class NanoPurge : ModItem, ILocalizedModType
    {
        public static int UseTime = 20;

        public new string LocalizationCategory => "Items.Weapons.Magic";

        public override void SetDefaults()
        {
            Item.width = 62;
            Item.height = 34;
            Item.damage = 135;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 8;
            Item.useAnimation = Item.useTime = UseTime;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.channel = true;
            Item.knockBack = 3f;
            Item.value = CalamityGlobalItem.RarityTurquoiseBuyPrice;
            Item.rare = ModContent.RarityType<Turquoise>();
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<NanoPurgeHoldout>();
            Item.shootSpeed = 16f;
        }

        public override bool CanUseItem(Player player) => player.ownedProjectileCounts[Item.shoot] <= 0;

        public override void HoldItem(Player player) => player.Calamity().mouseRotationListener = true;

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Vector2 spawnPosition = player.RotatedRelativePoint(player.MountedCenter, true);
            // 14NOV2024: Ozzatron: clamped mouse position unnecessary, only used for direction
            Projectile.NewProjectile(source, spawnPosition, player.Calamity().mouseWorld - spawnPosition, ModContent.ProjectileType<NanoPurgeHoldout>(), damage, knockback, player.whoAmI);
            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ItemID.LaserMachinegun).
                AddIngredient<UelibloomBar>(10).
                AddIngredient(ItemID.Nanites, 100).
                AddTile(TileID.MythrilAnvil).
                Register();
        }
    }
}
