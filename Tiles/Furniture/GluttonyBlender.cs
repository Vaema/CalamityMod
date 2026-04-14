using CalamityMod.Items.Placeables.Furniture;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace CalamityMod.Tiles.Furniture
{
    public class GluttonyBlenderTile : ModTile
    {
        public override string Texture => "CalamityMod/Tiles/Furniture/GluttonyPlaceholder";

        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileLavaDeath[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style3x3);
            TileObjectData.addTile(Type);
            AddMapEntry(new Color(128, 128, 128), CalamityUtils.GetItemName<GluttonyBlender>());
        }

        public override bool CreateDust(int i, int j, ref int type) => false;

        public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY) => offsetY = 2;

        /*public override void AnimateTile(ref int frame, ref int frameCounter)
        {
            base.AnimateTile(ref frame, ref frameCounter);
        }*/
    }
}
