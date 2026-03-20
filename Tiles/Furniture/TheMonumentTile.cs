using CalamityMod.Items.Placeables.Furniture;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace CalamityMod.Tiles.Furniture
{
    public class TheMonumentTile : ModTile
    {
        public override string Texture => "CalamityMod/Tiles/Furniture/Monoliths/BlueDistortedMonolithTile"; //Placeholder
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style3x3);
            TileObjectData.newTile.Width = 5;
            TileObjectData.newTile.Height = 5;
            TileObjectData.newTile.Origin = new Point16(2, 4);
            TileObjectData.newTile.CoordinateHeights = new[] { 16, 16, 16, 16, 18 };
            TileObjectData.newTile.LavaDeath = false;
            TileObjectData.newTile.UsesCustomCanPlace = true;
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop, 5, 0);

            AnimationFrameHeight = TileObjectData.newTile.CoordinateFullHeight;
            TileObjectData.addTile(Type);
        }
    }
}
