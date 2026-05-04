using CalamityMod.Tiles.Furniture;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.Furniture
{
    public class GluttonyBlender : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public const int OneInXChanceForGoodSlop = 5;

        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<GluttonyBlenderTile>());
            Item.rare = ItemRarityID.Orange;
            Item.value = Item.buyPrice(gold: 5);
        }
    }

    public class QualitySlop : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Potions";
        public override string Texture => "CalamityMod/Items/Potions/Food/QualitySlop";

        public override void SetDefaults()
        {
            Item.DefaultToFood(32, 30, BuffID.WellFed3, CalamityUtils.MinutesToFrames(60));
            Item.rare = ItemRarityID.Orange;
        }
    }

    public class DisgustingSlop : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Misc";
        public override string Texture => "CalamityMod/Items/Materials/DisgustingSlop";

        public override void SetStaticDefaults()
        {
            ItemID.Sets.ExtractinatorMode[Type] = ItemID.OldShoe;
        }
        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 36;
            Item.maxStack = Item.CommonMaxStack;
            Item.rare = ItemRarityID.Gray;
            Item.MakeUsableWithChlorophyteExtractinator();
        }
    }
}
