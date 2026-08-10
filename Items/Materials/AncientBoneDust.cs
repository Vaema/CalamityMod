using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Materials;

[LegacyName("DemonicBoneAsh")]
public class AncientBoneDust : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Materials";
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 5;
    }

    public override void SetDefaults()
    {
        Item.width = 20;
        Item.height = 20;
        Item.maxStack = Item.CommonMaxStack;
        Item.value = Item.sellPrice(copper: 10);
        Item.rare = ItemRarityID.Blue;
    }
}
