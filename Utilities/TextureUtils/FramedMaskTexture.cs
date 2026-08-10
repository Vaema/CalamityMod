using System;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod;

public sealed class FramedMaskTexture : IDeferredLoadTexture
{
    /// <summary>
    /// Cached Texture2D reference, null on server
    /// </summary>
    public Texture2D Texture; // Leave this as field for performances sake

    /// <summary>
    /// Check if Texture Asset is actually loaded
    /// </summary>
    public bool IsAssetLoaded => _Asset?.IsLoaded ?? false;

    /// <summary>
    /// X axis's frame count, 0 on server
    /// </summary>
    public int FrameXCount => _FrameXCount;

    /// <summary>
    /// Y axis's frame count, 0 on server
    /// </summary>
    public int FrameYCount => _FrameYCount;

    /// <summary>
    /// Pixel width for each frame
    /// </summary>
    public int FrameWidth => _FrameWidth;

    /// <summary>
    /// Pixel height for each frame
    /// </summary>
    public int FrameHeight => _FrameHeight;

    /// <summary>
    /// Pixel Width of Texture, 0 on server
    /// </summary>
    public int TextureWidth => _TextureWidth;

    /// <summary>
    /// Pixel Height of Texture, 0 on server
    /// </summary>
    public int TextureHeight => _TextureHeight;

    private int _FrameWidth = 0, _FrameHeight = 0;
    private int _FrameXCount = 0, _FrameYCount = 0;
    private int _TextureWidth = 0, _TextureHeight = 0;
    private bool[] _HasMaskContent;
    private Asset<Texture2D> _Asset;
    private readonly bool _EveryFrameHasContent = false;
    private bool _Prepared = false;

    public FramedMaskTexture(string asset, int frameWidth, int frameHeight, bool pretendEveryFrameHaveContent = false)
    {
        _FrameWidth = frameWidth;
        _FrameHeight = frameHeight;
        _EveryFrameHasContent = pretendEveryFrameHaveContent;
        _Prepared = pretendEveryFrameHaveContent;

        // Don't do anything further on server
        if (Main.dedServ)
            return;

        _Asset = ModContent.Request<Texture2D>(asset);
        Texture = _Asset.Value; // This should feed transparent pixel
        DeferredTextureLoadingManager.Enqueue(this);
    }

    public void OnTextureLoaded()
    {
        Texture = _Asset.Value;

        if (_Prepared)
            return;

        if (Texture is null)
            return;

        _TextureWidth = Texture.Width;
        _TextureHeight = Texture.Height;
        _FrameXCount = _TextureWidth / _FrameWidth;
        _FrameYCount = _TextureHeight / _FrameHeight;

        _HasMaskContent = new bool[FrameXCount * FrameYCount];

        Color[] colData = new Color[_TextureWidth * _TextureHeight];
        Texture.GetData(colData);

        Parallel.For(0, FrameXCount * FrameYCount, (i) =>
        {
            int xFrame = i % FrameXCount;
            int yFrame = i / FrameXCount;

            int xStart = xFrame * _FrameWidth;
            int xEnd = Math.Min(xStart + _FrameWidth, _TextureWidth);

            int yStart = yFrame * _FrameHeight;
            int yEnd = Math.Min(yStart + _FrameHeight, _TextureHeight);

            bool frameHasData = false;
            for (int x = xStart; x < xEnd; x++)
            {
                if (frameHasData)
                {
                    break;
                }

                for (int y = yStart; y < yEnd; y++)
                {
                    Color col = colData[x + (y * _TextureWidth)];
                    if (col.A >= 1)
                    {
                        frameHasData = true;
                        break;
                    }
                }
            }

            _HasMaskContent[xFrame + (yFrame * _FrameXCount)] = frameHasData;
        });

        _Prepared = true;
    }

    public void Unload()
    {
        Texture = null;
        _Asset = null;
        _Prepared = false;
        _FrameWidth = 0;
        _FrameHeight = 0;
        _FrameXCount = 0;
        _FrameYCount = 0;
    }

    public bool HasContentInFrameIndex(int xFrame, int yFrame)
    {
        if (!_Prepared)
            return false;

        if (_EveryFrameHasContent)
            return true; // TRUE

        if (Texture is null)
            return false;

        if (xFrame < 0 || xFrame >= _FrameXCount)
            return false;

        if (yFrame < 0 || yFrame >= _FrameYCount)
            return false;

        return _HasMaskContent[xFrame + (yFrame * _FrameXCount)];
    }

    public bool HasContentInFramePos(int xPos, int yPos)
    {
        if (!_Prepared)
            return false;

        if (_EveryFrameHasContent)
            return true; // TRUE

        int xFrame = xPos / _FrameWidth;
        int yFrame = yPos / _FrameHeight;

        if (xFrame < 0 || xFrame >= _FrameXCount)
            return false;

        if (yFrame < 0 || yFrame >= _FrameYCount)
            return false;

        return _HasMaskContent[xFrame + (yFrame * _FrameXCount)];
    }
}
