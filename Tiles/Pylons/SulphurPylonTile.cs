using CalamityMod.BiomeManagers;
using CalamityMod.Items.Placeables.Pylons;
using CalamityMod.Systems;
using CalamityMod.Tiles.BaseTiles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;

namespace CalamityMod.Tiles.Pylons;


public class SulphurPylonTile : BasePylonTile
{
    public override int AssociatedItem => ModContent.ItemType<SulphurPylon>();
    public override Color PylonMapColor => Color.YellowGreen;
    public override Color DustColor => Color.GreenYellow;
    public override Color LightColor => new Color(1f, 0.8f, 0f);

    public override NPCShop.Entry GetNPCShopEntry()
    {
        return new NPCShop.Entry(AssociatedItem, Condition.AnotherTownNPCNearby, CalamityConditions.InSulph);
    }

    public override bool ValidTeleportCheck_BiomeRequirements(TeleportPylonInfo pylonInfo, SceneMetrics sceneData)
    {
        var tilePos = pylonInfo.PositionInTiles.ToPoint();
        var inSpace = tilePos.Y <= Main.worldSurface * 0.35; // Adding InSpace Condition for Pylon Specifically
        var inUnderground = tilePos.Y >= Main.worldSurface; // Underground check is easiest way to filter out Abyss Biome without headache
        var tileCountRequirement = BiomeTileCounterSystem.SulphurTiles >= 300
            && BiomeTileCounterSystem.Layer1Tiles < 200
            && BiomeTileCounterSystem.Layer2Tiles < 200
            && BiomeTileCounterSystem.Layer3Tiles < 200
            && BiomeTileCounterSystem.Layer4Tiles < 200;
        return tileCountRequirement || (SulphurousSeaBiome.IsInBiomePosition(tilePos) && !inSpace && !inUnderground);
    }
}
