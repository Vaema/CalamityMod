using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Fishing.BrimstoneCragCatches;

public class Slurpfish : BaseQuestFish
{
    public override bool QuestCondition => NPC.downedBoss3;
    public override LocalizedText Location => CalamityUtils.GetText("Items.Fishing.CaughtInBrimstoneCrag");

    // Must override this since the dialogue uses the player's name
    public override void AnglerQuestChat(ref string description, ref string catchLocation)
    {
        description = this.GetLocalization("QuestDescription").Format(Main.LocalPlayer.name);
        catchLocation = Location.ToString().Replace("'", string.Empty);
    }
}
