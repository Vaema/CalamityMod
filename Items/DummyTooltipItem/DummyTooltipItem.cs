using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items
{
    /* EXPLANATION:
    These items are made to facilitate tooltip reading and serve no other functions
    As such these items do not have ANY features beyond their texture
    */
    public abstract class DummyTooltipItem : ModItem, ILocalizedModType
    {
        public override LocalizedText DisplayName => LocalizedText.Empty;
        public override LocalizedText Tooltip => LocalizedText.Empty;

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 0;
            ItemID.Sets.ItemsThatShouldNotBeInInventory[Type] = true;
        }
    }
}
