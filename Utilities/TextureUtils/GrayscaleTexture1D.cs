using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using ReLogic.Threading;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod;

/// <summary>
/// Utility Class for using 1D Grayscale Texture as gradient
/// </summary>
public sealed class GrayscaleTexture1D : IDeferredLoadTexture
{
    public Texture2D Texture { get; private set; }

    private int _Width = 0;
    private float[] _Scales;

    private Asset<Texture2D> _Asset;
    private bool _Prepared;

    public bool IsAssetLoaded => _Asset?.IsLoaded ?? false;

    public GrayscaleTexture1D(string assetName)
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
        _Scales = new float[Texture.Width];

        var colorScheme = new Color[_Width];
        Texture.GetData(colorScheme);
        FastParallel.For(0, _Width, (startInclusive, endExclusive, context) =>
        {
            for (int i = startInclusive; i < endExclusive; i++)
            {
                _Scales[i] = colorScheme[i].R / 255.0f;
            }
        });

        _Prepared = true;
    }

    public float GetClamp(int x)
    {
        if (!_Prepared)
            return default;

        if (_Width == 0)
            return default;

        x = Math.Clamp(x, 0, _Width - 1);
        return _Scales[x];
    }

    public float GetRepeat(int x)
    {
        if (!_Prepared)
            return default;

        if (_Width == 0)
            return default;

        x %= _Width;
        return _Scales[x];
    }
}
