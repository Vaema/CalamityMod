using System.Linq;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace CalamityMod.Systems;

internal sealed class GemcornValidGroundSystem : ModSystem
{
    public static TileObjectData GemSaplingData => TileObjectData.GetTileData(TileID.GemSaplings, 0);

    public int[] ValidGemcornGrounds = [];

    public override void PostSetupContent()
    {
        var tiles = CalamityMod.Instance.GetContent<ModTile>()
            .Where(tile => WorldGen.GemTreeGroundTest(tile.Type))
            .Select(tile => (int)tile.Type)
            .ToArray();

        ValidGemcornGrounds = [.. tiles];

        var gemSaplingData = GemSaplingData;
        gemSaplingData.AnchorValidTiles = [.. gemSaplingData.AnchorValidTiles, .. ValidGemcornGrounds];
    }

    public override void Unload()
    {
        var gemSaplingData = GemSaplingData;
        gemSaplingData.AnchorValidTiles = [.. gemSaplingData.AnchorValidTiles.Where(type => !ValidGemcornGrounds.Contains(type))];
    }
}
