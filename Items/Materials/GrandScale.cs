using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Materials;

public class GrandScale : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Materials";
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 5;
    }

    public override void SetDefaults()
    {
        Item.width = 28;
        Item.height = 32;
        Item.maxStack = Item.CommonMaxStack;
        Item.value = Item.sellPrice(gold: 1);
        Item.rare = ItemRarityID.Lime;
    }
}
