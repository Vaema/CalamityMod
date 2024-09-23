using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Terraria.DataStructures;
using Terraria;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;

namespace CalamityMod.Systems
{
    public sealed partial class TileBlendMergeSystem : ModSystem
    {
        private static void DrawEffects(int i, int j, int type, SpriteBatch spriteBatch, ref TileDrawInfo drawData)
        {
            if (!InValidRange(i, j))
                return;

            Vector2 zero = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange, Main.offScreenRange);
            Vector2 drawPos = new Vector2(i * 16, j * 16) - Main.screenPosition + zero;
            Color drawColor = drawData.tileLight;
            for (int idx = 0; idx < 8; idx++)
            {
                var data = drawData.tileCache.Get<TileBlendingData>().GetData(idx);
                var sheetIdx = drawData.tileCache.Get<TileBlendingData>().GetSheetIndex(idx);
                if (sheetIdx == EmptySheetIndex)
                    continue;

                //var rect = new Rectangle(x, y, 16, 16);
                //spriteBatch.Draw(texture, drawPos, rect, drawColor, rotation: 0.0f, origin: default, scale: 1.0f, SpriteEffects.None, layerDepth: 0.0f);
            }
        }
    }
}
