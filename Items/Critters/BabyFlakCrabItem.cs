using CalamityMod.NPCs.AcidRain;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Critters;

public class BabyFlakCrabItem : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Misc";
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 5;
    }

    public override void SetDefaults()
    {
        Item.DefaultToCapturedCritter(ModContent.NPCType<BabyFlakCrab>());
        Item.value = Item.sellPrice(silver: 5);
        Item.rare = ItemRarityID.White;
    }
}
