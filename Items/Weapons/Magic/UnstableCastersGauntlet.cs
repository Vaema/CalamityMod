using CalamityMod.Projectiles.Magic;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityMod.Items.Materials;

namespace CalamityMod.Items.Weapons.Magic
{
    public class UnstableCastersGauntlet : ModItem, ILocalizedModType, IHoldShiftTooltipItem
    {
        public static int UseTime = 20;
        public new string LocalizationCategory => "Items.Weapons.Magic";

        public override void SetDefaults()
        {
            Item.width = 62;
            Item.height = 60;
            Item.damage = 240;
            Item.DamageType = DamageClass.Magic;
            Item.mana = 0;
            Item.useAnimation = Item.useTime = UseTime;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.channel = true;
            Item.knockBack = 3f;
            Item.value = CalamityGlobalItem.RarityRedBuyPrice;
            Item.rare = ItemRarityID.Red;
            Item.Calamity().donorItem = true;
            Item.autoReuse = true;
            Item.shoot = ModContent.ProjectileType<UnstableCastersGauntletHoldout>();
            Item.shootSpeed = 15f;
        }

        public override bool CanUseItem(Player player) => player.ownedProjectileCounts[Item.shoot] <= 0;

        public override bool CanRightClick() => true;
        public override bool AltFunctionUse(Player player) => true;

        public override void HoldItem(Player player)
        {
            player.Calamity().mouseRotationListener = true;
        }

        public override void EquipFrameEffects(Player player, EquipType type)
        {
            player.handon = -1;
            player.handoff = -1;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            // Projectile will point toward your cursor.
            Vector2 spawnPosition = player.RotatedRelativePoint(player.MountedCenter, true);

            Projectile.NewProjectile(source, spawnPosition, player.Calamity().mouseWorld - spawnPosition, ModContent.ProjectileType<UnstableCastersGauntletHoldout>(), damage, knockback, player.whoAmI);
            return false;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ItemID.LunarBar, 5).
                AddIngredient<LifeAlloy>(5).
                AddIngredient(ItemID.FragmentNebula, 5).
                AddTile(TileID.MythrilAnvil).
                Register();
        }
    }
}
