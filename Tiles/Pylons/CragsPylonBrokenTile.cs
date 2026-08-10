using CalamityMod.Items.Placeables.Pylons;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace CalamityMod.Tiles.Pylons;

public class CragsPylonBrokenTile : ModTile
{
    public override string Texture => "CalamityMod/Tiles/Pylons/CragsPylonTile";

    public override void SetStaticDefaults()
    {
        Main.tileLavaDeath[Type] = false;
        Main.tileFrameImportant[Type] = true;
        TileObjectData.newTile.CopyFrom(TileObjectData.Style3x4);
        TileObjectData.newTile.StyleHorizontal = true;
        TileObjectData.newTile.LavaDeath = false;
        TileObjectData.newTile.LavaPlacement = LiquidPlacement.Allowed;
        TileObjectData.newTile.DrawYOffset = 2;
        TileObjectData.addTile(Type);

        RegisterItemDrop(ModContent.ItemType<CragsPylon>());
        FlexibleTileWand.RubblePlacementLarge.AddVariations(ModContent.ItemType<CragsPylon>(), Type, 0);

        AddMapEntry(Color.OrangeRed, CalamityUtils.GetItemName(ModContent.ItemType<CragsPylon>()));
        TileID.Sets.PreventsSandfall[Type] = true;
    }
    public override bool CreateDust(int i, int j, ref int type) => false;
}
