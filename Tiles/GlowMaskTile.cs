using Microsoft.Xna.Framework;
using Terraria.DataStructures;
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
        public bool GlowMaskAffectedByLight = true;
        public bool GlowMaskCanBeCulled = true;

        public abstract string GlowMaskAsset { get; }

        public sealed override void SetStaticDefaults()
        {
            if (GlowMask != null)
            {
                CalamityMod.Log.Error($"{Name} has called {nameof(SetStaticDefaults)} themselve! This is not allowed!");
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
    }
}
