using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria.ModLoader;

namespace CalamityMod.UI.ResourceSets;

public record CalamityUIResourceSet(Asset<Texture2D> Bar, Asset<Texture2D> Obj)
{
    public Asset<Texture2D> Heart => Obj;
    public Asset<Texture2D> Star => Obj;
}

public sealed class CalamityUIResourceSets : ILoadable
{
    public static string BasePath => "CalamityMod/UI/ResourceSets/";

    public static CalamityUIResourceSet HPChalice { get; private set; }
    public static CalamityUIResourceSet HPChaliceBleed { get; private set; }
    public static CalamityUIResourceSet HPMiracleFruit { get; private set; }
    public static CalamityUIResourceSet HPSacredStrawberry { get; private set; }
    public static CalamityUIResourceSet HPSanguineTangerine { get; private set; }
    public static CalamityUIResourceSet HPTaintedCloudberry { get; private set; }

    public static CalamityUIResourceSet MPCometShard { get; private set; }
    public static CalamityUIResourceSet MPEtherealCore { get; private set; }
    public static CalamityUIResourceSet MPManaBurn { get; private set; }
    public static CalamityUIResourceSet MPPhantomHeart { get; private set; }

    private static CalamityUIResourceSet LoadResourceSet(string path, bool isHP)
    {
        return new(Bar: ModContent.Request<Texture2D>(BasePath + path + "Bar"), Obj: ModContent.Request<Texture2D>(BasePath + path + (isHP ? "Heart" : "Star")));
    }

    void ILoadable.Load(Mod mod)
    {
        HPChalice = LoadResourceSet(nameof(HPChalice), isHP: true);
        HPChaliceBleed = LoadResourceSet(nameof(HPChaliceBleed), isHP: true);
        HPMiracleFruit = LoadResourceSet(nameof(HPMiracleFruit), isHP: true);
        HPSacredStrawberry = LoadResourceSet(nameof(HPSacredStrawberry), isHP: true);
        HPSanguineTangerine = LoadResourceSet(nameof(HPSanguineTangerine), isHP: true);
        HPTaintedCloudberry = LoadResourceSet(nameof(HPTaintedCloudberry), isHP: true);

        MPCometShard = LoadResourceSet(nameof(MPCometShard), isHP: false);
        MPEtherealCore = LoadResourceSet(nameof(MPEtherealCore), isHP: false);
        MPManaBurn = LoadResourceSet(nameof(MPManaBurn), isHP: false);
        MPPhantomHeart = LoadResourceSet(nameof(MPPhantomHeart), isHP: false);
    }

    void ILoadable.Unload()
    {
        HPChalice = null;
        HPChaliceBleed = null;
        HPMiracleFruit = null;
        HPSacredStrawberry = null;
        HPSanguineTangerine = null;
        HPTaintedCloudberry = null;

        MPCometShard = null;
        MPEtherealCore = null;
        MPManaBurn = null;
        MPPhantomHeart = null;
    }
}
