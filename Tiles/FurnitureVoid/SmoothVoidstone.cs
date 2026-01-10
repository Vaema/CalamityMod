
using System;
using CalamityMod.Systems;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;

namespace CalamityMod.Tiles.FurnitureVoid
{
    public class SmoothVoidstone : GlowMaskTile
    {
        public override void SetupStatic()
        {
            Main.tileSolid[Type] = true;
            Main.tileMergeDirt[Type] = true;
            Main.tileBlockLight[Type] = true;

            CalamityUtils.MergeWithGeneral(Type);
            CalamityUtils.MergeSmoothTiles(Type);
            CalamityUtils.MergeDecorativeTiles(Type);
            CalamityUtils.MergeWithAbyss(Type);

            HitSound = SoundID.Tink;
            MineResist = 2.1f;
            AddMapEntry(new Color(27, 24, 31));
        }

        int animationFrameWidth = 288;

        public override bool CreateDust(int i, int j, ref int type)
        {
            Dust.NewDust(new Vector2(i, j) * 16f, 16, 16, DustID.DungeonSpirit, 0f, 0f, 1, new Color(255, 255, 255), 1f);
            return false;
        }

        public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset)
        {
            frameXOffset = animationFrameWidth * TileFramingSystem.GetVariation4x4_01_Low0(i, j);
        }

        public override Color GetGlowMaskColor(int i, int j, TileDrawInfo drawData)
        {
            float brightness = 1f;
            float timeFactor = Main.GameUpdateCount * 0.007f;
            brightness *= (float)MathF.Sin(i / 18f + timeFactor);
            brightness *= (float)MathF.Sin(j / 18f + timeFactor);
            brightness *= (float)MathF.Sin(i * 18f + timeFactor);
            brightness *= (float)MathF.Sin(j * 18f + timeFactor);
            return Color.White * brightness;
        }
    }
}
