using CalamityMod.NPCs.SunkenSea;
using CalamityMod.Packets;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Critters
{
    public class SeaFloatyItem : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Misc";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 5;
        }

        public override void SetDefaults()
        {
            Item.DefaultToCapturedCritter(ModContent.NPCType<SeaFloaty>());
            Item.value = Item.sellPrice(gold: 3); // These things are actually painful to catch, they can get a 300% value boost
            Item.rare = ItemRarityID.Green;
        }
    }
}
