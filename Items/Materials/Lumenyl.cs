using CalamityMod.Tiles.Abyss;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Materials
{
    [LegacyName("Lumenite")]
    public class Lumenyl : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Materials";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 25;
        }

        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<LumenylCrystals>());
            Item.value = Item.sellPrice(silver: 12);
            Item.rare = ItemRarityID.Lime;
        }
    }
}
