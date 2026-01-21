using CalamityMod.NPCs.SunkenSea;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace CalamityMod.Items.Critters
{
    public class AlphaSeaMinnowGoldItem : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Misc";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 5;
        }

        public override void SetDefaults()
        {
            Item.DefaultToCapturedCritter(ModContent.NPCType<AlphaSeaMinnowGold>());
            Item.bait = 50;
            Item.value = Item.sellPrice(gold: 5);
            Item.rare = ItemRarityID.Green;
        }
    }
}
