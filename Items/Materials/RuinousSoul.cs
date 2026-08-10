using CalamityMod.Rarities;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Materials;

public class RuinousSoul : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Materials";
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 25;
        Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(6, 6));
        ItemID.Sets.AnimatesAsSoul[Type] = true;
        ItemID.Sets.SortingPriorityMaterials[Type] = 111;
    }

    public override void SetDefaults()
    {
        Item.width = 26;
        Item.height = 42;
        Item.maxStack = Item.CommonMaxStack;
        Item.value = Item.sellPrice(gold: 1, silver: 50);
        Item.rare = ModContent.RarityType<PureGreen>();
    }
}
