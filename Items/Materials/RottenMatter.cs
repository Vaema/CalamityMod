using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Materials
{
    [LegacyName("TrueShadowScale")]
    public class RottenMatter : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Materials";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 25;
            ItemID.Sets.SortingPriorityMaterials[Type] = 65; // Crimtane Ore
            ItemTrader.ChlorophyteExtractinator.AddOption_OneWay(Type, 1, ModContent.ItemType<BloodSample>(), 1);
        }

        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 32;
            Item.maxStack = 9999;
            Item.value = Item.sellPrice(silver: 2);
            Item.rare = ItemRarityID.Orange;

            Item.MakeUsableWithChlorophyteExtractinator();
        }
    }
}
