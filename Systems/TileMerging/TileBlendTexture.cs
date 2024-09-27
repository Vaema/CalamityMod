using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
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
        public const int BlendTextureWidth = 18 * BlendTextureXCount;
        public const int BlendTextureHeight = 18 * BlendTextureYCount;
        #endregion



        #region Properties
        public Asset<Texture2D> TextureAsset { get; private set; }
        public int Slot { get; private set; }
        public RenderTarget2D[] BlendTextures { get; private set; } // dimension: [3]
        #endregion



        #region Overrides
        public abstract int TileType { get; }
        #endregion


        #region Setups
        protected sealed override void Register()
        {
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



        public void RebuildBlendSheet(Texture2D texture = null)
        {
            texture ??= TextureAsset.Value;
            BakeBlendTexture(texture);
        }
    }
}
