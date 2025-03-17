using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.ModLoader;
using Terraria;
using Microsoft.Xna.Framework;

namespace CalamityMod
{
    /// <summary>
    /// Utility Class for using 1D Grayscale Texture as gradient
    /// </summary>
    public sealed class GrayscaleTexture1D
    {
        private int _Width = 0;
        private float[] _Scales;

        public GrayscaleTexture1D(string assetName)
        {
            if (Main.dedServ)
                return;

            var texture = ModContent.Request<Texture2D>(assetName, AssetRequestMode.ImmediateLoad).Value;
            if (texture is null)
                return;

            Main.QueueMainThreadAction(() =>
            {
                _Width = texture.Width;
                _Scales = new float[texture.Width];

                var colorScheme = new Color[_Width];
                texture.GetData(colorScheme);
                for (int i = 0; i<_Width; i++)
                {
                    _Scales[i] = colorScheme[i].R / 255.0f;
                }
            });
        }

        public void Unload()
        {
            _Width = 0;
            _Scales = null;
        }

        public float GetClamp(int x)
        {
            if (_Width == 0)
                return default;

            x = Math.Clamp(x, 0, _Width - 1);
            return _Scales[x];
        }

        public float GetRepeat(int x)
        {
            if (_Width == 0)
                return default;

            x %= _Width;
            return _Scales[x];
        }
    }
}
