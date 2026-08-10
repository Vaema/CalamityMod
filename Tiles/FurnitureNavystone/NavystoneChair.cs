using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Tiles.FurnitureNavystone;

public class NavystoneChair : ModTile
{
    public override void SetStaticDefaults() => this.SetUpChair(ModContent.ItemType<Items.Placeables.FurnitureNavystone.NavystoneChair>());

    public override bool CreateDust(int i, int j, ref int type)
    {
        Dust.NewDust(new Vector2(i, j) * 16f, 16, 16, DustID.SnowBlock, 0f, 0f, 1, new Color(54, 69, 72), 1f);
        return false;
    }

    public override void NumDust(int i, int j, bool fail, ref int num)
    {
        num = fail ? 1 : 3;
    }

    public override void ModifySittingTargetInfo(int i, int j, ref TileRestingInfo info) => FurnitureCommon.ChairSitInfo(i, j, ref info);

    public override bool RightClick(int i, int j) => FurnitureCommon.ChairRightClick(i, j);

    public override void MouseOver(int i, int j) => FurnitureCommon.ChairMouseOver(i, j, ModContent.ItemType<Items.Placeables.FurnitureNavystone.NavystoneChair>());

    public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings)
    {
        return settings.player.IsWithinSnappngRangeToTile(i, j, PlayerSittingHelper.ChairSittingMaxDistance);
    }
}
