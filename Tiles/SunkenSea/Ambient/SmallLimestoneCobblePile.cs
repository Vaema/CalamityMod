using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Microsoft.Xna.Framework;

namespace CalamityMod.Tiles.SunkenSea.Ambient;

	public class SmallLimestoneCobblePileBase : ModTile
	{
    public override string Texture => "CalamityMod/Tiles/SunkenSea/Ambient/SmallLimestoneCobblePile";

    public override void SetStaticDefaults()
    {
        Main.tileFrameImportant[Type] = true;
        Main.tileNoFail[Type] = true;
        Main.tileObsidianKill[Type] = true;

        DustType = DustID.Stone;

        TileObjectData.newTile.CopyFrom(TileObjectData.Style2x1);
        TileObjectData.newTile.DrawYOffset = 2;
        TileObjectData.addTile(Type);


        AddMapEntry(new Color(174, 120, 91));
        DustType = DustID.BlueMoss;
        HitSound = SoundID.Dig;
    }
}

public class SmallLimestoneCobblePile : SmallLimestoneCobblePileBase
{
    public override void SetStaticDefaults()
    {
        base.SetStaticDefaults();

        TileID.Sets.BreakableWhenPlacing[Type] = true;
        TileID.Sets.ReplaceTileBreakUp[Type] = true;

        TileObjectData.GetTileData(Type, 0).LavaDeath = false;
    }
}
