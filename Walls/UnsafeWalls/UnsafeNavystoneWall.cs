using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using Terraria;
using Terraria.ModLoader;
namespace CalamityMod.Walls.UnsafeWalls
{
    public class UnsafeNavystoneWall : ModWall, IVisibleThroughWater
    {
        public override string Texture => "CalamityMod/Walls/NavystoneWall";
        int IVisibleThroughWater.WaterMapEntry { get; set; }

        public override void SetStaticDefaults()
        {
            DustType = 96;
            this.AddMapEntryWithWaterVisibility(new Color(16, 45, 48));
        }
        
        public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;

        public override bool Drop(int i, int j, ref int type)
        {
            return false;
        }
    }
}
