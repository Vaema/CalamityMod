using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ObjectData;

namespace CalamityMod.Tiles.Abyss.AbyssAmbient
{
    public class SpiderCoral1 : GlowMaskTile
    {
        public override string GlowMaskAsset => "CalamityMod/Tiles/Abyss/AbyssAmbient/SpiderCoral1Glow";

        public override void SetupStatic()
        {
            Main.tileLighted[Type] = true;
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
            TileObjectData.addTile(Type);
            AddMapEntry(new Color(82, 49, 27));
            DustType = DustID.Sand;
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            r = 0.48f;
            g = 0.12f;
            b = 0.08f;
        }

        public override Color GetGlowMaskColor(int i, int j, TileDrawInfo drawData)
        {
            return Color.White;
        }
    }

    public class SpiderCoral2 : SpiderCoral1
    {
        public override string GlowMaskAsset => "CalamityMod/Tiles/Abyss/AbyssAmbient/SpiderCoral2Glow";
    }

    public class SpiderCoral3 : SpiderCoral1
    {
        public override string GlowMaskAsset => "CalamityMod/Tiles/Abyss/AbyssAmbient/SpiderCoral3Glow";
    }

    public class SpiderCoral4 : SpiderCoral1
    {
        public override string GlowMaskAsset => "CalamityMod/Tiles/Abyss/AbyssAmbient/SpiderCoral4Glow";
    }

    public class SpiderCoral5 : SpiderCoral1
    {
        public override string GlowMaskAsset => "CalamityMod/Tiles/Abyss/AbyssAmbient/SpiderCoral5Glow";
    }
}
