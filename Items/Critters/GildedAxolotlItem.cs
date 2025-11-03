using CalamityMod.NPCs.Abyss;
using CalamityMod.NPCs.SunkenSea;
using CalamityMod.NPCs.TownNPCs;
using CalamityMod.Rarities;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Critters
{
    public class GildedAxolotlItem : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Misc";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 5;
        }

        public override void SetDefaults()
        {
            Item.DefaultToCapturedCritter(ModContent.NPCType<GildedAxolotl>());
            Item.value = Item.sellPrice(silver: 30);
            Item.rare = ItemRarityID.Green;
        }
    }
}
