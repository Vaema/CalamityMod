using CalamityMod.NPCs.NormalNPCs;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Items.Critters;

public class ShroombleItem : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Misc";
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 5;
    }

    public override void SetDefaults()
    {
        Item.DefaultToCapturedCritter(ModContent.NPCType<Shroomble>());
        Item.value = Item.sellPrice(silver: 1);
    }
}
