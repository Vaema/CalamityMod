using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.Ores;

public class AerialiteOreDisenchanted : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 100;
        ItemID.Sets.SortingPriorityMaterials[Type] = 69; // Hellstone
    }

    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.Ores.AerialiteOreDisenchanted>());
        Item.value = Item.sellPrice(silver: 6);
        Item.rare = ItemRarityID.Orange;
    }
}
