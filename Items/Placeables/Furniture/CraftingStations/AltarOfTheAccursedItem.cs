using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables.Crags;
using CalamityMod.Rarities;
using CalamityMod.Tiles.Furniture.CraftingStations;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.Furniture.CraftingStations
{
    [LegacyName("SCalAltarItem")]
    public class AltarOfTheAccursedItem : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<SCalAltarLarge>());
            Item.value = Item.sellPrice(gold: 40);
            Item.rare = ModContent.RarityType<BurnishedAuric>();
        }

        public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup)
        {
            itemGroup = ContentSamples.CreativeHelper.ItemGroup.CraftingObjects;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<BrimstoneSlag>(30).
                AddIngredient<AuricBar>(5).
                AddIngredient<CoreofCalamity>().
                //Not Cosmic Anvil so if people *really* want do do Scal pre-moon lord it is possible.
                AddTile(TileID.MythrilAnvil). 
                Register();
        }
    }
}
