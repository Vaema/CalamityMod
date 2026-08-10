using CalamityMod.Items.Placeables.FurnitureExo;
using CalamityMod.Rarities;
using CalamityMod.Tiles.Furniture.CraftingStations;
using CalamityMod.Tiles.Furniture.Monoliths;
using Terraria;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.Furniture.Monoliths;

public class ExoObelisk : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<ExoObeliskTile>());
        Item.value = Item.sellPrice(gold: 1);
        Item.rare = ModContent.RarityType<BurnishedAuric>();
        Item.accessory = true;
        Item.vanity = true;
    }

    public override void UpdateEquip(Player player)
    {
        if (player.whoAmI == Main.myPlayer)
        {
            player.Calamity().monolithExoShader = 30;
        }
    }
    public override void UpdateVanity(Player player)
    {
        if (player.whoAmI == Main.myPlayer)
        {
            player.Calamity().monolithExoShader = 30;
        }
    }
    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient<ExoPlating>(15).
            AddTile<DraedonsForge>().
            Register();
    }
}
