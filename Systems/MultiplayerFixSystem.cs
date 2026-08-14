using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Systems;

internal class MultiplayerFixSystem : ModSystem
{
    private int oldSmooth;

    public override void OnModLoad()
    {
        oldSmooth = Main.multiplayerNPCSmoothingRange;
        Main.multiplayerNPCSmoothingRange = 0;
    }

    public override void OnModUnload() =>
        Main.multiplayerNPCSmoothingRange = oldSmooth;

    public override void PostSetupContent() =>
        Main.multiplayerNPCSmoothingRange = 0;
}
