using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Items.Materials;
using CalamityMod.Projectiles.Ranged;
using CalamityMod.Rarities;
using CalamityMod.Systems.Collections;
using CalamityMod.Tiles.Furniture.CraftingStations;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Ranged
{
    [LegacyName("Megafleet")]
    public class Voidragon : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Ranged";
        private int shotType = 1;

        public static int AmmoSavedPercent = 50;
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(AmmoSavedPercent);
        public override void SetStaticDefaults()
        {
            CalamityItemSets.ExtraDebuffTooltip_Enemy[Type] = [ModContent.BuffType<Voidfrost>()];
        }

        public override void SetDefaults()
        {
            Item.width = 96;
            Item.height = 38;
            Item.damage = 240;
            Item.DamageType = DamageClass.Ranged;
            Item.useTime = 5;
            Item.useAnimation = 5;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.noMelee = true;
            Item.knockBack = 5f;
            Item.UseSound = SoundID.Item11;
            Item.autoReuse = true;
            Item.shoot = ProjectileID.PurificationPowder;
            Item.shootSpeed = 18f;
            Item.useAmmo = AmmoID.Bullet;

            Item.value = CalamityGlobalItem.RarityHotPinkBuyPrice;
            Item.rare = ModContent.RarityType<HotPink>();
            Item.Calamity().devItem = true;
        }

        public override Vector2? HoldoutOffset() => new Vector2(-10, 0);

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            float SpeedX = velocity.X + (float)Main.rand.Next(-5, 6) * 0.05f;
            float SpeedY = velocity.Y + (float)Main.rand.Next(-5, 6) * 0.05f;

            if (shotType > 2)
                shotType = 1;

            if (shotType == 1)
                Projectile.NewProjectile(source, position.X, position.Y, SpeedX, SpeedY, type, damage, knockback, player.whoAmI);
            else
                Projectile.NewProjectile(source, position.X, position.Y, SpeedX, SpeedY, ModContent.ProjectileType<Projectiles.Ranged.Voidragon>(), damage, knockback, player.whoAmI);

            shotType++;

            Projectile.NewProjectile(source, position.X, position.Y, SpeedX, SpeedY, ModContent.ProjectileType<VoidragonTentacle>(), damage, knockback, player.whoAmI, (Main.rand.Next(-160, 160) * 0.001f), (Main.rand.Next(-160, 160) * 0.001f));

            return false;
        }

        public override bool CanConsumeAmmo(Item ammo, Player player) => Main.rand.Next(100) >= AmmoSavedPercent && shotType % 2 == 1;

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<Seadragon>().
                AddIngredient<ShadowspecBar>(5).
                AddTile<DraedonsForge>().
                Register();
        }
    }
}
