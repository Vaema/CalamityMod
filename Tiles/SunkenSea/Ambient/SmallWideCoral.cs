using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace CalamityMod.Tiles.SunkenSea.Ambient
{
    public class SmallWideCoral : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileLavaDeath[Type] = true;
            Main.tileWaterDeath[Type] = false;
            Main.tileNoAttach[Type] = true;
            Main.tileLighted[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style2x1);
            TileObjectData.newTile.DrawYOffset = 2;
            TileObjectData.addTile(Type);
            AddMapEntry(new Color(137, 154, 71));
            DustType = DustID.Coralstone;

            base.SetStaticDefaults();
        }
        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            r = 161f / 500f;
            g = 183f / 500f;
            b = 73f / 500f;
        }
    }
    public class SmallWideCoral2 : SmallWideCoral
    {
        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            r = 229f / 700f;
            g = 137f / 700f;
            b = 204f / 700f;
        }
    }
}
