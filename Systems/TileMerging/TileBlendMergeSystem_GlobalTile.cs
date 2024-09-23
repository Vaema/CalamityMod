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
            public Dictionary<int, int> _TypesCounter = new();

            [SuppressMessage("Simplify Method Call", "IDE0002", Justification = "Leave this alone for Consistency")]
            public override void PostSetupTileMerge()
            {
                TileBlendMergeSystem.SetupMergeData();
            }

            public override bool TileFrame(int i, int j, int type, ref bool resetFrame, ref bool noBreak)
            {
                if (_TypesCounter.ContainsKey(type))
                    _TypesCounter[type]++;
                else
                    _TypesCounter[type] = 1;

                TileBlendMergeSystem.TileFrame(i, j, type);
                return base.TileFrame(i, j, type, ref resetFrame, ref noBreak);
            }

            public override void DrawEffects(int i, int j, int type, SpriteBatch spriteBatch, ref TileDrawInfo drawData)
            {
                TileBlendMergeSystem.DrawEffects(i, j, type, spriteBatch, ref drawData);
            }

            public override void AnimateTile()
            {
                Main.NewText($"- {_TypesCounter.Count} - ");
                foreach (var entry in _TypesCounter)
                {
                    
                    var mod = TileLoader.GetTile(entry.Key);
                    if (mod is not null)
                    {
                        Main.NewText($"{mod.Name} {entry.Value}");
                    }
                    else
                    {
                        Main.NewText($"{entry.Key} {entry.Value}");
                    }
                }
                _TypesCounter.Clear();
            }
        }
        #endregion
    }
}
