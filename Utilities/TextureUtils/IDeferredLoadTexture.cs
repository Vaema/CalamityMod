namespace CalamityMod;

public interface IDeferredLoadTexture
{
    bool IsAssetLoaded { get; }
    void OnTextureLoaded();
}
