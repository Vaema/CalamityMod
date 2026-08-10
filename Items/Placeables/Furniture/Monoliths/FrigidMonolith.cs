using CalamityMod.Tiles.Furniture.Monoliths;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.Furniture.Monoliths;

public class FrigidMonolith : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<FrigidMonolithTile>());
        Item.value = Item.buyPrice(gold: 25); // Sold by Archmage
        Item.rare = ItemRarityID.Pink;
        Item.accessory = true;
        Item.vanity = true;
    }

    public override void UpdateEquip(Player player)
    {
        if (player.whoAmI == Main.myPlayer)
        {
            player.Calamity().monolithCryogenShader = 30;
        }
    }
    public override void UpdateVanity(Player player)
    {
        if (player.whoAmI == Main.myPlayer)
        {
            player.Calamity().monolithCryogenShader = 30;
        }
    }
}
