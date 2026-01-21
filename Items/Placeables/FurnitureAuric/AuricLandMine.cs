using CalamityMod.Items.Materials;
using CalamityMod.Tiles.Furniture.CraftingStations;
using CalamityMod.Tiles.FurnitureAuric;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.FurnitureAuric
{
    public class AuricLandMine : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";

        public override void SetStaticDefaults()
        {
            // Did you know Land Mines can be placed on Weapon Racks? Yeah, me neither.
            ItemID.Sets.CanBePlacedOnWeaponRacks[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<AuricLandMineTile>());
            Item.value = Item.sellPrice(gold: 1);
            Item.mech = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe(50).
                AddIngredient(ItemID.LandMine, 50).
                AddIngredient<AuricBar>().
                AddTile<CosmicAnvil>().
                Register();
        }
    }
}
