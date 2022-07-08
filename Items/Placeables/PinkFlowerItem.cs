using CalamityMod.Tiles.FloralParadise;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables
{
    // TODO -- Come up with a better name for this.
    public class PinkFlowerItem : ModItem
    {
        public override void SetStaticDefaults()
        {
            DisplayName.SetDefault("Pink Flower");
            SacrificeTotal = 10;
        }

        public override void SetDefaults()
        {
            Item.createTile = ModContent.TileType<PinkFlower>();
            Item.useStyle = ItemUseStyleID.Swing;
            Item.useTurn = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.autoReuse = true;
            Item.consumable = true;
            Item.width = 24;
            Item.height = 24;
            Item.maxStack = 999;
        }
    }
}
