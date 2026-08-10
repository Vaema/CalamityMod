using CalamityMod.Tiles.Furniture.Monoliths;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.Furniture.Monoliths;

public class DeepSeaAnchor : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<DeepSeaAnchorTile>());
        Item.value = Item.sellPrice(gold: 8);
        Item.rare = ItemRarityID.Lime;
        Item.accessory = true;
        Item.vanity = true;
    }

    public override void UpdateEquip(Player player)
    {
        if (player.whoAmI == Main.myPlayer)
        {
            player.Calamity().monolithLeviathanShader = 30;
        }
    }
    public override void UpdateVanity(Player player)
    {
        if (player.whoAmI == Main.myPlayer)
        {
            player.Calamity().monolithLeviathanShader = 30;
        }
    }
}
