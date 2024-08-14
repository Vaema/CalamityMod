using CalamityMod.Items.Materials;
using CalamityMod.Projectiles.Magic;
using CalamityMod.Rarities;
using CalamityMod.Tiles.Furniture.CraftingStations;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Magic
{
    public class HeliumFlash : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Magic";
        internal const float ExplosionDamageMultiplier = 1.4f;
        public static readonly SoundStyle Charge = new("CalamityMod/Sounds/Item/HeliumFlashCharge");
        public static readonly SoundStyle ChargeLoop = new("CalamityMod/Sounds/Item/HeliumFlashFullChargeLoop");
        internal static readonly int ChargeLoopSoundFrames = 120;
        public static readonly SoundStyle ChargeFire = new("CalamityMod/Sounds/Item/HeliumFlashFire") { Volume = 1f };

        public static int AftershotCooldownFrames = 17;
        public static int FullChargeFrames = 88;

        public override void SetDefaults()
        {
            Item.width = 112;
            Item.height = 112;
            Item.DamageType = DamageClass.Magic;
            Item.damage = 5000;
            Item.knockBack = 9.5f;
            Item.mana = 80;
            Item.useAnimation = Item.useTime = AftershotCooldownFrames;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.channel = true;
            Item.noUseGraphic = true;
            Item.useStyle = ItemUseStyleID.Shoot;

            Item.value = CalamityGlobalItem.RarityVioletBuyPrice;
            Item.rare = ModContent.RarityType<Violet>();

            Item.shoot = ModContent.ProjectileType<HeliumFlashHoldout>();
            Item.shootSpeed = 15f;
        }

        public override bool CanUseItem(Player player) => player.ownedProjectileCounts[Item.shoot] <= 0;

        public override void HoldItem(Player player) => player.Calamity().mouseRotationListener = true;

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            Vector2 spawnPosition = player.RotatedRelativePoint(player.MountedCenter, true);
            Projectile.NewProjectileDirect(source, spawnPosition, player.Calamity().mouseWorld - spawnPosition, ModContent.ProjectileType<HeliumFlashHoldout>(), damage, knockback, player.whoAmI);
            return false;
        }
        public override void AddRecipes()
        {
            Recipe r = CreateRecipe();
            r.AddIngredient<VenusianTrident>();
            r.AddIngredient<ForbiddenSun>();
            r.AddIngredient(ItemID.FragmentSolar, 10);
            r.AddIngredient(ItemID.FragmentNebula, 5);
            r.AddIngredient<AuricBar>(5);
            r.AddTile<CosmicAnvil>();
            r.Register();
        }
    }
}
