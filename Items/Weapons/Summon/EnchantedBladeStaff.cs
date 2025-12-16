using CalamityMod.Projectiles.Summon;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Summon
{
    public class EnchantedBladeStaff : ModItem, ILocalizedModType
    {
        public static float SwingTime = 20f;
        public static float SwingWait = 10f;
        public static float ProjectileSpeed = 15f;
        public static float DashCooldown = 30f;
        public static float DashSpeed = 16f;

        public new string LocalizationCategory => "Items.Weapons.Summon";

        public override void SetStaticDefaults() => Item.staff[Type] = true;

        public override void SetDefaults()
        {
            Item.damage = 10;
            Item.DamageType = DamageClass.Summon;
            Item.shoot = ModContent.ProjectileType<EnchantedBladeSummon>();
            Item.knockBack = 2f;

            Item.useTime = Item.useAnimation = 15;
            Item.mana = 10;
            Item.noMelee = true;
            Item.autoReuse = true;
            Item.width = 50;
            Item.height = 50;
            Item.value = CalamityGlobalItem.RarityBlueBuyPrice;
            Item.rare = ItemRarityID.Blue;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.UseSound = SoundID.Item8;

            // Does nothing mechanical. Just makes Item.staff[] work.
            Item.shootSpeed = 1f;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Projectile.NewProjectileDirect(source, Main.MouseWorld, Vector2.Zero, type, damage, knockback, player.whoAmI);
            return false;
        }
    }
}
