using CalamityMod.Items.Materials;
using CalamityMod.Projectiles.Ranged;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Ranged
{
    [LegacyName("SandstormGun")]
    public class Sandblaster : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Ranged";

        public static int AmmoSavedPercent = 50;
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(AmmoSavedPercent);

        public override void SetDefaults()
        {
            Item.width = 62;
            Item.height = 26;
            Item.damage = 70;
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = Item.useAnimation = 18;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 5f;
            Item.value = CalamityGlobalItem.RarityLimeBuyPrice;
            Item.rare = ItemRarityID.Lime;
            Item.UseSound = SoundID.Item11;
            Item.autoReuse = true;
            Item.shootSpeed = 12f;
            Item.shoot = ModContent.ProjectileType<SandblasterBullet>();
            Item.useAmmo = AmmoID.Sand;
        }

        public override Vector2? HoldoutOffset() => new Vector2(-10, 0);

        public override bool CanConsumeAmmo(Item ammo, Player player) => Main.rand.Next(100) >= AmmoSavedPercent;

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) => type = Item.shoot;

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ItemID.Sandgun).
                AddIngredient(ItemID.AncientCloth, 2).
                AddIngredient<GrandScale>().
                AddTile(TileID.MythrilAnvil).
                Register();
        }
    }
}
