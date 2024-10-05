using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.PlaceableTurrets
{
    public class HostileWaterTurret : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override string Texture => "CalamityMod/Items/Placeables/PlaceableTurrets/WaterTurret";
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.DraedonStructures.HostileWaterTurret>());

            Item.value = Item.sellPrice(gold: 1);
            Item.rare = ItemRarityID.Orange;
        }
    }
}
