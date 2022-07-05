using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace CalamityMod.Tiles.FloralParadise
{
    public class TallFloralPlants : ModTile
    {
        public const int Variants = 12;

        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoFail[Type] = true;
            Main.tileObsidianKill[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style2x1);
            TileObjectData.addTile(Type);

            HitSound = SoundID.Grass;
            DustType = 2;
            AddMapEntry(new Color(106, 135, 69));
        }

        public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY)
        {
            offsetY = 2;
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 4;
        }

        public override void NearbyEffects(int i, int j, bool closer)
        {
            if (CalamityUtils.ParanoidTileRetrieval(i, j).LiquidAmount > 64)
                WorldGen.KillTile(i, j);
        }
    }
}
