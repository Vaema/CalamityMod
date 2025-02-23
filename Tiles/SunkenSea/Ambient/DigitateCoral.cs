using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace CalamityMod.Tiles.SunkenSea.Ambient
{
    public class DigitateCoral : ModTile
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
            AddMapEntry(new Color(48, 201, 214));
            DustType = DustID.IceGolem;

            base.SetStaticDefaults();
        }
        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 2;
        }
        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            r = 56f / 400f;
            g = 164f / 400f;
            b = 180f / 400f;
        }
    }

    public class DigitateCoral2 : DigitateCoral
    {
        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            r = 56f / 400f;
            g = 164f / 400f;
            b = 180f / 400f;
        }
    }
    
    public class DigitateCoral3 : DigitateCoral
    {
        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            r = 56f / 400f;
            g = 164f / 400f;
            b = 180f / 400f;
        }
    }
}
