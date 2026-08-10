using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.ModLoader;

namespace CalamityMod.Skies;

[Autoload(Side = ModSide.Client)]
internal sealed class SkyTextureRefs : ModSystem
{
    // Astral Sky/BG
    public static Asset<Texture2D> AstralSky;
    public static Asset<Texture2D> AstralSurfaceFront;
    public static Asset<Texture2D> AstralSurfaceFrontGlow;
    public static Asset<Texture2D> AstralSurfaceClose;
    public static Asset<Texture2D> AstralSurfaceCloseGlow;
    public static Asset<Texture2D> AstralSurfaceMiddle;
    public static Asset<Texture2D> AstralSurfaceMiddleGlow;

    // Astral Desert Sky/BG
    public static Asset<Texture2D> AstralDesertSurfaceClose;
    public static Asset<Texture2D> AstralDesertSurfaceMiddle;

    // Astral Snow Sky/BG
    public static Asset<Texture2D> AstralSnowSurfaceMiddle;

    // Sulphur Sea Sky/BG
    public static Asset<Texture2D> SulphurSeaSky;
    public static Asset<Texture2D> SulphurSeaSkyFront;
    public static Asset<Texture2D> SulphurSeaSurface;

    public override void OnModLoad()
    {
        // Astral Sky/BG
        AstralSky = ModContent.Request<Texture2D>("CalamityMod/Skies/AstralSky");
        AstralSurfaceFront = ModContent.Request<Texture2D>("CalamityMod/Backgrounds/AstralSurfaceFront");
        AstralSurfaceFrontGlow = ModContent.Request<Texture2D>("CalamityMod/Backgrounds/AstralSurfaceFrontGlow");
        AstralSurfaceClose = ModContent.Request<Texture2D>("CalamityMod/Backgrounds/AstralSurfaceClose");
        AstralSurfaceCloseGlow = ModContent.Request<Texture2D>("CalamityMod/Backgrounds/AstralSurfaceCloseGlow");
        AstralSurfaceMiddle = ModContent.Request<Texture2D>("CalamityMod/Backgrounds/AstralSurfaceMiddle");
        AstralSurfaceMiddleGlow = ModContent.Request<Texture2D>("CalamityMod/Backgrounds/AstralSurfaceMiddleGlow");

        //Astral Desert Sky/BG
        AstralDesertSurfaceClose = ModContent.Request<Texture2D>("CalamityMod/Backgrounds/AstralDesertSurfaceClose");
        AstralDesertSurfaceMiddle = ModContent.Request<Texture2D>("CalamityMod/Backgrounds/AstralDesertSurfaceMiddle");

        //Astral Snow Sky/BG
        AstralSnowSurfaceMiddle = ModContent.Request<Texture2D>("CalamityMod/Backgrounds/AstralSnowSurfaceMiddle");

        // Sulpher Sea Sky/BG
        SulphurSeaSky = ModContent.Request<Texture2D>("CalamityMod/Skies/SulphurSeaSky");
        SulphurSeaSkyFront = ModContent.Request<Texture2D>("CalamityMod/Skies/SulphurSeaSkyFront");
        SulphurSeaSurface = ModContent.Request<Texture2D>("CalamityMod/Skies/SulphurSeaSurface");
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
