using CalamityMod.Items.Placeables.FurnitureMonolith;
using CalamityMod.Tiles.Furniture.Monoliths;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.Furniture.Monoliths;

public class AstralProjector : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<AstralProjectorTile>());
        Item.value = Item.sellPrice(silver: 1);
        Item.rare = ItemRarityID.LightRed;
        Item.accessory = true;
        Item.vanity = true;
    }

    public override void UpdateEquip(Player player)
    {
        if (player.whoAmI == Main.myPlayer)
        {
            player.Calamity().monolithAstralShader = 30;
        }
    }
    public override void UpdateVanity(Player player)
    {
        if (player.whoAmI == Main.myPlayer)
        {
            player.Calamity().monolithAstralShader = 30;
        }
    }
    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient<AstralMonolith>(15).
            AddTile(TileID.Anvils).
            Register();
    }
}
