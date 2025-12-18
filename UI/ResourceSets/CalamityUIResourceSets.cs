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
    public string BasePath => "CalamityMod/UI/ResourceSets/";

    public static CalamityUIResourceSet ChaliceHP { get; private set; }
    public static CalamityUIResourceSet ChaliceBleedHP { get; private set; }
    public static CalamityUIResourceSet MiracleFruitHP { get; private set; }
    public static CalamityUIResourceSet SacredStrawberryHP { get; private set; }
    public static CalamityUIResourceSet SanguineTangerineHP { get; private set; }
    public static CalamityUIResourceSet TaintedCloudberryHP { get; private set; }

    public static CalamityUIResourceSet CometShardMP { get; private set; }
    public static CalamityUIResourceSet EtherealCoreMP { get; private set; }
    public static CalamityUIResourceSet ManaBurnMP { get; private set; }
    public static CalamityUIResourceSet PhantomHeartMP { get; private set; }

    private static CalamityUIResourceSet LoadResourceSet(string path, bool isHP)
    {
        return new(Bar: ModContent.Request<Texture2D>(path + "Bar"), Obj: ModContent.Request<Texture2D>(path + (isHP ? "Heart" : "Star")));
    }

    void ILoadable.Load(Mod mod)
    {
        ChaliceHP = LoadResourceSet(BasePath + "HPChalice", isHP: true);
        ChaliceBleedHP = LoadResourceSet(BasePath + "HPChaliceBleed", isHP: true);
        MiracleFruitHP = LoadResourceSet(BasePath + "HPMiracleFruit", isHP: true);
        SacredStrawberryHP = LoadResourceSet(BasePath + "HPSacredStrawberry", isHP: true);
        SanguineTangerineHP = LoadResourceSet(BasePath + "HPSanguineTangerine", isHP: true);
        TaintedCloudberryHP = LoadResourceSet(BasePath + "HPTaintedCloudberry", isHP: true);

        CometShardMP = LoadResourceSet(BasePath + "MPCometShard", isHP: false);
        EtherealCoreMP = LoadResourceSet(BasePath + "MPEtherealCore", isHP: false);
        ManaBurnMP = LoadResourceSet(BasePath + "MPManaBurn", isHP: false);
        PhantomHeartMP = LoadResourceSet(BasePath + "MPPhantomHeart", isHP: false);
    }

    void ILoadable.Unload()
    {
        ChaliceHP = null;
        ChaliceBleedHP = null;
        MiracleFruitHP = null;
        SacredStrawberryHP = null;
        SanguineTangerineHP = null;
        TaintedCloudberryHP = null;

        CometShardMP = null;
        EtherealCoreMP = null;
        ManaBurnMP = null;
        PhantomHeartMP = null;
    }
}
