using CalamityMod.Projectiles.Magic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Magic
{
    public class Waywasher : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Magic";
        public int shotCount = 0;
        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 30;
            Item.damage = 16;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 15;
            Item.useTime = 14;
            Item.useAnimation = 28;
            Item.reuseDelay = 23;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 6.5f;
            Item.value = CalamityGlobalItem.RarityGreenBuyPrice;
            Item.rare = ItemRarityID.Green;
            Item.UseSound = SoundID.Item8;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<WaywasherProj>();
            Item.shootSpeed = 13f;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectile(source, position, velocity.RotatedBy(-0.3f * (shotCount % 2 == 0 ? -1 : 1)), ModContent.ProjectileType<WaywasherProj>(), damage, knockback, player.whoAmI, 0, shotCount % 2 == 0 ? -1 : 1);
            shotCount++;
            return false;
        }

    }
}
