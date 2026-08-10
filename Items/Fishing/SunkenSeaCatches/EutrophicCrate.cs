using CalamityMod.Items.Placeables.SunkenSea;
using CalamityMod.Tiles.SunkenSea;
using Terraria;
using Terraria.GameContent.ItemDropRules;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Fishing.SunkenSeaCatches;

[LegacyName("SunkenCrate")]
public class EutrophicCrate : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Fishing";
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 5;
        ItemID.Sets.IsFishingCrate[Type] = true;
    }

    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<EutrophicCrateTile>());
        Item.width = Item.height = 32;
        Item.value = Item.sellPrice(gold: 1);
        Item.rare = ItemRarityID.Green;
    }

    public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup)
    {
        itemGroup = ContentSamples.CreativeHelper.ItemGroup.Crates;
    }

    public override bool CanRightClick() => true;
    public override void ModifyItemLoot(ItemLoot itemLoot)
    {
        // 20-50 Blocks @ 100%; Individually 33.33%
        itemLoot.Add(new OneFromRulesRule(1, new IItemDropRule[3]
        {
            ItemDropRule.NotScalingWithLuck(ModContent.ItemType<Items.Placeables.SunkenSea.Navystone>(), 1, 20, 50),
            ItemDropRule.NotScalingWithLuck(ModContent.ItemType<Items.Placeables.SunkenSea.EutrophicSand>(), 1, 20, 50),
            ItemDropRule.NotScalingWithLuck(ModContent.ItemType<Items.Placeables.FurnitureDriftwood.Driftwood>(), 1, 20, 50)
        }));

        // 10-20 Coral Blocks @ 100%; Individually 20%
        itemLoot.Add(new OneFromRulesRule(1, new IItemDropRule[5]
        {
            ItemDropRule.NotScalingWithLuck(ModContent.ItemType<Items.Placeables.SunkenSea.CyanCoral>(), 1, 10, 20),
            ItemDropRule.NotScalingWithLuck(ModContent.ItemType<Items.Placeables.SunkenSea.OrangeCoral>(), 1, 10, 20),
            ItemDropRule.NotScalingWithLuck(ModContent.ItemType<Items.Placeables.SunkenSea.LimeCoral>(), 1, 10, 20),
            ItemDropRule.NotScalingWithLuck(ModContent.ItemType<Items.Placeables.SunkenSea.MagentaCoral>(), 1, 10, 20),
            ItemDropRule.NotScalingWithLuck(ModContent.ItemType<Items.Placeables.SunkenSea.YellowCoral>(), 1, 10, 20)
        }));

        // 4-10 Prism Shards @ 50%
        // This is our equivalent to Crystal Shards/Ichor
        itemLoot.Add(ModContent.ItemType<PrismShard>(), 2, 4, 10);

        itemLoot.AddBiomeCrateLootRules(false);
    }
}
