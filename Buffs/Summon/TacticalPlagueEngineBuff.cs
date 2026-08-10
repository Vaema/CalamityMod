using CalamityMod.Projectiles.Summon;
using Terraria.ModLoader;

namespace CalamityMod.Buffs.Summon;

public class TacticalPlagueEngineBuff : BaseSummonBuff
{
    protected override int MinionProjectileType => ModContent.ProjectileType<TacticalPlagueJet>();

    protected override ref bool MinionBool => ref BuffModdedOwner.plagueEngine;
}
