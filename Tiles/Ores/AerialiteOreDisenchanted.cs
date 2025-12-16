using CalamityMod.Systems;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Tiles.Ores
{
    public class AerialiteOreDisenchanted : ModTile
    {
        private const int AnimationFrameWidth = 234;

        public override void SetStaticDefaults()
        {
            Main.tileBlockLight[Type] = true;
            Main.tileSolid[Type] = true;
            Main.tileLighted[Type] = true;
            //Main.tileNoSunLight[Type] = false;

            TileID.Sets.Ore[Type] = true;

            CalamityUtils.MergeWithGeneral(Type);
            CalamityUtils.SetMerge(Type, ModContent.TileType<AerialiteOre>());
            CalamityUtils.SetMerge(Type, TileID.Cloud);
            CalamityUtils.SetMerge(Type, TileID.RainCloud);
            CalamityUtils.SetMerge(Type, TileID.SnowCloud);
            //Main.tileMerge[TileID.Cloud][ModContent.TileType<AerialiteOre>()] = true;

            //Main.tileShine[Type] = 3500;
            Main.tileShine2[Type] = false;

            TileID.Sets.ChecksForMerge[Type] = true;
            DustType = DustID.Water;
            AddMapEntry(new Color(204, 170, 81), CreateMapEntryName());
            MinPick = 110;
            HitSound = SoundID.Tink;
            Main.tileSpelunker[Type] = true;

            this.RegisterBlendMergeWith(TileID.Cloud);
            this.RegisterBlendMergeWith(TileID.RainCloud);
            this.RegisterBlendMergeWith(TileID.SnowCloud);
            this.RegisterBlendMergeWith(TileID.Dirt);
        }
        public override void PostSetDefaults()
        {
            Main.tileNoSunLight[Type] = false;
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }

        public override bool CanExplode(int i, int j)
        {
            return false;
        }

        public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset)
        {
            frameXOffset = AnimationFrameWidth * TileFramingSystem.GetVariation4x4_012_Low0(i, j);
        }
    }
}
