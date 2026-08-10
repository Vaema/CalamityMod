using CalamityMod.Items.Materials;
using CalamityMod.Items.Weapons.Ranged;
using CalamityMod.Tiles.Crags;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Fishing.BrimstoneCragCatches;

public class BrimstoneCrate : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Fishing";
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 5;
        ItemID.Sets.IsFishingCrate[Type] = true;
        ItemID.Sets.IsFishingCrateHardmode[Type] = true;
        ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<SlagCrate>();
    }

    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 32;
        Item.maxStack = Item.CommonMaxStack;
        Item.consumable = true;
        Item.rare = ItemRarityID.Green;
        Item.value = Item.sellPrice(gold: 1);
        Item.createTile = ModContent.TileType<BrimstoneCrateTile>();
        Item.useTurn = true;
        Item.autoReuse = true;
        Item.useAnimation = 15;
        Item.useTime = 10;
        Item.useStyle = ItemUseStyleID.Swing;
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

        // 2-5 Essences of Havoc @ 50%
        // This is our equivalent to Souls of Light/Night
        itemLoot.Add(ModContent.ItemType<EssenceofHavoc>(), 2, 2, 5);

        // Slagfire Douser @ 10%
        itemLoot.Add(ModContent.ItemType<SlagfireDouser>(), 10, 1, 1);

        itemLoot.AddBiomeCrateLootRules();
    }
}
