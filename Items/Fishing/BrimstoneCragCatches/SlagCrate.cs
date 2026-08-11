using CalamityMod.Items.Weapons.Ranged;
using CalamityMod.Tiles.Crags;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Fishing.BrimstoneCragCatches;

public class SlagCrate : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Fishing";
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 5;
        ItemID.Sets.IsFishingCrate[Type] = true;
    }

    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<SlagCrateTile>());
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
        // 20-50 Scorched Bones @ 33.33%
        // This is our equivalent to Bamboo
        itemLoot.Add(ModContent.ItemType<Placeables.Crags.ScorchedBone>(), 3, 20, 50);

        // Slagfire Douser @ 10%
        itemLoot.Add(ModContent.ItemType<SlagfireDouser>(), 10, 1, 1);

        itemLoot.AddBiomeCrateLootRules(false);
    }
}
