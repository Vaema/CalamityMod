using CalamityMod.ForegroundDrawing.LoopingTextures;
using CalamityMod.Rarities;
using CalamityMod.Tiles.Furniture.Monoliths;
using Terraria;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.Furniture.Monoliths;

[LegacyName("OldDukeMonolith")]
public class EldenDiorama : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetDefaults()
    {
        Item.DefaultToPlaceableTile(ModContent.TileType<OldDukeMonolithTile>());
        Item.value = Item.sellPrice(gold: 20);
        Item.rare = ModContent.RarityType<PureGreen>();
        Item.accessory = true;
        Item.vanity = true;
    }

    public override void UpdateEquip(Player player)
    {
        if (player.whoAmI == Main.myPlayer)
        {
            player.GetModPlayer<NuclearTorrentPlayer>().ShouldDisplayTorrentMonolith = true;
        }
    }
    public override void UpdateVanity(Player player)
    {
        if (player.whoAmI == Main.myPlayer)
        {
            player.GetModPlayer<NuclearTorrentPlayer>().ShouldDisplayTorrentMonolith = true;
        }
    }
}
