using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.Ores
{
    public class AstralOre : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 100;
            ItemID.Sets.SortingPriorityMaterials[Type] = 99; // Luminite
            ItemTrader.ChlorophyteExtractinator.AddOption_OneWay(Type, 1, ItemID.Meteorite, 1);
        }

        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.Ores.AstralOre>());
            Item.value = Item.sellPrice(silver: 36);
            Item.rare = ItemRarityID.Cyan;
        }
    }
}
