using CalamityMod.NPCs.SunkenSea;
using Terraria;
using Terraria.ModLoader;
using Terraria.ID;

namespace CalamityMod.Items.Critters
{
    public class ShoreskipperItem : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Misc";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 5;
        }

        public override void SetDefaults()
        {
            Item.DefaultToCapturedCritter(ModContent.NPCType<Shoreskipper>());
            Item.value = Item.sellPrice(silver: 5);
            Item.rare = ItemRarityID.Green;
        }
    }
}
