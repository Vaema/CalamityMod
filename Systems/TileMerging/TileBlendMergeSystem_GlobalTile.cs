using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.ModLoader;

namespace CalamityMod.Systems
{
    public sealed partial class TileBlendMergeSystem : ModSystem
    {
        #region Don't mind this, This is a small GlobalTile for hooks
        [Autoload(Side = ModSide.Client)]
        private class FancyTileMergeGlobalTile : GlobalTile
        {
            [SuppressMessage("Simplify Method Call", "IDE0002", Justification = "Leave this alone for Consistency")]
            public override void PostSetupTileMerge()
            {
                TileBlendMergeSystem.SetupMergeData();
            }

            public override bool TileFrame(int i, int j, int type, ref bool resetFrame, ref bool noBreak)
            {
                TileBlendMergeSystem.TileFrame(i, j, type);
                return base.TileFrame(i, j, type, ref resetFrame, ref noBreak);
            }
        }
        #endregion
    }
}
