using CalamityMod.NPCs.Abyss;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Critters;

public class BabyCannonballJellyfishItem : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Misc";
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 5;
    }
    //Intentionally NOT bait due to bestiary entry on them being used as ammunition, although dynamite fishing in terraria does sound pretty fun
    public override void SetDefaults()
    {
        Item.DefaultToCapturedCritter(ModContent.NPCType<BabyCannonballJellyfish>());
        Item.value = Item.sellPrice(silver: 10);
        Item.rare = ItemRarityID.Green;
        Item.damage = 300;
    }
}
