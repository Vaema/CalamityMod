using System;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Systems
{
    [Autoload(Side = ModSide.Client)]
    public abstract partial class TileBlendTexture : ModTexturedType
    {
        #region Constants
        public const byte EmptySheetIndex = byte.MaxValue;
        public const int VariantCount = 3;

        public const int BlendTextureXCount = 16;
        public const int BlendTextureYCount = 16;

        // FA: 2024/OCT/01
        // Removing Margin as I believe artifacts never happens on Terraria
        // And built sheets are not meant to be edited/viewed by human.
        // But if graphic artifact happens, Please set these value to 18 and remove this comment
        public const int BlendTextureFrameWidth = 16;
        public const int BlendTextureFrameHeight = 16;

        public const int BlendTextureWidth = BlendTextureFrameWidth * BlendTextureXCount;
        public const int BlendTextureHeight = BlendTextureFrameHeight * BlendTextureYCount;
        #endregion


        #region Properties
        public Asset<Texture2D> TextureAsset { get; private set; }
        public int Slot { get; private set; } = -1;
        public RenderTarget2D[] BlendTextures { get; private set; } // dimension: [3]
        #endregion


        #region Overrides
        public abstract int TileType { get; }
        #endregion


        #region Setups
        protected sealed override void Register()
        {
            ModTypeLookup<TileBlendTexture>.Register(this);
            Slot = TileBlendTextureLoader.Register(this);
            TextureAsset = ModContent.Request<Texture2D>(Texture);
            BlendTextures = new RenderTarget2D[3];

            Main.QueueMainThreadAction(() =>
            {
                for (int v = 0; v < VariantCount; v++)
                    BlendTextures[v] = new(
                        Main.instance.GraphicsDevice,
                        BlendTextureWidth,
                        BlendTextureHeight,
                        mipMap: false,
                        preferredFormat: SurfaceFormat.Color,
                        preferredDepthFormat: DepthFormat.None,
                        preferredMultiSampleCount: 0,
                        usage: RenderTargetUsage.PreserveContents);
            });
        }

        public sealed override void SetupContent()
        {
            SetStaticDefaults();
        }

        public sealed override void Unload()
        {
            Main.QueueMainThreadAction(() =>
            {
                if (BlendTextures is not null)
                {
                    foreach (var rt in BlendTextures)
                    {
                        if (rt is null)
                            continue;

                        if (rt.IsDisposed)
                            continue;

                        rt.Dispose();
                    }

                    Array.Clear(BlendTextures);
                    BlendTextures = null;
                }
            });

            PostUnload();
        }

        public virtual void PostUnload()
        {

        }
        #endregion


        #region Public API
        public void RebuildBlendSheet(Texture2D texture = null)
        {
            texture ??= TextureAsset.Value;
            BakeBlendTexture(texture);
        }
        #endregion
    }
}
