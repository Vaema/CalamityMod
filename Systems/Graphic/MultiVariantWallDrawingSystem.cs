using CalamityMod.Walls;
using Microsoft.Xna.Framework;
using MonoMod.Cil;
using Terraria;
using Terraria.GameContent.Drawing;
using Terraria.ModLoader;

namespace CalamityMod.Systems
{
    internal sealed class MultiVariantWallDrawingSystem : ModSystem
    {
        public sealed override void Load()
        {
            IL_WallDrawing.DrawWalls += DrawingFrameOffsetForWallSupport;
        }

        private void DrawingFrameOffsetForWallSupport(ILContext il)
        {
            var cursor = new ILCursor(il);

            if (!cursor.TryGotoNext(x => x.MatchCall<Tile>("wallFrameY")))
            {
                LogILFailure("call::Tile.wallFrameY was not found!");
                return;
            }

            int tileLocalIdx = -1;
            int rectLocalIdx = -1;
            if (!cursor.TryGotoPrev(x => x.MatchLdloca(out tileLocalIdx))) // First ldloca should be "tile"
            {
                LogILFailure("Ldloca::tile was not found!");
                return;
            }

            if (!cursor.TryGotoPrev(x => x.MatchLdloca(out rectLocalIdx))) // Second ldloca should be "value" which is Rectangle for drawing
            {
                LogILFailure("Ldloca::rect was not found!");
                return;
            }

            if (!cursor.TryGotoNext(MoveType.After, x => x.MatchStfld<Rectangle>(nameof(Rectangle.Y))))
            {
                LogILFailure("Stfld::Rectangle.Y was not found!");
                return;
            }

            cursor.EmitLdloc(tileLocalIdx);
            cursor.EmitLdloca(rectLocalIdx);
            cursor.EmitDelegate((Tile tile, ref Rectangle drawRect) =>
            {
                if (ModContent.GetModWall(tile.WallType) is MultiVariantModWall mvWall)
                {
                    tile.TilePos(out var i, out var j);

                    int offsetX = 0;
                    int offsetY = 0;

                    mvWall.PopulateWallVariant(i, j, ref offsetX, ref offsetY);

                    drawRect.X += offsetX;
                    drawRect.Y += offsetY;
                }
            });
        }

        private static void LogILFailure(string reason)
        {
            CalamityMod.Log.ILFailure("Support for Wall FrameOffset", reason);
        }
    }
}
