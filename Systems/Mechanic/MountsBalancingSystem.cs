using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalamityMod.CalPlayer;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Systems
{
    internal sealed class MountsBalancingSystem : ModSystem
    {
        public override void Load()
        {
            // Mount balancing occurs during runtime and is undone when Calamity is unloaded.
            Mount.mounts[MountID.Unicorn].dashSpeed *= CalamityPlayer.UnicornSpeedNerfPower;
            Mount.mounts[MountID.Unicorn].runSpeed *= CalamityPlayer.UnicornSpeedNerfPower;

            // Buff DCU's pickaxe power to equal PML pickaxe capabilities
            Mount.drillPickPower = 225;
        }

        public override void Unload()
        {
            Mount.mounts[MountID.Unicorn].dashSpeed /= CalamityPlayer.UnicornSpeedNerfPower;
            Mount.mounts[MountID.Unicorn].runSpeed /= CalamityPlayer.UnicornSpeedNerfPower;

            Mount.drillPickPower = 210;
        }
    }
}
