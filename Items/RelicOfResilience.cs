using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items
{
    public class RelicOfResilience : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Misc";
        public override void SetDefaults()
        {
            Item.width = 40;
            Item.height = 34;
            Item.value = CalamityGlobalItem.RarityPurpleBuyPrice;
            Item.rare = ItemRarityID.Purple;
        }
        public override void HoldItem(Player player)
        {
            player.Calamity().mouseWorldListener = true;
        }

        public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup)
        {
            itemGroup = (ContentSamples.CreativeHelper.ItemGroup)CalamityResearchSorting.ToolsOther;
        }
    }
}
