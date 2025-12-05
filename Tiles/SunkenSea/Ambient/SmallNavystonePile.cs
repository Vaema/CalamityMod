using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Microsoft.Xna.Framework;
using Terraria.GameContent;

namespace CalamityMod.Tiles.SunkenSea.Ambient
{
	public class SmallNavystonePileBase : ModTile
	{
        public override string Texture => "CalamityMod/Tiles/SunkenSea/Ambient/SmallNavystonePile";

        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoFail[Type] = true;
            Main.tileObsidianKill[Type] = true;

            DustType = DustID.Stone;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style2x1);
            TileObjectData.newTile.DrawYOffset = 2;
            TileObjectData.addTile(Type);


            AddMapEntry(new Color(0, 62, 84));
            DustType = DustID.BlueMoss;
            HitSound = SoundID.Dig;
        }
    }

    public class SmallNavystonePile : NavystonePileBase
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
