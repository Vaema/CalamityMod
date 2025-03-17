using CalamityMod.Items.Weapons.Melee;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace CalamityMod.Tiles
{
    public class RoxTile : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileNoAttach[Type] = true;
            Main.tileSpelunker[Type] = true;
            Main.tileLighted[Type] = true;

            //Explicitly declared that its immune to liquids and obsidian
            Main.tileLavaDeath[Type] = false;
            Main.tileWaterDeath[Type] = false;
            Main.tileObsidianKill[Type] = false;

            Main.tileOreFinderPriority[Type] = 910;
            //Astral Ore is 900, the main reason Im using it as a reference is due to being the highest HM ore and since Catalyst generates Astral ore deeper.
            //Wont conflict with Metanova (Catalyst), Uelibloom, or any other higher tier ores - Shade

            TileObjectData.newTile.CopyFrom(TileObjectData.Style3x4);
            TileObjectData.newTile.CoordinateHeights = new int[] { 16, 16, 16, 18 };

            TileObjectData.addTile(Type);

            AddMapEntry(new Color(240, 77, 7), CalamityUtils.GetItemName<Roxcalibur>());
            TileID.Sets.DisableSmartCursor[Type] = true;
            RegisterItemDrop(ModContent.ItemType<Roxcalibur>());
            FlexibleTileWand.RubblePlacementLarge.AddVariations(ModContent.ItemType<Roxcalibur>(), Type, 0);

            DustType = DustID.Lava;
        }
        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            r = 1.64f;
            g = 0.25f;
            b = 1.89f;
        }
        public override void DrawEffects(int i, int j, SpriteBatch spriteBatch, ref TileDrawInfo drawData)
        {
            Tile tile = Main.tile[i, j];
            if (tile.TileFrameY == 18 && tile.TileFrameX < 54)
            {
                CalamityUtils.DrawFlameSparks(DustID.ShadowbeamStaff, 5, i, j);
            }
        }

        public override bool CreateDust(int i, int j, ref int type)
        {
            type = !WorldGen.genRand.NextBool(3) ? DustID.ShadowbeamStaff : DustID.Lava;
            return true;
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 5 : 50;
        }
    }
}
