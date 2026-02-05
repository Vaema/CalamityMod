using CalamityMod.Items.Placeables.SunkenSea;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace CalamityMod.Tiles.Rubblemaker.SunkenSea
{
    public class SeaAnemoneEcho : ModTile
    {
        public override string Texture => "CalamityMod/Tiles/SunkenSea/Ambient/SeaAnemone";
        public override void SetStaticDefaults()
        {
            Main.tileLighted[Type] = true;
            AnimationFrameHeight = 36;
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
            TileObjectData.addTile(Type);
            DustType = DustID.TsunamiInABottle;
            AddMapEntry(new Color(54, 69, 72));
            RegisterItemDrop(ModContent.ItemType<HardenedEutrophicSand>());
            FlexibleTileWand.RubblePlacementMedium.AddVariations(ModContent.ItemType<HardenedEutrophicSand>(), Type, 0);

            base.SetStaticDefaults();
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            r = 0.11f;
            g = 0.29f;
            b = 0.57f;
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }

        public override void AnimateTile(ref int frame, ref int frameCounter)
        {
            frameCounter++;
            if (frameCounter > 12)
            {
                frameCounter = 0;
                frame++;
                if (frame > 5)
                {
                    frame = 0;
                }
            }
        }
    }
}
