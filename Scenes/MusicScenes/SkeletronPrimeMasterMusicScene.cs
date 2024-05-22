using CalamityMod.World;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Systems
{
    public class SkeletronPrimeMasterMusicScene : ModSceneEffect
    {
        public override int Music => MusicID.Boss3; // I spent 20 minutes trying to get this music to play while forgetting to actually override this in the first place _ YuH
        public override SceneEffectPriority Priority => SceneEffectPriority.BossLow;
        public override bool IsSceneEffectActive(Player player) => Main.masterMode && CalamityWorld.revenge && NPC.AnyNPCs(NPCID.SkeletronPrime) && Main.npc[NPC.FindFirstNPC(NPCID.SkeletronPrime)].Distance(player.Center) <= 525f * 16f;
    }
}
