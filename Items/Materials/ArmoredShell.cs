using CalamityMod.Rarities;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Materials;

public class ArmoredShell : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Materials";
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 5;
        ItemID.Sets.SortingPriorityMaterials[Type] = 107;
    }

    public override void SetDefaults()
    {
        Item.width = 46;
        Item.height = 34;
        Item.maxStack = Item.CommonMaxStack;
        Item.value = Item.sellPrice(gold: 1, silver: 40);
        Item.rare = ModContent.RarityType<Turquoise>();
    }
}
