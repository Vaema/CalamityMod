using CalamityMod.ForegroundDrawing.LoopingTextures;
using CalamityMod.Tiles.Furniture.Monoliths;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Items.Placeables.Furniture.Monoliths
{
    public class OldDukeMonolith : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetDefaults()
        {
            Item.DefaultToPlaceableTile(ModContent.TileType<OldDukeMonolithTile>());
            Item.value = Item.buyPrice(gold: 25); // Sold by Archmage
            Item.rare = ItemRarityID.Pink;
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
}
