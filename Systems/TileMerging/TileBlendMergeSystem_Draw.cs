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
using Terraria.GameContent.Drawing;

namespace CalamityMod.Systems
{
    public sealed partial class TileBlendMergeSystem : ModSystem
    {
        private static void OnDrawSingleTile(On_TileDrawing.orig_DrawSingleTile orig, TileDrawing self, TileDrawInfo drawData, bool solidLayer, int waterStyleOverride, Vector2 screenPosition, Vector2 screenOffset, int tileX, int tileY)
        {
            orig(self, drawData, solidLayer, waterStyleOverride, screenPosition, screenOffset, tileX, tileY);

            var tile = drawData.tileCache;

            Color drawColor = drawData.tileLight;
            if (drawColor.R <= 0 && drawColor.G <= 0 && drawColor.B <= 0)
                return;

            Vector2 drawPos = new Vector2(tileX * 16, tileY * 16) - screenPosition + screenOffset;
            var blendingData = tile.Get<TileBlendingData>();
            for (int idx = 0; idx < 8; idx++)
            {
                blendingData.Get(idx, out var sheetIdx, out var data);

                // Break here as standard for TileBlendingData is 0->Count fill, so further fields should be also Invalid
                if (sheetIdx == EmptySheetIndex)
                    break;

                var rect = SideDataToSheetRect(data);
                var variant = Math.Clamp(tile.TileFrameNumber, 0, 2);
                Main.spriteBatch.Draw(_BlendTextures[sheetIdx, variant], drawPos, rect, drawColor, rotation: 0.0f, origin: default, scale: 1.0f, SpriteEffects.None, layerDepth: 0.0f);
            }
        }
    }
}
