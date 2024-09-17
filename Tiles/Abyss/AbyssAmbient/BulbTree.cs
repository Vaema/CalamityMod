using CalamityMod.Dusts;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace CalamityMod.Tiles.Abyss.AbyssAmbient
{
    public class BulbTree1 : GlowMaskTile
    {
        public override string GlowMaskAsset => "CalamityMod/Tiles/Abyss/AbyssAmbient/BulbTree1Glow";

        public override void SetupStatic()
        {
            Main.tileLighted[Type] = true;
            Main.tileFrameImportant[Type] = true;
            TileObjectData.newTile.Width = 3;
            TileObjectData.newTile.Height = 3;
            TileObjectData.newTile.Origin = new Point16(1, 2);
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.Table | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
            TileObjectData.newTile.UsesCustomCanPlace = true;
            TileObjectData.newTile.CoordinateHeights = new int[] { 16, 16, 16 };
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinatePadding = 2;
            TileObjectData.newTile.WaterDeath = false;
            TileObjectData.newTile.LavaDeath = true;
            TileObjectData.newTile.DrawYOffset = 2;
            TileObjectData.addTile(Type);
            AddMapEntry(new Color(52, 124, 153));
            DustType = 33;
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            r = 0.1f;
            g = 0.32f;
            b = 0.5f;
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 2;
        }

        public override Color GetGlowMaskColor(int i, int j, TileDrawInfo drawData)
        {
            return Color.White;
        }
    }

    public class BulbTree2 : BulbTree1
    {
        public override string GlowMaskAsset => "CalamityMod/Tiles/Abyss/AbyssAmbient/BulbTree2Glow";
    }

    public class BulbTree3 : BulbTree1
    {
        public override string GlowMaskAsset => "CalamityMod/Tiles/Abyss/AbyssAmbient/BulbTree3Glow";
    }
}
