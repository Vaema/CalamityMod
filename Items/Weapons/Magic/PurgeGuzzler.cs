using CalamityMod.Items.Weapons.Summon;
using CalamityMod.Projectiles.Magic;
using CalamityMod.Rarities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Magic
{
    public class PurgeGuzzler : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Magic";
        private const float Spread = 0.025f;
        public override void SetDefaults()
        {
            Item.width = 64;
            Item.height = 48;
            Item.damage = 152;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 50;
            Item.useTime = 38;
            Item.useAnimation = 38;
            Item.autoReuse = true;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.channel = true;
            Item.knockBack = 4.5f;
            Item.shoot = ModContent.ProjectileType<PurgeGuzzlerHoldout>();
            Item.shootSpeed = 6f;

            Item.value = CalamityGlobalItem.RarityTurquoiseBuyPrice;
            Item.rare = ModContent.RarityType<Turquoise>();
        }
        public override bool CanUseItem(Player player) => player.ownedProjectileCounts[Item.shoot] <= 0;

        public override void HoldItem(Player player) => player.Calamity().mouseRotationListener = true;

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Vector2 spawnPosition = player.RotatedRelativePoint(player.MountedCenter, true);
            // 14NOV2024: Ozzatron: clamped mouse position unnecessary, only used for direction
            Projectile.NewProjectileDirect(source, spawnPosition, player.Calamity().mouseWorld - spawnPosition, type, damage, knockback, player.whoAmI);
            return false;
        }
    }
}
