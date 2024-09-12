using CalamityMod.Systems;
using CalamityMod.Tiles.Ores;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Tiles.FurnitureAuric
{
    public class AuricAbsorberTile : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;
            HitSound = SoundID.NPCHit34;
            AddMapEntry(new Color(192, 237, 255));
            DustType = DustID.Electric;
        }

        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            var tileCache = Main.tile[i, j];
            Color drawColour = GetDrawColour(i, j, Color.White);

            TileFramingSystem.SlopedGlowmask(in tileCache, i, j, TextureAssets.Tile[Type].Value, null, GetDrawColour(i, j, drawColour), default);
        }

        private Color GetDrawColour(int i, int j, Color colour)
        {
            int colType = Main.tile[i, j].TileColor;
            Color paintCol = WorldGen.paintColor(colType);
            if (colType >= 13 && colType <= 24)
            {
                colour.R = (byte)(paintCol.R / 255f * colour.R);
                colour.G = (byte)(paintCol.G / 255f * colour.G);
                colour.B = (byte)(paintCol.B / 255f * colour.B);
            }
            return colour;
        }
        public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
        {
            return TileFramingSystem.BetterGemsparkFraming(i, j, resetFrame);
        }
    }
}
