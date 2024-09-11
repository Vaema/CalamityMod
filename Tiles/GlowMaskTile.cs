using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Tiles
{
    public abstract class GlowMaskTile : ModTile
    {
        public enum PaintColorTint
        {
            OnlyByDeepPaint,
            ByEveryPaint,
            None,
        }

        public FramedMaskTexture GlowMask;

        internal static GlowMaskTile[] InstanceLookup; // This Lookup is Array for performances sake
        internal static int LookupLength;

        public PaintColorTint GlowMaskPaintInteraction = PaintColorTint.OnlyByDeepPaint;

        public abstract string GlowMaskAsset { get; }

        public sealed override void SetStaticDefaults()
        {
            if (GlowMask != null)
            {
                CalamityMod.Instance.Logger.Error($"{Name} has called {nameof(SetStaticDefaults)} themselve! This is not allowed!");
                return;
            }

            GlowMask = new(GlowMaskAsset, 18, 18);

            InstanceLookup ??= new GlowMaskTile[TileLoader.TileCount];
            LookupLength = InstanceLookup.Length;

            InstanceLookup[Type] = this;

            SetupStatic();
        }

        public sealed override void Unload()
        {
            GlowMask?.Unload();
            GlowMask = null;

            InstanceLookup = null;

            OnUnload();
        }

        public virtual void SetupStatic() { }

        public virtual void OnUnload() { }

        public abstract Color GetGlowMaskColor(int i, int j, TileDrawInfo drawData);

        public static Color ApplyPaint(int paintType, Color color, bool deepPaintOnly = true)
        {
            if (deepPaintOnly && !IsDeepPaint(paintType))
                return color;

            Color paintCol = WorldGen.paintColor(paintType);
            color = color.MultiplyRGB(paintCol);
            return color;
        }

        private static bool IsDeepPaint(int paintType)
        {
            return PaintID.DeepRedPaint <= paintType && paintType <= PaintID.DeepPinkPaint;
        }
    }
}
