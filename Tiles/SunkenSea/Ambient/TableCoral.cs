using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace CalamityMod.Tiles.SunkenSea.Ambient
{
    public class TableCoral : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileSolidTop[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
            TileObjectData.newTile.StyleMultiplier = 2;
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.None, 0, 0);
            TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
            TileObjectData.newAlternate.AnchorLeft = new AnchorData(AnchorType.SolidTile, 2, 0);
            TileObjectData.newAlternate.Direction = TileObjectDirection.PlaceRight;
            TileObjectData.addAlternate(1);
            TileObjectData.newTile.AnchorRight = new AnchorData(AnchorType.SolidTile, 2, 0);
            TileObjectData.newTile.Direction = TileObjectDirection.PlaceLeft;
            TileObjectData.addTile(Type);
            DustType = 253;
            AddMapEntry(new Color(54, 69, 72));
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }
    }

    public class TableCoral2 : TableCoral
    {
    }

    public class TableCoral3 : TableCoral
    {
    }
}
