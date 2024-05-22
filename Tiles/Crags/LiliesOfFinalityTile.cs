using CalamityMod.Items.Weapons.Summon;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace CalamityMod.Tiles.Crags
{
    public class LiliesOfFinalityTile : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileSpelunker[Type] = true;
            Main.tileLighted[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style3x3);
            TileObjectData.newTile.CoordinateHeights = new int[] { 16, 16, 16 };

            TileObjectData.addTile(Type);

            AddMapEntry(Color.White, CalamityUtils.GetItemName<LiliesOfFinality>());
            TileID.Sets.DisableSmartCursor[Type] = true;
            RegisterItemDrop(ModContent.ItemType<LiliesOfFinality>());
            FlexibleTileWand.RubblePlacementLarge.AddVariations(ModContent.ItemType<LiliesOfFinality>(), Type, 0);
            HitSound = new("CalamityMod/Sounds/Custom/LiliesOfFinalityTileHitSound");

            DustType = DustID.GemDiamond;
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b) => r = g = b = 0.5f;

        public override bool CreateDust(int i, int j, ref int type)
        {
            type = !WorldGen.genRand.NextBool(3) ? DustID.SpectreStaff : DustID.GemDiamond;
            return true;
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 5 : 50;
        }
    }
}
