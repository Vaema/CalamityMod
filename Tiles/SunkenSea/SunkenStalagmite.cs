using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace CalamityMod.Tiles.SunkenSea
{
    [LegacyName("SunkenStalagmites")]
    public class SunkenStalagmite1 : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style1x2);
            TileObjectData.addTile(Type);
            DustType = 253;
            AddMapEntry(new Color(31, 92, 114));

            base.SetStaticDefaults();
        }
    }

    public class SunkenStalagmite2 : SunkenStalagmite1
    {
    }

    public class SunkenStalagmite3 : SunkenStalagmite1
    {
    }
}
