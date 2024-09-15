using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.ModLoader;

namespace CalamityMod.Tiles
{
    public abstract class GlowMaskCactus : ModCactus
    {
        public abstract Asset<Texture2D> GetFruitGlowTexture();
        public abstract Asset<Texture2D> GetGlowTexture();
    }
}
