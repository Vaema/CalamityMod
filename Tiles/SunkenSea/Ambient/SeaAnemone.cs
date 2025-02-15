using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace CalamityMod.Tiles.SunkenSea.Ambient
{
    public class SeaAnemone : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileLighted[Type] = true;
            AnimationFrameHeight = 36;
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
            TileObjectData.addTile(Type);
            DustType = 253;
            AddMapEntry(new Color(54, 69, 72));

            base.SetStaticDefaults();
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            float brightness = 0.7f;
            float declareThisHereToPreventRunningTheSameCalculationMultipleTimes = Main.GameUpdateCount * 0.01f;
            brightness *= (float)MathF.Sin(-j / 8f + declareThisHereToPreventRunningTheSameCalculationMultipleTimes + i);
            brightness *= (float)MathF.Sin(-i / 8f + declareThisHereToPreventRunningTheSameCalculationMultipleTimes + j);
            brightness += 0.4f;
            brightness = MathHelper.Clamp(brightness, 0.1f, 0.5f);
            r = 85f / 255f;
            b = 151f / 255f;
            g = 196f / 255f;
            r *= brightness;
            g *= brightness;
            b *= brightness;
        }
        public override void AnimateTile(ref int frame, ref int frameCounter)
        {
            frameCounter++;
            if (frameCounter > 12)
            {
                frameCounter = 0;
                frame++;
                if (frame > 5)
                    frame = 0;
            }
        }
    }
}
