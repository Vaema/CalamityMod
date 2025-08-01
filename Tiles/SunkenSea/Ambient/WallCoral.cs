using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace CalamityMod.Tiles.SunkenSea.Ambient
{
    public class WallCoral1 : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileSolidTop[Type] = false;
            Main.tileLighted[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style3x3);
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.None, 0, 0);
            TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
            TileObjectData.newAlternate.AnchorLeft = new AnchorData(AnchorType.SolidTile, 2, 0);
            TileObjectData.addAlternate(1);
            TileObjectData.newTile.AnchorRight = new AnchorData(AnchorType.SolidTile, 2, 0);
            TileObjectData.addTile(Type);
            DustType = 253;
            AddMapEntry(new Color(54, 69, 72));
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            r = 0.42f;
            g = 0.16f;
            b = 0.16f;
        }
    }

    public class WallCoral2 : WallCoral1
    {
        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            r = 0.213f;
            g = 0.349f;
            b = 0.416f;
        }
    }

    public class WallCoral3 : WallCoral1
    {
        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            r = 0.151f;
            g = 0.342f;
            b = 0.284f;
        }
    }

    public class WallCoral4 : WallCoral1
    {
        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            r = 0.38f;
            g = 0.28f;
            b = 0.32f;
        }
    }
}
