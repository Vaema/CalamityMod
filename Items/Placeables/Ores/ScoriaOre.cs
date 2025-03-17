using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.Ores
{
    [LegacyName("ChaoticOre")]
    public class ScoriaOre : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 100;
            ItemID.Sets.SortingPriorityMaterials[Type] = 95; // Stardust Fragment
        }

        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.Ores.ScoriaOre>());
            Item.value = Item.sellPrice(silver: 25);
            Item.rare = ItemRarityID.Yellow;
        }
    }
}
