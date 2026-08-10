using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.Ores;

public class CryonicOre : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 100;
        ItemID.Sets.SortingPriorityMaterials[Type] = 90; // Chlorophyte Ore
    }

    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.Ores.CryonicOre>());
        Item.value = Item.sellPrice(silver: 18);
        Item.rare = ItemRarityID.Pink;
    }
}
