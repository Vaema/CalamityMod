using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Walls
{
    public class VoidstoneWallUnsafe : ModWall
    {
        internal static FramedMaskTexture GlowMask;
        public override string Texture => "CalamityMod/Walls/VoidstoneWall";

        public override void SetStaticDefaults()
        {
            // We basically have same copy in VoidstoneWall
            // But leaving this in case of changing to Unsafe variant specific glowmask
            GlowMask = new("CalamityMod/Walls/VoidstoneWall_Glowmask", 36, 36);

            DustType = 187;
            AddMapEntry(new Color(0, 0, 0));
        }

        public override void Unload()
        {
            GlowMask?.Unload();
            GlowMask = null;
        }

        public override void RandomUpdate(int i, int j)
        {
            if (Main.tile[i, j].LiquidAmount == 0 && j < Main.maxTilesY - 205)
            {
                Main.tile[i, j].Get<LiquidData>().LiquidType = LiquidID.Water;
                Main.tile[i, j].LiquidAmount = byte.MaxValue;
                WorldGen.SquareTileFrame(i, j);
                if (Main.dedServ)
                    NetMessage.sendWater(i, j);
            }
        }

        public static bool DrawWallGlow(int type, int i, int j, SpriteBatch spriteBatch)
        {
            if (GlowMask.Texture is null)
                return true;

            spriteBatch.SafeBegin(SpriteSortMode.Immediate, BatchSetting.AlphaBlend, null, Main.GameViewMatrix.TransformationMatrix, () =>
            {
                Tile tile = Main.tile[i, j];
    			Effect tileShader = Main.tileShader;
                TreePaintingSettings settings = TreePaintSystemData.GetWallSettings(type);
                int paintColor = tile.WallColor;
			    tileShader.Parameters["leafHueTestOffset"]?.SetValue(settings.HueTestOffset);
    			tileShader.Parameters["leafMinHue"]?.SetValue(settings.SpecialGroupMinimalHueValue);
	    		tileShader.Parameters["leafMaxHue"]?.SetValue(settings.SpecialGroupMaximumHueValue);
		    	tileShader.Parameters["leafMinSat"]?.SetValue(settings.SpecialGroupMinimumSaturationValue);
			    tileShader.Parameters["leafMaxSat"]?.SetValue(settings.SpecialGroupMaximumSaturationValue);
		    	tileShader.Parameters["invertSpecialGroupResult"]?.SetValue(settings.InvertSpecialGroupResult);
	    		int index = Main.ConvertPaintIdToTileShaderIndex(paintColor, settings.UseSpecialGroups, settings.UseWallShaderHacks);
    			tileShader.CurrentTechnique.Passes[index].Apply();

                Texture2D sprite = TextureAssets.Wall[type].Value;
                Vector2 offset = new Vector2(i * 16 - Main.screenPosition.X, j * 16 - Main.screenPosition.Y) + (Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange)) - Vector2.One * 8f;
                int yPos = tile.WallFrameY;
                int xPos = tile.WallFrameX;
                Rectangle frame = new Rectangle(xPos, yPos, 32, 32);
                Color lightColor = tile.IsWallFullbright ? Color.White : Lighting.GetColor(i, j);

                spriteBatch.Draw(sprite, offset, frame, lightColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);

                if (GlowMask.HasContentInFramePos(xPos, yPos))
                {
                    Color glowColor = Color.Lerp(lightColor, Color.White, 0.2f + MathF.Sin(Main.GameUpdateCount * 0.007f) * 0.1f);
                    spriteBatch.Draw(GlowMask.Texture, offset, frame, glowColor, 0f, Vector2.Zero, 1f, SpriteEffects.None, 0f);
                }
            });
            return false;
        }

        public override bool PreDraw(int i, int j, SpriteBatch spriteBatch) => DrawWallGlow(Type, i, j, spriteBatch);

        public override void KillWall(int i, int j, ref bool fail) => fail = true;

        public override bool CanExplode(int i, int j) => false;

        public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;
    }
}
