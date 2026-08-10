using System;
using CalamityMod.NPCs;
using CalamityMod.NPCs.VanillaNPCAIOverrides.Bosses.BrainOfCthulhu;
using CalamityMod.World;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static CalamityMod.NPCs.VanillaNPCAIOverrides.Bosses.BrainOfCthulhu.BrainOfCthulhuAI;

namespace CalamityMod.Scenes.MusicScenes;

public class RevBoCIntroMusicScene : ModSceneEffect
{
    public override bool IsSceneEffectActive(Player player)
    {
        if(!CalamityWorld.revenge)
            return false;

        if (NPC.crimsonBoss == -1)
            return false;

        NPC brain = Main.npc[NPC.crimsonBoss];

        if ((BrainAIState)brain.ai[0] > BrainAIState.SurfaceSpawnAnimation) //BoC isn't doing its spawn animation
            return false;

        if (!brain.TryGetAIOverride<BrainOfCthulhuAI>(out var revBrain))
            return false;

        // BoC has sent out all its Creepers
        // The second part leaves one frame at the end of the animation where the boss music starts playing, so that it can be instantly maxed out
        return revBrain.SpawnTime != 0 && (revBrain.Time - Math.Abs(revBrain.SpawnTime) < 420);
    }

    public override int Music => MusicLoader.GetMusicSlot(Mod, "Sounds/Music/Silence");

    public override SceneEffectPriority Priority => SceneEffectPriority.BossMedium;
}

public class RevBoCPreIntroMusicScene : ModSceneEffect
{
    public override bool IsSceneEffectActive(Player player)
    {
        if (!CalamityWorld.revenge)
            return false;

        if (NPC.crimsonBoss == -1)
            return false;

        NPC brain = Main.npc[NPC.crimsonBoss];

        if ((BrainAIState)brain.ai[0] > BrainAIState.SurfaceSpawnAnimation) //BoC isn't doing its spawn animation
            return false;

        if (!brain.TryGetAIOverride<BrainOfCthulhuAI>(out var revBrain))
            return false;

        // BoC hasnt sent out all its Creepers yet
        return revBrain.SpawnTime == 0;
    }

    public override int Music => BrainOfCthulhuSystem.PreviousMusic >= 0 ? BrainOfCthulhuSystem.PreviousMusic : MusicID.Crimson; //Keeps the music that was playing prior to BoC being spawned

    public override SceneEffectPriority Priority => SceneEffectPriority.BossMedium;
}
