using CalamityMod.Projectiles.Typeless;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items
{
    public class RelicOfConvergence : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Misc";

        public static int HealValue = 60;
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(HealValue);

        public override void SetStaticDefaults()
        {
            ItemID.Sets.CanBePlacedOnWeaponRacks[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 46;
            Item.useAnimation = Item.useTime = 25;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.UseSound = SoundID.DD2_DarkMageCastHeal;
            Item.noMelee = true;
            Item.noUseGraphic = true;
            Item.channel = true;
            Item.value = CalamityGlobalItem.RarityPurpleBuyPrice;
            Item.rare = ItemRarityID.Purple;
            Item.shoot = ModContent.ProjectileType<RelicOfConvergenceCrystal>();
        }

        public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup)
        {
            itemGroup = (ContentSamples.CreativeHelper.ItemGroup)CalamityResearchSorting.ToolsOther;
        }

        public override bool CanUseItem(Player player) => player.ownedProjectileCounts[Item.shoot] <= 0 && player.ownedProjectileCounts[ModContent.ProjectileType<RelicOfDeliveranceSpear>()] <= 0;
    }
}
