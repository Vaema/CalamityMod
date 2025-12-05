using CalamityMod.Items.Placeables.SunkenSea;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Enums;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace CalamityMod.Tiles.Rubblemaker.SunkenSea
{
    public class FanCoralEcho : ModTile
    {
        public override string Texture => "CalamityMod/Tiles/SunkenSea/Ambient/FanCoral";
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileLighted[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style3x3);
            TileObjectData.newTile.Direction = TileObjectDirection.None;
            TileObjectData.addTile(Type);
            DustType = DustID.TsunamiInABottle;
            AddMapEntry(new Color(54, 69, 72));
            RegisterItemDrop(ModContent.ItemType<CyanCoral>());
            FlexibleTileWand.RubblePlacementLarge.AddVariations(ModContent.ItemType<CyanCoral>(), Type, 0);

            base.SetStaticDefaults();
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            r = 0.88f;
            g = 1f;
            b = 1f;
        }
    }
}
