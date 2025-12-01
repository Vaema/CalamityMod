using System;
using CalamityMod.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Tiles.FurnitureWulfrum
{

    public class WulfrumEnergyBarrier : ModTile
    {
        public static int TypeCache;

        public Asset<Texture2D> ReflectTexture;

        public override void SetStaticDefaults()
        {
            TypeCache = Type;
            Main.tileLighted[Type] = true;
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = false;
            Main.tileBrick[Type] = true;

            CalamityUtils.MergeWithGeneral(Type);
            CalamityUtils.MergeDecorativeTiles(Type);

            DustType = 108;
            AddMapEntry(new Color(194, 255, 67));
            HitSound = SoundID.Shatter;
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }
        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            float brightness = 0.9f;
            Color lime = new Color(194, 255, 67);
            Color green = new Color(110, 192, 93);
            Color value = Color.Lerp(lime, green, (MathF.Sin(-j / 80f + Main.GameUpdateCount * 0.017f + i / 40f) + 1f) / 2f);
            Color value1 = Color.Lerp(lime, green, (MathF.Sin((j - 100) / 50f + Main.GameUpdateCount * 0.004f + -i / 30f) + 1f) / 2f);

            r = (value.R + value1.R) / 800f;
            g = (value.G + value1.G) / 800f;
            b = (value.B + value1.B) / 800f;
            r *= brightness;
            g *= brightness;
            b *= brightness;
        }
        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            if (Main.tile[i, j].IsTileActuallyInvisible())
                return;

            float brightness = MathHelper.Clamp(0.2f - (j / 680), 0f, 0.2f);

            float time = Main.GameUpdateCount;
            float waveScale1 = time * 0.064f;
            int scalar = i - (j / 2);
            float wave1 = waveScale1 * -50 + scalar * 12;
            float wave1angle = 0.30f + 0.25f * MathF.Sin(MathHelper.ToRadians(wave1));

            float transparency = 0.05f + wave1angle;


            // Must be set here 
            TileID.Sets.DrawsWalls[Type] = true;
            Main.tileNoSunLight[Type] = false;

            ReflectTexture ??= ModContent.Request<Texture2D>("CalamityMod/Tiles/FurnitureWulfrum/WulfrumEnergyBarrierReflect");
            Texture2D tex = ReflectTexture.Value;

            Tile tile = Main.tile[i, j];
            Rectangle frame = new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, 16);

            TileFramingSystem.SlopedGlowmask(in tile, i, j, tex, frame, CalamityUtils.ApplyPaint(Main.tile[i, j].TileColor, Color.White * transparency, false), default);
        }

        public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
        {
            TileFramingSystem.CompactFraming(i, j, resetFrame);
            return false;
        }
    }
}
