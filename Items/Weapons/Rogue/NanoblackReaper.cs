using CalamityMod.Items.Materials;
using CalamityMod.Projectiles.Rogue;
using CalamityMod.Rarities;
using CalamityMod.Tiles.Furniture.CraftingStations;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Rogue
{
    // Deny me no longer!
    [LegacyName("NanoblackReaperMelee", "NanoblackReaperRogue")]
    public class NanoblackReaper : RogueWeapon, IHoldShiftTooltipItem
    {
        public bool ShowExtensionIndicator => false;
        public bool HasFlavorTooltip => true;
        public Color? TooltipExtensionColor => new Color(31, 223, 128); // #1FDF80
        public Color? FlavorTooltipColor => TooltipExtensionColor;

        public static float Knockback = 9f;
        public static float Speed = 16f;

        public override void SetDefaults()
        {
            Item.width = 78;
            Item.height = 64;
            Item.damage = 130;
            Item.knockBack = Knockback;
            Item.useTime = 6;
            Item.useAnimation = 6;
            Item.autoReuse = true;
            Item.noMelee = true;
            Item.noUseGraphic = true;

            Item.useStyle = ItemUseStyleID.Swing;
            Item.UseSound = SoundID.Item18;

            Item.value = CalamityGlobalItem.RarityHotPinkBuyPrice;
            Item.rare = ModContent.RarityType<HotPink>();
            Item.Calamity().devItem = true;

            Item.DamageType = RogueDamageClass.Instance;
            Item.shoot = ModContent.ProjectileType<NanoblackMain>();
            Item.shootSpeed = Speed;
        }

        public override bool Shoot(Player player, EntitySource_ItemUse_WithAmmo source, Vector2 position, Vector2 velocity, int type, int damage, float knockback)
        {
            if (player.Calamity().StealthStrikeAvailable())
            {
                int stealth = Projectile.NewProjectile(source, position, velocity, type, damage, knockback, player.whoAmI);
                if (stealth.WithinBounds(Main.maxProjectiles))
                    Main.projectile[stealth].Calamity().stealthStrike = player.Calamity().StealthStrikeAvailable();
                return false;
            }
            return true;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<GhoulishGouger>().
                AddIngredient<MoltenAmputator>().
                AddIngredient<ShadowspecBar>(5).
                AddIngredient<EndothermicEnergy>(40).
                AddIngredient<PlagueCellCanister>(20).
                AddIngredient(ItemID.Nanites, 400).
                AddTile<DraedonsForge>().
                Register();
        }
    }
}
