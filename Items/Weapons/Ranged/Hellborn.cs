using System.Linq;
using CalamityMod.Projectiles.Ranged;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Ranged
{
    public class Hellborn : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Ranged";

        public override void SetDefaults()
        {
            Item.width = 62;
            Item.height = 34;
            Item.damage = 475;
            Item.DamageType = DamageClass.Ranged;
            Item.useAnimation = Item.useTime = 35;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 2.5f;

            Item.value = CalamityGlobalItem.RarityPinkBuyPrice;
            Item.rare = ItemRarityID.Pink;

            Item.noMelee = true;
            Item.UseSound = null;
            Item.noUseGraphic = true;
            Item.channel = true;
            Item.autoReuse = true;

            Item.shoot = ModContent.ProjectileType<HellbornHoldout>();
            Item.shootSpeed = 12f;
        }
        public override bool CanUseItem(Player player) => player.ownedProjectileCounts[Item.shoot] <= 0;
        public override bool AltFunctionUse(Player player) => true;
        public override void HoldItem(Player player)
        {
            if (player.whoAmI != Main.myPlayer)
                return;

            // Right-click channeling
            player.Calamity().rightClickListener = true;

            if (player.Calamity().mouseRight && CanUseItem(player) && !Main.mapFullscreen && !Main.blockMouse)
            {
                // Only one out at a time
                if (Main.projectile.Any(n => n.active && n.type == Item.shoot && n.owner == player.whoAmI))
                    return;

                var source = player.GetSource_ItemUse(player.ActiveItem());
                Projectile holdout = Projectile.NewProjectileDirect(source, player.Center, Vector2.Zero, Item.shoot, player.ActiveItem().damage, 0f, player.whoAmI, 0, 0, 5);
                holdout.velocity = (player.Calamity().mouseWorld - player.MountedCenter).SafeNormalize(Vector2.Zero);
            }
        }
        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Only one out at a time
            if (Main.projectile.Any(n => n.active && n.type == Item.shoot && n.owner == player.whoAmI))
                return false;

            Projectile holdout = Projectile.NewProjectileDirect(source, player.MountedCenter, Vector2.Zero, type, damage, knockback, player.whoAmI, 0, 0, 0);

            // We set the rotation to the direction to the mouse so the first frame doesn't appear bugged out.
            holdout.velocity = (player.Calamity().mouseWorld - player.MountedCenter).SafeNormalize(Vector2.Zero);
            return false;
        }
    }
}
