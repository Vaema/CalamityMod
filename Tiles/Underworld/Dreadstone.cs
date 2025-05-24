using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalamityMod.Sounds;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Tiles.Underworld
{
    public class Dreadstone : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileBrick[Type] = true;

            CalamityUtils.MergeWithGeneral(Type);
            CalamityUtils.MergeWithHell(Type);

            TileID.Sets.ChecksForMerge[Type] = true;
            HitSound = SoundID.Tink;
            MineResist = 2f;
            MinPick = 65;
            AddMapEntry(new Color(102, 65, 65));

            this.RegisterUniversalMerge(TileID.Dirt, "CalamityMod/Tiles/Merges/AshMerge");
        }

        public override bool CreateDust(int i, int j, ref int type)
        {
            Dust.NewDust(new Vector2(i, j) * 16f, 16, 16, Main.rand.NextBool(4) ? DustID.Torch : DustID.RichMahogany, 0f, 0f, 1, default, 1f);
            return false;
        }
    }
}
