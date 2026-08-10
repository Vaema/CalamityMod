using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace CalamityMod.Tiles.SunkenSea.Ambient;

public class SmallWideCoral : ModTile
{
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

        base.SetStaticDefaults();
    }
    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
    {
        r = 0.322f;
        g = 0.366f;
        b = 0.146f;
    }
}

public class SmallWideCoral2 : SmallWideCoral
{
    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
    {
        r = 0.327f;
        g = 0.196f;
        b = 0.291f;
    }
}
