using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria.DataStructures;
using Terraria.Enums;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CalamityMod.Tiles.SunkenSea.Ambient
{
	public class NavystonePile1 : ModTile
	{
		public override void SetStaticDefaults()
		{
			Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
            TileObjectData.addTile(Type);
            AddMapEntry(new Color(0, 62, 84));
            DustType = 96;
            HitSound = SoundID.Dig;
		}
	}

	public class NavystonePile2 : NavystonePile1
	{
	}

	public class NavystonePile3 : NavystonePile1
	{
	}
}