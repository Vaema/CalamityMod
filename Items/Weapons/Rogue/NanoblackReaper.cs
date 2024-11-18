using CalamityMod.CalPlayer;
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

        public static int ArmorPenetration = 30;
        // Armor pen declared on projectiles will be added to that of the parent projectile or, failing that, item that spawned it. Scary.
        public static int ZeroPointArmorPenetration = 120; // Total: 150.
        public static float TesselationDamageRatio = 0.25f;

        public override float StealthDamageMultiplier => 1.0f;

        public override void SetDefaults()
        {
            Item.width = 78;
            Item.height = 64;
            Item.damage = 315;
            Item.knockBack = Knockback;
            Item.ArmorPenetration = ArmorPenetration;
            Item.useTime = Item.useAnimation = 19;
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
            CalamityPlayer modPlayer = player.Calamity();
            if (modPlayer.StealthStrikeAvailable())
            {
                int stealthDamage = (int)(StealthDamageMultiplier * damage);

                // Technically, NewProjectileDirect is optimal.
                // However, it is unsafe, because it immediately indexes the array without checks, and will blow up on projectile caps.
                int ssProjIdx = Projectile.NewProjectile(source, position, velocity, type, stealthDamage, knockback, player.whoAmI);
                if (ssProjIdx.WithinBounds(Main.maxProjectiles))
                    Main.projectile[ssProjIdx].Calamity().stealthStrike = modPlayer.StealthStrikeAvailable();
                return false;
            }

            // In the case of non-stealth strikes, just spawn the projectile normally.
            return true;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<MoltenAmputator>().
                AddIngredient<GhoulishGouger>().
                AddIngredient<ShadowspecBar>(5).
                AddIngredient<EndothermicEnergy>(40).
                AddIngredient<PlagueCellCanister>(20).
                AddIngredient(ItemID.Nanites, 400).
                AddTile<DraedonsForge>().
                Register();
        }
    }
}
