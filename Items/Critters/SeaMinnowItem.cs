using CalamityMod.NPCs.SunkenSea;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Critters;

public class SeaMinnowItem : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Misc";
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 5;
    }

    public override void SetDefaults()
    {
        Item.DefaultToCapturedCritter(ModContent.NPCType<SeaMinnow>());
        Item.bait = 20;
        Item.value = Item.sellPrice(silver: 10);
        Item.rare = ItemRarityID.Green;
    }
}
