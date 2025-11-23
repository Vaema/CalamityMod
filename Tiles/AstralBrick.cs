using CalamityMod.Dusts;
using CalamityMod.Systems;
using CalamityMod.Tiles.Astral;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Tiles
{
    public class AstralBrick : GlowMaskTile
    {
        private const short subsheetWidth = 324;
        private const short subsheetHeight = 90;

        public override string GlowMaskAsset => "CalamityMod/Tiles/AstralBrickGlow";

        public override void SetupStatic()
        {
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;
            HitSound = SoundID.Tink;
            AddMapEntry(new Color(128, 128, 158));

            this.RegisterBlendMergeWith(TileID.Dirt);
            this.RegisterBlendMergeWith(ModContent.TileType<AstralDirt>());
        }

        public override bool CreateDust(int i, int j, ref int type)
        {
            Dust.NewDust(new Vector2(i, j) * 16f, 16, 16, ModContent.DustType<AstralBasic>(), 0f, 0f, 1, new Color(255, 255, 255), 1f);
            return false;
        }

        public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
        {
            return TileFramingSystem.BetterGemsparkFraming(i, j, resetFrame);
        }

        public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset)
        {
            int xPos = i % 2;
            int yPos = j % 2;
            frameXOffset = xPos * subsheetWidth;
            frameYOffset = yPos * subsheetHeight;
        }

        public override Color GetGlowMaskColor(int i, int j, TileDrawInfo drawData)
        {
            return new Color(50, 50, 50);
        }
    }
}
