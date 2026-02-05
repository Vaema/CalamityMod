using CalamityMod.Projectiles.Ranged;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Ranged
{
    public class BlossomFlux : LegendaryItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Ranged";

        public override Color? TooltipExtensionColor => new Color(109, 161, 84);
        public override void SetStaticDefaults()
        {
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 38;
            Item.height = 68;
            Item.damage = 18;
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 4;
            Item.useAnimation = 16;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 0.15f;
            Item.UseSound = SoundID.Item5;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<LeafArrow>();
            Item.shootSpeed = 10f;
            Item.useAmmo = AmmoID.Arrow;
            Item.consumeAmmoOnFirstShotOnly = true;

            Item.value = CalamityGlobalItem.RarityLimeBuyPrice;
            Item.rare = ItemRarityID.Lime;
        }

        public override bool AltFunctionUse(Player player) => true;

        public override bool CanUseItem(Player player)
        {
            if (player.altFunctionUse == 2)
            {
                Item.useTime = 25;
                Item.useAnimation = 25;
                Item.useLimitPerAnimation = 1;
                Item.UseSound = SoundID.Item77;
            }
            else
            {
                Item.useTime = 3;
                Item.useAnimation = 15;
                Item.useLimitPerAnimation = 5;
                Item.UseSound = SoundID.Item5;
            }
            return base.CanUseItem(player);
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2)
            {
                Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<SporeBomb>(), (int)(damage * 3.15f), knockback * 60f, player.whoAmI);
            }
            else
            {
                Projectile.NewProjectile(source, position, velocity, ModContent.ProjectileType<LeafArrow>(), damage, knockback, player.whoAmI);
            }
            return false;
        }
    }
}
