using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Graphics.Primitives;

public sealed class PrimitiveRenderSystem : ModSystem
{
    public override void Load()
    {
        if (Main.dedServ)
            return;
        Main.QueueMainThreadAction(() =>
        {
            SanePrimitiveRenderer.Initialize();
        });
    }

    public override void Unload()
    {
        SanePrimitiveRenderer.Dispose();
    }
}
