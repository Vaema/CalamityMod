using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.PlaceableTurrets
{
    public class HostileLaserTurret : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override string Texture => "CalamityMod/Items/Placeables/PlaceableTurrets/LaserTurret";
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.DraedonStructures.HostileLaserTurret>());

            Item.value = Item.sellPrice(gold: 2);
            Item.rare = ItemRarityID.Pink;
        }
    }
}
