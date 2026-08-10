using CalamityMod.Items.Materials;
using CalamityMod.Tiles.Furniture.Monoliths;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.Furniture.Monoliths;

public class PlagueHumidifier : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<PlagueHumidifierTile>());
        Item.value = Item.sellPrice(gold: 12);
        Item.rare = ItemRarityID.Yellow;
        Item.accessory = true;
        Item.vanity = true;
    }

    public override void UpdateEquip(Player player)
    {
        if (player.whoAmI == Main.myPlayer)
        {
            player.Calamity().monolithPlagueShader = 30;
        }
    }
    public override void UpdateVanity(Player player)
    {
        if (player.whoAmI == Main.myPlayer)
        {
            player.Calamity().monolithPlagueShader = 30;
        }
    }
    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient<InfectedArmorPlating>(15).
            AddTile(TileID.MythrilAnvil).
            Register();
    }
}
