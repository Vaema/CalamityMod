using CalamityMod.Items.Placeables.FurnitureWulfrum.FurnitureAnodizedWulfrum;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Tiles.FurnitureWulfrum.FurnitureAnodizedWulfrum;

public class AnodizedWulfrumDoorOpen : ModTile
{
    public override void SetStaticDefaults()
    {
        this.SetUpDoorOpen(ModContent.ItemType<AnodizedWulfrumDoor>(), true);
        TileID.Sets.CloseDoorID[Type] = ModContent.TileType<AnodizedWulfrumDoorClosed>();
    }

    public override bool CreateDust(int i, int j, ref int type)
    {
        Dust.NewDust(new Vector2(i, j) * 16f, 16, 16, DustID.Tungsten, 0f, 0f, 1, new Color(255, 255, 255), 1f);
        return false;
    }

    public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => true;

    public override void NumDust(int i, int j, bool fail, ref int num)
    {
        num = fail ? 1 : 3;
    }

    public override void MouseOver(int i, int j)
    {
        Player player = Main.LocalPlayer;
        player.noThrow = 2;
        player.cursorItemIconEnabled = true;
        player.cursorItemIconID = ModContent.ItemType<AnodizedWulfrumDoor>();
    }
}
