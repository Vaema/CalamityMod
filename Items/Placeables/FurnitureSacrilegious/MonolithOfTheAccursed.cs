using CalamityMod.Rarities;
using CalamityMod.Tiles.Furniture.CraftingStations;
using CalamityMod.Tiles.FurnitureSacrilegious;
using Terraria;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.FurnitureSacrilegious;

public class MonolithOfTheAccursed : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<MonolithOfTheAccursedTile>());
        Item.value = Item.sellPrice(gold: 1);
        Item.rare = ModContent.RarityType<BurnishedAuric>(); 
        Item.accessory = true;
        Item.vanity = true;
    }
    public override void UpdateEquip(Player player)
    {
        if (player.whoAmI == Main.myPlayer)
        {
            player.Calamity().monolithAccursedShader = 30;
        }
    }
    public override void UpdateVanity(Player player)
    {
        if (player.whoAmI == Main.myPlayer)
        {
            player.Calamity().monolithAccursedShader = 30;
        }
    }

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient<OccultBrickItem>(15).
            AddTile<SCalAltar>().
            Register();
    }
}
