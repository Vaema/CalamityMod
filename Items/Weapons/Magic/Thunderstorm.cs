using CalamityMod.Projectiles.Magic;
using CalamityMod.Rarities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Magic
{
    public class Thunderstorm : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Magic";
        public override void SetDefaults()
        {
            Item.width = 220;
            Item.height = 60;
            Item.damage = 132;
            Item.mana = 50;
            Item.DamageType = DamageClass.Magic;
            Item.useTime = Item.useAnimation = 80; // 42 frames of firing animation
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.channel = true;
            Item.knockBack = 2f;
            Item.value = CalamityGlobalItem.RarityTurquoiseBuyPrice;
            Item.rare = ModContent.RarityType<Turquoise>();
            Item.autoReuse = true;
            Item.shootSpeed = 6f;
            Item.shoot = ModContent.ProjectileType<ThunderstormHoldout>();
        }

        // Cancels out the mana used to summon the holdout
        public override void OnConsumeMana(Player player, int manaConsumed)
        {
            if (player.ownedProjectileCounts[Item.shoot] <= 0)
                player.statMana += manaConsumed;
        }

        public override bool CanUseItem(Player player) => player.ownedProjectileCounts[Item.shoot] <= 0;

        public override void HoldItem(Player player) => player.Calamity().mouseRotationListener = true;

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Vector2 spawnPosition = player.RotatedRelativePoint(player.MountedCenter, true);
            Projectile.NewProjectileDirect(source, spawnPosition, player.Calamity().mouseWorld - spawnPosition, type, damage, knockback, player.whoAmI);
            return false;
        }
    }
}
