using CalamityMod.Systems;
using CalamityMod.Tiles.Ores;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

namespace CalamityMod.Tiles.FurnitureAuric
{
    public class AuricRepulserPanelTile : GlowMaskTile
    {
        public override string GlowMaskAsset => "CalamityMod/Tiles/FurnitureAuric/AuricRepulserPanelTile_Glow";

        public override void SetupStatic()
        {
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;
            HitSound = AuricOre.MineSound;
            MineResist = 3f;
            AddMapEntry(new Color(225, 208, 125));
        }

        public override bool CreateDust(int i, int j, ref int type)
        {
            Dust.NewDust(new Vector2(i, j) * 16f, 16, 16, DustID.Pixie, 0f, 0f, 1, new Color(255, 255, 255), 1f);
            Dust.NewDust(new Vector2(i, j) * 16f, 16, 16, DustID.Electric, 0f, 0f, 1, new Color(255, 255, 255), 1f);
            return false;
        }

        public override Color GetGlowMaskColor(int i, int j, TileDrawInfo drawData)
        {
            return Color.White;
        }

        public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
        {
            return TileFramingSystem.BetterGemsparkFraming(i, j, resetFrame);
        }
    }
}
