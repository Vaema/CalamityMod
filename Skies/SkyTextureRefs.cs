using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.ModLoader;

namespace CalamityMod.Skies
{
    [Autoload(Side = ModSide.Client)]
    internal sealed class SkyTextureRefs : ModSystem
    {
        // Astral Sky/BG
        public static Texture2D AstralSky;
        public static Texture2D AstralSurfaceFront;
        public static Texture2D AstralSurfaceFrontGlow;
        public static Texture2D AstralSurfaceClose;
        public static Texture2D AstralSurfaceCloseGlow;
        public static Texture2D AstralSurfaceMiddle;
        public static Texture2D AstralSurfaceMiddleGlow;

        // Astral Desert Sky/BG
        public static Texture2D AstralDesertSurfaceClose;
        public static Texture2D AstralDesertSurfaceMiddle;

        // Astral Snow Sky/BG
        public static Texture2D AstralSnowSurfaceMiddle;

        // Sulphur Sea Sky/BG
        public static Texture2D SulphurSeaSky;
        public static Texture2D SulphurSeaSkyFront;
        public static Texture2D SulphurSeaSurface;

        public override void OnModLoad()
        {
            // Astral Sky/BG
            AstralSky = ModContent.Request<Texture2D>("CalamityMod/Skies/AstralSky", AssetRequestMode.ImmediateLoad).Value;
            AstralSurfaceFront = ModContent.Request<Texture2D>("CalamityMod/Backgrounds/AstralSurfaceFront", AssetRequestMode.ImmediateLoad).Value;
            AstralSurfaceFrontGlow = ModContent.Request<Texture2D>("CalamityMod/Backgrounds/AstralSurfaceFrontGlow", AssetRequestMode.ImmediateLoad).Value;
            AstralSurfaceClose = ModContent.Request<Texture2D>("CalamityMod/Backgrounds/AstralSurfaceClose", AssetRequestMode.ImmediateLoad).Value;
            AstralSurfaceCloseGlow = ModContent.Request<Texture2D>("CalamityMod/Backgrounds/AstralSurfaceCloseGlow", AssetRequestMode.ImmediateLoad).Value;
            AstralSurfaceMiddle = ModContent.Request<Texture2D>("CalamityMod/Backgrounds/AstralSurfaceMiddle", AssetRequestMode.ImmediateLoad).Value;
            AstralSurfaceMiddleGlow = ModContent.Request<Texture2D>("CalamityMod/Backgrounds/AstralSurfaceMiddleGlow", AssetRequestMode.ImmediateLoad).Value;

            //Astral Desert Sky/BG
            AstralDesertSurfaceClose = ModContent.Request<Texture2D>("CalamityMod/Backgrounds/AstralDesertSurfaceClose", AssetRequestMode.ImmediateLoad).Value;
            AstralDesertSurfaceMiddle = ModContent.Request<Texture2D>("CalamityMod/Backgrounds/AstralDesertSurfaceMiddle", AssetRequestMode.ImmediateLoad).Value;

            //Astral Snow Sky/BG
            AstralSnowSurfaceMiddle = ModContent.Request<Texture2D>("CalamityMod/Backgrounds/AstralSnowSurfaceMiddle", AssetRequestMode.ImmediateLoad).Value;

            // Sulpher Sea Sky/BG
            SulphurSeaSky = ModContent.Request<Texture2D>("CalamityMod/Skies/SulphurSeaSky", AssetRequestMode.ImmediateLoad).Value;
            SulphurSeaSkyFront = ModContent.Request<Texture2D>("CalamityMod/Skies/SulphurSeaSkyFront", AssetRequestMode.ImmediateLoad).Value;
            SulphurSeaSurface = ModContent.Request<Texture2D>("CalamityMod/Skies/SulphurSeaSurface", AssetRequestMode.ImmediateLoad).Value;
        }

        public override void Unload()
        {
            AstralSky = null;
            AstralSurfaceFront = null;
            AstralSurfaceFrontGlow = null;
            AstralSurfaceClose = null;
            AstralSurfaceCloseGlow = null;
            AstralSurfaceMiddle = null;
            AstralSurfaceMiddleGlow = null;

            AstralDesertSurfaceClose = null;
            AstralDesertSurfaceMiddle = null;

            AstralSnowSurfaceMiddle = null;

            SulphurSeaSky = null;
            SulphurSeaSkyFront = null;
            SulphurSeaSurface = null;
        }
    }
}
