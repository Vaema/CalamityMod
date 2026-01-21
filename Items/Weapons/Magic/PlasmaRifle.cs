using CalamityMod.Projectiles.Magic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Magic
{
    public class PlasmaRifle : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Magic";

        public static readonly SoundStyle HeavyShotSound = new("CalamityMod/Sounds/Item/PlasmaRifleMain");
        public static readonly SoundStyle FastShotSound = new("CalamityMod/Sounds/Item/PlasmaRifleAlt");

        public override void SetStaticDefaults()
        {
            ItemID.Sets.ItemsThatAllowRepeatedRightClick[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 72;
            Item.height = 20;
            Item.damage = 160;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 40;
            Item.useAnimation = Item.useTime = 40;
            Item.knockBack = 4f;
            Item.shoot = ModContent.ProjectileType<PlasmaRifleShot>();
            Item.shootSpeed = 14f;

            Item.UseSound = HeavyShotSound;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.autoReuse = true;
            Item.noMelee = true;

            Item.rare = ItemRarityID.Red;
            Item.value = CalamityGlobalItem.RarityRedBuyPrice;
        }

        public override Vector2? HoldoutOffset() => new Vector2(-10, 0);

        public override bool AltFunctionUse(Player player) => true;

        public override bool CanUseItem(Player player)
        {
            Item.UseSound = player.altFunctionUse == 2 ? FastShotSound : HeavyShotSound;
            return base.CanUseItem(player);
        }

        public override void ModifyManaCost(Player player, ref float reduce, ref float mult)
        {
            if (player.altFunctionUse == 2)
                mult *= 0.25f;
        }

        public override float UseSpeedMultiplier(Player player) => player.altFunctionUse == 2 ? 5f : 1f;

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) => position += velocity.SafeNormalize(Vector2.UnitX) * 56f;

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.altFunctionUse == 2)
            {
                Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI, 1f);
                return false;
            }
            return true;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ItemID.ToxicFlask).
                AddIngredient(ItemID.LaserRifle).
                AddIngredient(ItemID.FragmentVortex, 6).
                AddTile(TileID.LunarCraftingStation).
                Register();
        }
    }
}
