using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Materials
{
    [LegacyName("Stardust")]
    public class StarblightSoot : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Materials";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 100;
        }

        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 26;
            Item.maxStack = 9999;
            Item.value = Item.sellPrice(silver: 3);
            Item.rare = ItemRarityID.LightRed;
        }
    }
}
