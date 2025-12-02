using System;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using ReLogic.Threading;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod
{
    /// <summary>
    /// Utility Class for using 2D Grayscale Texture as gradient
    /// </summary>
    public sealed class GrayscaleTexture2D : IDeferredLoadTexture
    {
        public Texture2D Texture { get; private set; }

        private int _Width = 0;
        private int _Height = 0;
        private float[] _Scales; // 2D array is not thread-safe

        private Asset<Texture2D> _Asset;
        private bool _Prepared;

        public bool IsAssetLoaded => _Asset?.IsLoaded ?? false;

        public GrayscaleTexture2D(string assetName)
        {
            if (Main.dedServ)
                return;

            _Asset = ModContent.Request<Texture2D>(assetName);
            if (_Asset is null)
                return;

            Texture = _Asset.Value;
            DeferredTextureLoadingManager.Enqueue(this);
        }

        public void Unload()
        {
            _Width = 0;
            _Height = 0;
            _Scales = null;
            _Asset = null;
            Texture = null;
            _Prepared = false;
        }

        public void OnTextureLoaded()
        {
            if (_Prepared)
                return;

            Texture = _Asset.Value;
            if (Texture is null)
                return;

            _Width = Texture.Width;
            _Height = Texture.Height;
            _Scales = new float[_Width * _Height];

            var colorScheme = Texture.GetColorsFromTexture();
            FastParallel.For(0, _Width * _Height, (startInclusive, endExclusive, context) =>
            {
                for (int i = startInclusive; i < endExclusive; i++)
                {
                    var y = Math.DivRem(i, _Width, out var x);
                    _Scales[i] = colorScheme[x, y].R / 255.0f;
                }
            });

            _Prepared = true;
        }

        public float GetClamp(int x, int y)
        {
            if (!_Prepared)
                return default;

            if (_Width == 0 || _Height == 0)
                return default;

            x = Math.Clamp(x, 0, _Width - 1);
            y = Math.Clamp(x, 0, _Height - 1);
            return _Scales[x + (y * _Height)];
        }

        public float GetRepeat(int x, int y)
        {
            if (!_Prepared)
                return default;

            if (_Width == 0 || _Height == 0)
                return default;

            x %= _Width;
            y %= _Height;
            return _Scales[x + (y * _Height)];
        }
    }
}
