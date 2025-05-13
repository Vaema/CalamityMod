using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace CalamityMod.Tiles.SunkenSea.Ambient
{
    public class MediumCoral : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileLavaDeath[Type] = true;
            Main.tileWaterDeath[Type] = false;
            Main.tileNoAttach[Type] = true;
            Main.tileLighted[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
            TileObjectData.newTile.DrawYOffset = 2;
            TileObjectData.addTile(Type);
            AddMapEntry(new Color(233, 132, 58));
            DustType = DustID.Coralstone;

            base.SetStaticDefaults();
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 2;
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            r = 0.654f;
            g = 0.391f;
            b = 0.583f;
        }
    }

    public class MediumCoral2 : MediumCoral
    {
        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            r = 0.15f;
            g = 0.275f;
            b = 0.365f;
        }
    }
    
    public class MediumCoral3 : MediumCoral
    {
        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            r = 0.235f;
            g = 0.431f;
            b = 0.573f;
        }
    }
}
