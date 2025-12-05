using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;

namespace CalamityMod.Tiles.Ores
{
    public class AuricOre : GlowMaskTile
    {
        public static readonly SoundStyle MineSound = new("CalamityMod/Sounds/Custom/AuricMine", 3);
        public static bool Animate;

        public override string GlowMaskAsset => "CalamityMod/Tiles/Ores/AuricOreGlow";

        public override void SetupStatic()
        {
            AnimationFrameHeight = 90;
            Main.tileLighted[Type] = true;
            Main.tileSolid[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileSpelunker[Type] = true;
            Main.tileOreFinderPriority[Type] = 1000;
            Main.tileShine[Type] = 3500;
            Main.tileShine2[Type] = false;

            CalamityUtils.MergeWithGeneral(Type);

            TileID.Sets.Ore[Type] = true;
            TileID.Sets.OreMergesWithMud[Type] = true;

            DustType = DustID.Pixie;
            AddMapEntry(new Color(255, 200, 0), CreateMapEntryName());
            MineResist = 5f;
            MinPick = 250;
            HitSound = MineSound;

            this.RegisterBlendMergeWith(TileID.Dirt);
            this.RegisterBlendMergeWith(TileID.Stone);
        }

        public override bool CanExplode(int i, int j)
        {
            return false;
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            if (!Animate)
            { return; }
            r = 0.24f;
            g = 0.40f;
            b = 0.47f;
        }
        public override void AnimateTile(ref int frame, ref int frameCounter)
        {
            if (!Animate)
            { return; }
            frameCounter++;
            if (frameCounter > 4)
            {
                frameCounter = 0;
                frame++;
                if (frame > 7)
                {
                    Animate = false;
                    frame = 0;
                }
            }
        }

        public override Color GetGlowMaskColor(int i, int j, TileDrawInfo drawData)
        {
            return Color.White;
        }
    }
}
