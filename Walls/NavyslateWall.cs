using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria.GameContent;
using Terraria;
using Terraria.ModLoader;
namespace CalamityMod.Walls
{
    [LegacyName("EutrophicSandWall")]
    public class NavyslateWall : ModWall, IVisibleThroughWater
    {
        int IVisibleThroughWater.WaterMapEntry { get; set; }

        public override void SetStaticDefaults()
        {
            DustType = 96;
            this.AddMapEntryWithWaterVisibility(new Color(11, 40, 43));
        }
        
        public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;
    }
}
