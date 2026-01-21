using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Systems
{
    internal sealed class MountsBalancingSystem : ModSystem
    {
        public override void OnModLoad()
        {
            // Mount balancing occurs during runtime and is undone when Calamity is unloaded
            // Buff DCU's pickaxe power to equal PML pickaxe capabilities
            Mount.drillPickPower = 225;
        }

        public override void Unload()
        {
            Mount.drillPickPower = 210;
        }
    }
}
