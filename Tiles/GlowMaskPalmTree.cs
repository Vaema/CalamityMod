using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace CalamityMod.Tiles
{
    public abstract class GlowMaskPalmTree : ModPalmTree
    {
        public abstract Asset<Texture2D> GetOasisTopGlowTextures();
        public abstract Asset<Texture2D> GetGlowTexture();
        public abstract Asset<Texture2D> GetTopGlowTextures();
    }
}
