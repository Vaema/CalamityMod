using Terraria;

namespace CalamityMod.NPCs.VanillaNPCAIOverrides;

public class VanillaAIOverrideContext
{
    public NPC NPC { get; init; }
    public int NPCType { get; init; }
    public bool InRevengeanceWorld { get; init; }
    public bool InDeathWorld { get; init; }
    public bool InBossRush { get; init; }
    public VanillaAIOverride OverrideToApply { get; set; }
}
