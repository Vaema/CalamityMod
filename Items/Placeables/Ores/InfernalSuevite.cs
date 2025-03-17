using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.Ores
{
    [LegacyName("CharredOre")]
    public class InfernalSuevite : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 100;
            ItemID.Sets.SortingPriorityMaterials[Type] = 90; // Chlorophyte Ore
        }

        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.Ores.InfernalSuevite>());
            Item.value = Item.sellPrice(silver: 16);
            Item.rare = ItemRarityID.Pink;
        }
    }
}
