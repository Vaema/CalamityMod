using Terraria.ID;
using Terraria.ModLoader;
using WallTiles = CalamityMod.Walls.UnsafeWalls;

namespace CalamityMod.Items.Placeables.Walls;

public class UnsafeNavystoneWall : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Placeables";
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 400;
        ItemID.Sets.DrawUnsafeIndicator[Type] = true;
        ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<Items.Placeables.Walls.NavystoneWall>();
    }

    public override void SetDefaults() => Item.DefaultToPlaceableWall(ModContent.WallType<WallTiles.UnsafeNavystoneWall>());
}
