using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Microsoft.Xna.Framework;
using Terraria.GameContent;

namespace CalamityMod.Tiles.SunkenSea.Ambient
{
	public class NavystonePileBase : ModTile
	{
        public override string Texture => "CalamityMod/Tiles/SunkenSea/Ambient/NavystonePile";

        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoFail[Type] = true;
            Main.tileObsidianKill[Type] = true;

            DustType = DustID.Stone;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
            TileObjectData.newTile.DrawYOffset = 2;
            TileObjectData.addTile(Type);


            AddMapEntry(new Color(0, 62, 84));
            DustType = 96;
            HitSound = SoundID.Dig;
        }
    }

    public class NavystonePile : NavystonePileBase
    {
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();

            TileID.Sets.BreakableWhenPlacing[Type] = true;
            TileID.Sets.ReplaceTileBreakUp[Type] = true;

            TileObjectData.GetTileData(Type, 0).LavaDeath = false;
        }
    }
}
