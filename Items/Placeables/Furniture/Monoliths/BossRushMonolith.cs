using CalamityMod.Rarities;
using CalamityMod.Tiles.Furniture.Monoliths;
using Terraria;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.Furniture.Monoliths;

public class BossRushMonolith : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<BossRushMonolithTile>());
        Item.value = Item.sellPrice(platinum: 1); // No longer a mere rock
        Item.rare = ModContent.RarityType<HotPink>();
        Item.accessory = true;
        Item.vanity = true;
    }

    public override void UpdateEquip(Player player)
    {
        if (player.whoAmI == Main.myPlayer)
        {
            player.Calamity().monolithBossRushShader = 30;
        }
    }
    public override void UpdateVanity(Player player)
    {
        if (player.whoAmI == Main.myPlayer)
        {
            player.Calamity().monolithBossRushShader = 30;
        }
    }
}
