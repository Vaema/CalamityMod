using CalamityMod.Items.Materials;
using CalamityMod.Rarities;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Tools
{
    public class BrimstoneLocus : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Tools";
        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.rare = ModContent.RarityType<CalamityRed>();
            Item.value = 0;
        }

        public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup)
        {
            itemGroup = (ContentSamples.CreativeHelper.ItemGroup)CalamityResearchSorting.ToolsOther;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<AshesofAnnihilation>().
                AddIngredient<BloodstoneCore>(3).
                Register();
        }
    }
}
