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
    public sealed class FramedMaskTexture
    {
        /// <summary>
        /// Cached Texture2D reference, null on server
        /// </summary>
        public Texture2D Texture; // Leave this as field for performances sake
        
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
        private readonly bool[] _HasMaskContent;

        public FramedMaskTexture(string asset, int frameWidth, int frameHeight, bool pretendEveryFrameHaveGlow = false)
        {
            _FrameWidth = frameWidth;
            _FrameHeight = frameHeight;

            FramedGlowMaskSystem.OnUnload += Unload;

            // Don't do anything further on server
            if (Main.dedServ)
                return;

            Texture = ModContent.Request<Texture2D>(asset, AssetRequestMode.ImmediateLoad).Value;
            if (Texture is null)
                return;

            _TextureWidth = Texture.Width;
            _TextureHeight = Texture.Height;
            _FrameXCount = _TextureWidth / frameWidth;
            _FrameYCount = _TextureHeight / frameHeight;

            _HasMaskContent = new bool[FrameXCount * FrameYCount];

            
            if (pretendEveryFrameHaveGlow)
            {
                for(int x = 0; x<FrameXCount; x++)
                {
                    for (int y = 0; y<FrameYCount; y++)
                    {
                        _HasMaskContent[x + (y * _FrameXCount)] = true;
                    }
                }
            }
            else
            {
                Main.QueueMainThreadAction(() =>
                {
                    var colData = Texture.GetColorsFromTexture();
                    
                    // Interesting case, not sure if this will even happens
                    if (colData is null)
                        return;

                    Parallel.For(0, FrameXCount * FrameYCount, (i) =>
                    {
                        int xFrame = i % FrameXCount;
                        int yFrame = i / FrameXCount;

                        int xStart = xFrame * frameWidth;
                        int xEnd = Math.Min(xStart + frameWidth, _TextureWidth);

                        int yStart = yFrame * frameHeight;
                        int yEnd = Math.Min(yStart + frameHeight, _TextureHeight);

                        bool frameHasData = false;
                        for (int x = xStart; x < xEnd; x++)
                        {
                            if (frameHasData)
                            {
                                break;
                            }

                            for (int y = yStart; y < yEnd; y++)
                            {
                                Color col = colData[x, y];
                                if (col.A >= 1)
                                {
                                    frameHasData = true;
                                    break;
                                }
                            }
                        }

                        _HasMaskContent[xFrame + (yFrame * _FrameXCount)] = frameHasData;
                    });
                });
            }
        }

        public void Unload()
        {
            Texture = null;
            _FrameWidth = 0;
            _FrameHeight = 0;
            _FrameXCount = 0;
            _FrameYCount = 0;
        }

        public bool HasContentInFrameIndex(int xFrame, int yFrame)
        {
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
            int xFrame = xPos / _FrameWidth;
            int yFrame = yPos / _FrameHeight;

            if (xFrame < 0 || xFrame >= _FrameXCount)
                return false;

            if (yFrame < 0 || yFrame >= _FrameYCount)
                return false;

            return _HasMaskContent[xFrame + (yFrame * _FrameXCount)];
        }

        private sealed class FramedGlowMaskSystem : ModSystem
        {
            public static event Action OnUnload;

            public override void Unload()
            {
                OnUnload?.Invoke();
                OnUnload = null; // Clear cache completly
            }
        }
    }
}
