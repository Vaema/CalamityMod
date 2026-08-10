using CalamityMod.Items.Materials;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.TreasureBags.MiscGrabBags;

[LegacyName("FleshyGeodeT1")]
public class FleshyGeode : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.TreasureBags";
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 10;
    }

    public override void SetDefaults()
    {
        Item.width = 24;
        Item.height = 24;
        Item.maxStack = Item.CommonMaxStack;
        Item.consumable = true;
        Item.rare = ItemRarityID.Yellow;
    }

    public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup)
    {
        itemGroup = ContentSamples.CreativeHelper.ItemGroup.GoodieBags;
    }

    public override bool CanRightClick() => true;

    public override void ModifyItemLoot(ItemLoot itemLoot)
    {
        // Different drop rates on Normal and Expert, so define normal first, then expert
        // 1-3 bars on Normal, 2-3 bars on Expert
        // 1-2 essences on Normal, 1-3 essences on Expert
        // 33% chance of life alloy on Normal, 50% on Expert
        // 25% chance of core of calamity on Normal, 33% on Expert
        var normalOnly = itemLoot.DefineNormalOnlyDropSet();
        normalOnly.Add(ModContent.ItemType<CryonicBar>(), 1, 1, 3);
        normalOnly.Add(ModContent.ItemType<PerennialBar>(), 1, 1, 3);
        normalOnly.Add(ModContent.ItemType<ScoriaBar>(), 1, 1, 3);
        normalOnly.Add(ModContent.ItemType<EssenceofEleum>(), 1, 1, 2);
        normalOnly.Add(ModContent.ItemType<EssenceofSunlight>(), 1, 1, 2);
        normalOnly.Add(ModContent.ItemType<EssenceofHavoc>(), 1, 1, 2);
        normalOnly.Add(ModContent.ItemType<LifeAlloy>(), 3);
        normalOnly.Add(ModContent.ItemType<CoreofCalamity>(), 4);

        var expertPlus = itemLoot.DefineConditionalDropSet(new Conditions.IsExpert());
        expertPlus.Add(ModContent.ItemType<CryonicBar>(), 1, 2, 3);
        expertPlus.Add(ModContent.ItemType<PerennialBar>(), 1, 2, 3);
        expertPlus.Add(ModContent.ItemType<ScoriaBar>(), 1, 2, 3);
        expertPlus.Add(ModContent.ItemType<EssenceofEleum>(), 1, 1, 3);
        expertPlus.Add(ModContent.ItemType<EssenceofSunlight>(), 1, 1, 3);
        expertPlus.Add(ModContent.ItemType<EssenceofHavoc>(), 1, 1, 3);
        expertPlus.Add(ModContent.ItemType<LifeAlloy>(), 2);
        expertPlus.Add(ModContent.ItemType<CoreofCalamity>(), 3);
    }
}
