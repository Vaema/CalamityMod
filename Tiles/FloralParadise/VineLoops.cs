using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace CalamityMod.Tiles.FloralParadise
{
    public class VineLoops : ModTile
    {
        public const int Variants = 3;

        public override void SetStaticDefaults()
        {
            Main.tileCut[Type] = true;
            Main.tileBlockLight[Type] = true;
            Main.tileLavaDeath[Type] = true;
            Main.tileNoFail[Type] = true;
            Main.tileFrameImportant[Type] = true;

            TileObjectData.newTile.CopyFrom(TileObjectData.Style3x2);
            TileObjectData.newTile.AnchorBottom = AnchorData.Empty;
            TileObjectData.newTile.AnchorTop = new(AnchorType.SolidTile, 2, 0);
            TileObjectData.addTile(Type);

            ModTranslation name = CreateMapEntryName();
            AddMapEntry(new Color(47, 75, 32), name);
            HitSound = SoundID.Grass;
            DustType = 2;
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }
    }
}
