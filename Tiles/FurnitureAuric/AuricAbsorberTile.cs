using CalamityMod.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
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
            if (tileCache.IsTileActuallyInvisible())
                return;
            TileFramingSystem.SlopedGlowmask(in tileCache, i, j, TextureAssets.Tile[Type].Value, null, CalamityUtils.ApplyPaint(Main.tile[i, j].TileColor, Color.White), default);
        }

        public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
        {
            return TileFramingSystem.BetterGemsparkFraming(i, j, resetFrame);
        }
    }
}
