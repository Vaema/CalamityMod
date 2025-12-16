using CalamityMod.Items.Materials;
using CalamityMod.Rarities;
using CalamityMod.Tiles.Furniture.CraftingStations;
using CalamityMod.Tiles.Furniture.Monoliths;
using Terraria;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.Furniture.Monoliths
{
    public class BlueDistortedMonolith : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<BlueDistortedMonolithTile>());
            Item.value = Item.sellPrice(gold: 30);
            Item.rare = ModContent.RarityType<CosmicPurple>();
            Item.accessory = true;
            Item.vanity = true;
        }

        public override void UpdateEquip(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                player.Calamity().monolithDevourerBShader = 30;
            }
        }
        public override void UpdateVanity(Player player)
        {
            if (player.whoAmI == Main.myPlayer)
            {
                player.Calamity().monolithDevourerBShader = 30;
            }
        }
        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<CosmiliteBar>(15).
                AddTile<CosmicAnvil>().
                Register();
        }
    }
}
