using CalamityMod.Systems;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

namespace CalamityMod.Tiles.FurniturePlaguedPlate
{
    public class PlaguedPlate : GlowMaskTile
    {
        public override string GlowMaskAsset => "CalamityMod/Tiles/FurniturePlaguedPlate/PlaguedPlateGlow";

        public override void SetupStatic()
        {
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;
            HitSound = SoundID.Tink;
            MineResist = 2.1f;
            AddMapEntry(new Color(51, 99, 75));
        }

        public override bool CreateDust(int i, int j, ref int type)
        {
            Dust.NewDust(new Vector2(i, j) * 16f, 16, 16, DustID.BubbleBurst_Green, 0f, 0f, 1, new Color(255, 255, 255), 1f);
            return false;
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }

        public override Color GetGlowMaskColor(int i, int j, TileDrawInfo drawData)
        {
            return new Color(128, 128, 128);
        }

        public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
        {
            return TileFramingSystem.BrimstoneFraming(i, j, resetFrame);
        }
    }
}
