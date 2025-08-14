using CalamityMod.Items.Materials;
using CalamityMod.Rarities;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.Ores
{
    public class AuricOre : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 100;
            ItemID.Sets.SortingPriorityMaterials[Type] = 119;
        }

        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<Tiles.Ores.AuricOre>());
            Item.value = Item.sellPrice(silver: 70);
            Item.rare = ModContent.RarityType<BurnishedAuric>();
        }

        public override void AddRecipes()
        {
            CreateRecipe(30).
                AddIngredient<YharonSoulFragment>().
                AddCondition(Condition.NearShimmer).
                Register();
        }
    }
}
