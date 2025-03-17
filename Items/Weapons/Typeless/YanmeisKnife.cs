using CalamityMod.Items.Materials;
using CalamityMod.Projectiles.Typeless;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Weapons.Typeless
{
    public class YanmeisKnife : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Weapons.Typeless";
        public static readonly SoundStyle HitSound = new("CalamityMod/Sounds/Item/YanmeiKnifeHit");
        public static readonly SoundStyle ExpireSound = new("CalamityMod/Sounds/Custom/YanmeiKnifeExpire");

        public static int DebuffDuration = 600;
        public static int DebuffDoT = 250;
        public static float DebuffDamageReductionMult = 0.8f;
        public static float DebuffNPCSpeedCap = 16f;

        public static int BoostDuration = 600;
        public static float RunSpeedBoost = 0.15f;
        public static float RunAccelerationBoost = 0.15f;
        public static float DamageBoost = 0.15f; // All 15% so we only need just one in the tooltip -- Same for the respective buff
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(RunSpeedBoost.ToPercent(), BoostDuration.FramesToSeconds());

        public override void SetDefaults()
        {
            Item.width = 48;
            Item.height = 44;
            Item.damage = 8;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.useAnimation = Item.useTime = 32;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.knockBack = 4.5f;
            Item.autoReuse = false;
            Item.value = CalamityGlobalItem.RarityYellowBuyPrice;
            Item.rare = ItemRarityID.Yellow;
            Item.Calamity().donorItem = true;
            Item.UseSound = SoundID.Item71;
            Item.shoot = ModContent.ProjectileType<YanmeisKnifeSlash>();
            Item.shootSpeed = 24f;
        }

        public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup)
        {
            itemGroup = (ContentSamples.CreativeHelper.ItemGroup)CalamityResearchSorting.ClasslessWeapon;
        }

        // Terraria seems to really dislike high crit values in SetDefaults
        public override void ModifyWeaponCrit(Player player, ref float crit) => crit += 6;

        public override bool CanUseItem(Player player)
        {
            if (player.Calamity().KameiBladeUseDelay > 0)
                return false;
            return base.CanUseItem(player);
        }

        public override void ModifyShootStats(Player player, ref Vector2 position, ref Vector2 velocity, ref int type, ref int damage, ref float knockback) => player.Calamity().KameiBladeUseDelay = 180;

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ItemID.PsychoKnife).
                AddIngredient(ItemID.Obsidian, 10).
                AddRecipeGroup("IronBar", 20).
                AddIngredient<PlagueCellCanister>(50).
                AddTile(TileID.MythrilAnvil).
                Register();
        }
    }
}
