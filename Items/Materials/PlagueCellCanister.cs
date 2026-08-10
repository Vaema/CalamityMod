using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Materials;

[LegacyName("PlagueCellCluster")]
public class PlagueCellCanister : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Materials";
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 25;
    }

    public override void SetDefaults()
    {
        Item.width = 20;
        Item.height = 20;
        Item.maxStack = Item.CommonMaxStack;
        Item.value = Item.sellPrice(silver: 15);
        Item.rare = ItemRarityID.Yellow;
    }
}
