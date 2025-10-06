using CalamityMod.Projectiles.Ranged;
using CalamityMod.Projectiles.Turret;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Ranged
{
    public class P90 : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Ranged";

        public static int AmmoSavedPercent = 75;
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(AmmoSavedPercent);

        public bool fireShot = true;
        public override void SetDefaults()
        {
            Item.width = 60;
            Item.height = 28;
            Item.damage = 6;
            Item.DamageType = DamageClass.Ranged;
            Item.useAnimation = Item.useTime = 2;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 1.5f;
            Item.value = Item.buyPrice(gold: 35); // Sold by Arms Dealer
            Item.rare = ItemRarityID.LightRed;
            Item.UseSound = SoundID.Item11 with { Volume = 0.6f };
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<P90Round>();
            Item.shootSpeed = 9f;
            Item.useAmmo = AmmoID.Bullet;
        }

        public override Vector2? HoldoutOffset() => new Vector2(-14, -1);

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (fireShot)
                Projectile.NewProjectile(source, position, velocity.RotatedByRandom(0.03f), ModContent.ProjectileType<P90Round>(), damage, knockback, player.whoAmI);
            fireShot = !fireShot;
            return false;
        }

        public override bool CanConsumeAmmo(Item ammo, Player player) => fireShot && Main.rand.Next(100) >= AmmoSavedPercent;
    }
}
