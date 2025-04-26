using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalamityMod.Projectiles.Magic;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Summon
{
    public class FleshTotemBuff : BaseSummonBuff 
    {
        protected override int MinionProjectileType => ModContent.ProjectileType<FleshTotemMinion>();

        protected override ref bool MinionBool => ref BuffModdedOwner.fleshTotem;
    }
}
