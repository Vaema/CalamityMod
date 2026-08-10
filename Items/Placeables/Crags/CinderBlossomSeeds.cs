using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables.Crags;

public class CinderBlossomSeeds : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 25;
    }

    public override void SetDefaults()
    {
        Item.width = 16;
        Item.height = 16;
        Item.useTurn = true;
        Item.autoReuse = true;
        Item.consumable = true;
        Item.useTime = 10;
        Item.useAnimation = 15;
        Item.useStyle = ItemUseStyleID.Swing;
        Item.maxStack = Item.CommonMaxStack;

        Item.value = Item.buyPrice(silver: 1, copper: 50); // Sold by Dryad; equal to Ash Grass Seeds
    }

    public override bool? UseItem(Player player) => true;

    public override bool ConsumeItem(Player player)
    {
        var tileX = Player.tileTargetX;
        var tileY = Player.tileTargetY;
        var tile = Framing.GetTileSafely(tileX, tileY);
        var tileAbove = Framing.GetTileSafely(tileX, tileY - 1);

        if (tile.HasTile && !tileAbove.HasTile && tileAbove.LiquidAmount == 0 && tile.TileType == ModContent.TileType<Tiles.Crags.ScorchedRemains>() && player.IsInTileInteractionRange(Player.tileTargetX, Player.tileTargetY, TileReachCheckSettings.Simple))
        {
            tile.TileType = (ushort)ModContent.TileType<Tiles.Crags.ScorchedRemainsGrass>();
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                NetMessage.SendTileSquare(player.whoAmI, tileX, tileY);
            }
            SoundEngine.PlaySound(SoundID.Dig, player.Center);
            return true;
        }

        return false;
    }
}
