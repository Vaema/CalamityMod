using CalamityMod.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Fishing.BrimstoneCragCatches
{
    [LegacyName("ChaoticFish")]
    public class Havocfish : BaseQuestFish
    {
        public override bool QuestCondition => Main.hardMode;
        public override LocalizedText Location => CalamityUtils.GetText("Items.Fishing.CaughtInBrimstoneCrag");
    }
}
