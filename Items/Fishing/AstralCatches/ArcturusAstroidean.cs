using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Fishing.AstralCatches
{
    public class ArcturusAstroidean : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Fishing";

        public static float FishingPowerBiomeMult = 1.1f;
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(FishingPowerBiomeMult.ToString());

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 5;
            ItemID.Sets.CanBePlacedOnWeaponRacks[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.maxStack = Item.CommonMaxStack;
            Item.value = Item.sellPrice(silver: 10);
            Item.rare = ItemRarityID.Orange;
            Item.bait = 40;
        }
    }
}
