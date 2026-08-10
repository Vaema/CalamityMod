using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Fishing.BrimstoneCragCatches;

public class CragBullhead : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Fishing";
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 3;
        ItemID.Sets.CanBePlacedOnWeaponRacks[Type] = true;
    }

    public override void SetDefaults()
    {
        Item.width = 32;
        Item.height = 36;
        Item.maxStack = Item.CommonMaxStack;
        Item.value = Item.sellPrice(silver: 7, copper: 50);
        Item.rare = ItemRarityID.Blue;
    }

    public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup)
    {
        itemGroup = ContentSamples.CreativeHelper.ItemGroup.Fish;
    }
}
