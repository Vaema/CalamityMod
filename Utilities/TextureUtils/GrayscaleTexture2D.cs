using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod
{
    /// <summary>
    /// Utility Class for using 2D Grayscale Texture as gradient
    /// </summary>
    public sealed class GrayscaleTexture2D
    {
        private int _Width = 0;
        private int _Height = 0;
        private float[] _Scales; // 2D array is not thread-safe

        public GrayscaleTexture2D(string assetName)
        {
            if (Main.dedServ)
                return;

            var texture = ModContent.Request<Texture2D>(assetName, AssetRequestMode.ImmediateLoad).Value;
            if (texture is null)
                return;

            Main.QueueMainThreadAction(() =>
            {
                _Width = texture.Width;
                _Height = texture.Height;
                _Scales = new float[_Width * _Height];

                var colorScheme = texture.GetColorsFromTexture();
                for (int i = 0; i < _Width; i++)
                {
                    for (int j = 0; j < _Height; j++)
                    {
                        _Scales[(j * _Width) + i] = colorScheme[i, j].R / 255.0f;
                    }
                }
            });
        }

        public void Unload()
        {
            _Width = 0;
            _Height = 0;
            _Scales = null;
        }

        public float GetClamp(int x, int y)
        {
            if (_Width == 0 || _Height == 0)
                return default;

            x = Math.Clamp(x, 0, _Width - 1);
            y = Math.Clamp(x, 0, _Height - 1);
            return _Scales[x + (y * _Height)];
        }

        public float GetRepeat(int x, int y)
        {
            if (_Width == 0 || _Height == 0)
                return default;

            x %= _Width;
            y %= _Height;
            return _Scales[x + (y * _Height)];
        }
    }
}
