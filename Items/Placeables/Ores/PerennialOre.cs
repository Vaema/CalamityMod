using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.Ores;

public class PerennialOre : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 100;
        ItemID.Sets.SortingPriorityMaterials[Type] = 92; // Shroomite Bar
    }

    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.Ores.PerennialOre>());
        Item.value = Item.sellPrice(silver: 25);
        Item.rare = ItemRarityID.Lime;
    }
}
