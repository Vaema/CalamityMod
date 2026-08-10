using CalamityMod.Items.Placeables.SunkenSea;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace CalamityMod.Tiles.Rubblemaker.SunkenSea;

public class MediumCoralEcho : ModTile
{
    public override string Texture => "CalamityMod/Tiles/SunkenSea/Ambient/MediumCoral";

    public override void SetStaticDefaults()
    {
        Main.tileFrameImportant[Type] = true;
        Main.tileLavaDeath[Type] = true;
        Main.tileWaterDeath[Type] = false;
        Main.tileNoAttach[Type] = true;
        Main.tileLighted[Type] = true;
        TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
        TileObjectData.newTile.DrawYOffset = 2;
        TileObjectData.addTile(Type);
        AddMapEntry(new Color(233, 132, 58));
        DustType = DustID.Coralstone;
        RegisterItemDrop(ModContent.ItemType<EutrophicSand>());
        FlexibleTileWand.RubblePlacementMedium.AddVariations(ModContent.ItemType<EutrophicSand>(), Type, 0);

        base.SetStaticDefaults();
    }

    public override void NumDust(int i, int j, bool fail, ref int num)
    {
        num = fail ? 1 : 2;
    }

    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
    {
        r = 0.65f;
        g = 0.39f;
        b = 0.58f;
    }
}

public class MediumCoral2Echo : MediumCoralEcho
{
    public override string Texture => "CalamityMod/Tiles/SunkenSea/Ambient/MediumCoral2";

    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
    {
        r = 0.15f;
        g = 0.28f;
        b = 0.37f;
    }
}

public class MediumCoral3Echo : MediumCoralEcho
{
    public override string Texture => "CalamityMod/Tiles/SunkenSea/Ambient/MediumCoral3";

    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
    {
        r = 0.24f;
        g = 0.43f;
        b = 0.57f;
    }
}
