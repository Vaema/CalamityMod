using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace CalamityMod.Tiles.Rubblemaker.SunkenSea;

public class SmallWideCoralEcho : ModTile
{
    public override string Texture => "CalamityMod/Tiles/SunkenSea/Ambient/SmallWideCoral";

    public override void SetStaticDefaults()
    {
        Main.tileFrameImportant[Type] = true;
        Main.tileLavaDeath[Type] = true;
        Main.tileWaterDeath[Type] = false;
        Main.tileNoAttach[Type] = true;
        Main.tileLighted[Type] = true;
        TileObjectData.newTile.CopyFrom(TileObjectData.Style2x1);
        TileObjectData.newTile.DrawYOffset = 2;
        TileObjectData.addTile(Type);
        AddMapEntry(new Color(137, 154, 71));
        DustType = DustID.Coralstone;
        RegisterItemDrop(ItemID.CoralstoneBlock);
        FlexibleTileWand.RubblePlacementMedium.AddVariations(ItemID.CoralstoneBlock, Type, 0);

        base.SetStaticDefaults();
    }

    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
    {
        r = 0.32f;
        g = 0.37f;
        b = 0.15f;
    }
}

public class SmallWideCoral2Echo : SmallWideCoralEcho
{
    public override string Texture => "CalamityMod/Tiles/SunkenSea/Ambient/SmallWideCoral2";

    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
    {
        r = 0.33f;
        g = 0.2f;
        b = 0.29f;
    }
}
