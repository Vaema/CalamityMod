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

        // 204 slots per sheet (1 empty slot per sheet)
        public const int BlendTextureXCount = 17;
        public const int BlendTextureYCount = 12;

        // FA: 2024/OCT/01
        // Removing Margin as I believe artifacts never happens on Terraria
        // And built sheets are not meant to be edited/viewed by human.
        // But if graphic artifact happens, Please set these value to 18 and remove this comment
        public const int BlendTextureFrameWidth = 16;
        public const int BlendTextureFrameHeight = 16;

        public const int BlendTextureWidth = BlendTextureFrameWidth * BlendTextureXCount;
        public const int BlendTextureHeight = BlendTextureFrameHeight * BlendTextureYCount;
        public const int BlendTextureFullHeight = BlendTextureHeight * VariantCount;
        #endregion


        #region Properties
        public Asset<Texture2D> TextureAsset { get; private set; }
        public int Slot { get; private set; } = -1;
        public RenderTarget2D BakedBlendTexture { get; private set; }
        #endregion


        #region Overrides
        public abstract int TileType { get; }
        #endregion


        #region Setups

        protected sealed override void Register()
        {
            CalculateSheetPositionLookup();

            ModTypeLookup<TileBlendTexture>.Register(this);
            Slot = TileBlendTextureLoader.Register(this);
            TextureAsset = ModContent.Request<Texture2D>(Texture);
            BakedBlendTexture = null;
        }

        public sealed override void SetupContent()
        {
            SetStaticDefaults();
        }

        public sealed override void Unload()
        {
            Main.QueueMainThreadAction(() =>
            {
                BakedBlendTexture?.Dispose();
                BakedBlendTexture = null;
            });

            PostUnload();
        }

        public virtual void PostUnload()
        {

        }
        #endregion


        #region Public API
        public void RebuildBlendSheet(Asset<Texture2D> texture = null)
        {
            TextureAsset = texture ?? ModContent.Request<Texture2D>(Texture);
            ClearBakeCache();
        }
        #endregion
    }
}
