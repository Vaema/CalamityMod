using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Materials;

public class EffulgentFeather : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Materials";
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 25;
        Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(3, 11));
        ItemID.Sets.AnimatesAsSoul[Type] = true;
        ItemID.Sets.SortingPriorityMaterials[Type] = 102;
    }

    public override void SetDefaults()
    {
        Item.width = 24;
        Item.height = 24;
        Item.maxStack = Item.CommonMaxStack;
        Item.value = Item.sellPrice(gold: 1, silver: 30);
        Item.rare = ItemRarityID.Purple;
    }
}
