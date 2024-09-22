using System;
using CalamityMod.Items.Placeables;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ObjectData;

namespace CalamityMod.Tiles.SunkenSea
{
    public class MediumSeaPrismCrystal : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileLighted[Type] = true;
            Main.tileNoFail[Type] = true;
            Main.tileFrameImportant[Type] = true;
            Main.tileObsidianKill[Type] = true;
            AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTorch);
            AddMapEntry(new Color(48, 201, 214), CalamityUtils.GetItemName<PrismShard>());
            HitSound = SoundID.Item27;
            DustType = 67;
            Main.tileSpelunker[Type] = true;
            MinPick = 55;

            // Allow attaching sign to the ground
            TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
            TileObjectData.newTile.StyleHorizontal = false;
            TileObjectData.newTile.Origin = new Point16(0, 0);
            TileObjectData.newTile.CoordinateHeights = new[] { 16, 16 };

            // Allow attaching to a solid object that is to the right of the sign
            TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
            TileObjectData.newAlternate.Origin = new Point16(0, 0);
            TileObjectData.newAlternate.AnchorRight = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
            TileObjectData.newAlternate.AnchorBottom = AnchorData.Empty;
            TileObjectData.addAlternate(1);

            // Allow hanging from ceilings
            TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
            TileObjectData.newAlternate.Origin = new Point16(0, 0);
            TileObjectData.newAlternate.AnchorLeft = AnchorData.Empty;
            TileObjectData.newAlternate.AnchorRight = AnchorData.Empty;
            TileObjectData.newAlternate.AnchorTop = new AnchorData(AnchorType.SolidTile | AnchorType.SolidBottom, TileObjectData.newTile.Width, 0);
            TileObjectData.newAlternate.AnchorBottom = AnchorData.Empty;
            TileObjectData.addAlternate(2);

            // Allow attaching to a solid object that is to the left of the sign
            TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
            TileObjectData.newAlternate.Origin = new Point16(0, 0);
            TileObjectData.newAlternate.AnchorLeft = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
            TileObjectData.newAlternate.AnchorBottom = AnchorData.Empty;
            TileObjectData.addAlternate(3);
            TileObjectData.addTile(Type);
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }
        public override bool CanPlace(int i, int j)
        {
            return TileIsCapable(i, j + 1) || TileIsCapable(i, j - 1) || TileIsCapable(i + 1, j) || TileIsCapable(i - 1, j);
        }
        public static bool TileIsCapable(Tile tile)
        {
            return tile.HasTile && Main.tileSolid[tile.TileType] && !Main.tileSolidTop[tile.TileType] && tile.Slope == 0 && !tile.IsHalfBlock && !tile.IsActuated;
        }
        public static bool TileIsCapable(int i, int j)
        {
            if (WorldGen.InWorld(i, j, 20))
            {
                return TileIsCapable(Main.tile[i, j]);
            }
            else
                return false;
        }
        public bool ModifyFrames(int i, int j, bool randomize = false)
        {
            bool flag = true;
            if (TileIsCapable(i, j + 1)) //checks if below tile is active
            {
                Main.tile[i, j].TileFrameY = 0;
            }
            else if (TileIsCapable(i - 1, j)) //checks if left tile is active
            {
                Main.tile[i, j].TileFrameY = 54;
            }
            else if (TileIsCapable(i + 1, j)) //checks if right tile is active
            {
                Main.tile[i, j].TileFrameY = 36;
            }
            else if (TileIsCapable(i, j - 1)) //checks if above tile is active
            {
                Main.tile[i, j].TileFrameY = 18;
            }
            else
            {
                flag = false;
            }
            if (flag && randomize)
            {
                Main.tile[i, j].TileFrameX = (short)(WorldGen.genRand.Next(18) * 18);
                WorldGen.SquareTileFrame(i, j, true);
                NetMessage.SendTileSquare(-1, i, j, 2, TileChangeType.None);
                //NetMessage.SendData(17, -1, -1, null, 1, i, j, Type);
            }
            return flag;
        }
        public override void PlaceInWorld(int i, int j, Item item)
        {
            ModifyFrames(i, j, true);
        }

    public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            var tile = Main.tile[i, j];
            bool lessLightDueToLowWater = tile.LiquidAmount > 50 && tile.LiquidType == LiquidID.Water;

            //Blue
            Color blue = new Color(67, 187, 204);
            Color darkviolet = new Color(18, 67, 116);
            Color value = Color.Lerp(blue, darkviolet, (MathF.Sin(-j / 80f + Main.GameUpdateCount * 0.017f + i / 40f) + 1f) / 2f);
            Color value1 = Color.Lerp(blue, darkviolet, (MathF.Sin((j - 10) / 50f + Main.GameUpdateCount * 0.064f + -i / 30f) + 1f) / 2f);
            r = lessLightDueToLowWater ? 0.27f : 0.36f;
            g = lessLightDueToLowWater ? 0.405f : 0.54f;
            b = lessLightDueToLowWater ? 0.405f : 0.54f;
            r *= (value.R + value1.R) / 300f;
            g *= (value.G + value1.G) / 300f;
            b *= (value.B + value1.B) / 300f;
        }
        public override bool TileFrame(int i, int j, ref bool resetFrame, ref bool noBreak)
        {
            ModifyFrames(i, j);
            return true;
        }
        public void ModifyFrames(int i, int j)
        {
            Tile tile = Main.tile[i, j];
            int left = i;
            int top = j;
            if (tile.TileFrameX % 36 != 0)
            {
                left--;
            }
            if (tile.TileFrameY % 36 != 0)
            {
                top--;
            }
            //bool flag = true;
            if (TileIsCapable(left, top + 2) && TileIsCapable(left + 1, top + 2)) //checks if below tile is active
            {
                if (Main.tile[left, top].TileFrameY != 0)
                {
                    Main.tile[left, top].TileFrameY = (short)(Main.tile[left, top].TileFrameY % 36);
                    Main.tile[left + 1, top].TileFrameY = (short)(Main.tile[left + 1, top].TileFrameY % 36);
                    Main.tile[left + 1, top + 1].TileFrameY = (short)(Main.tile[left + 1, top + 1].TileFrameY % 36);
                    Main.tile[left, top + 1].TileFrameY = (short)(Main.tile[left, top + 1].TileFrameY % 36);
                }
                //else
                //	flag = false;
            }
            else if (TileIsCapable(left - 1, top) && TileIsCapable(left - 1, top + 1)) //checks if left tile is active
            {
                if (Main.tile[left, top].TileFrameY != 108)
                {
                    Main.tile[left, top].TileFrameY = (short)(Main.tile[left, top].TileFrameY % 36 + 108);
                    Main.tile[left + 1, top].TileFrameY = (short)(Main.tile[left + 1, top].TileFrameY % 36 + 108);
                    Main.tile[left + 1, top + 1].TileFrameY = (short)(Main.tile[left + 1, top + 1].TileFrameY % 36 + 108);
                    Main.tile[left, top + 1].TileFrameY = (short)(Main.tile[left, top + 1].TileFrameY % 36 + 108);
                }
                //else
                //	flag = false;
            }
            else if (TileIsCapable(left + 2, top) && TileIsCapable(left + 2, top + 1)) //checks if right tile is active
            {
                if (Main.tile[left, top].TileFrameY != 36)
                {
                    Main.tile[left, top].TileFrameY = (short)(Main.tile[left, top].TileFrameY % 36 + 36);
                    Main.tile[left + 1, top].TileFrameY = (short)(Main.tile[left + 1, top].TileFrameY % 36 + 36);
                    Main.tile[left + 1, top + 1].TileFrameY = (short)(Main.tile[left + 1, top + 1].TileFrameY % 36 + 36);
                    Main.tile[left, top + 1].TileFrameY = (short)(Main.tile[left, top + 1].TileFrameY % 36 + 36);
                }
                //else
                //	flag = false;
            }
            else if (TileIsCapable(left, top - 1) && TileIsCapable(left + 1, top - 1)) //checks if above tile is active
            {
                if (Main.tile[left, top].TileFrameY != 72)
                {
                    Main.tile[left, top].TileFrameY = (short)(Main.tile[left, top].TileFrameY % 36 + 72);
                    Main.tile[left + 1, top].TileFrameY = (short)(Main.tile[left + 1, top].TileFrameY % 36 + 72);
                    Main.tile[left + 1, top + 1].TileFrameY = (short)(Main.tile[left + 1, top + 1].TileFrameY % 36 + 72);
                    Main.tile[left, top + 1].TileFrameY = (short)(Main.tile[left, top + 1].TileFrameY % 36 + 72);
                }
                //else
                //	flag = false;
            }
            //else
            //	flag = false;
            //if (flag)
            //{
            //WorldGen.TileFrame(i, j, true);
            //NetMessage.SendTileSquare(-1, i, j, 4, TileChangeType.None);
            //NetMessage.SendData(17, -1, -1, null, 1, i, j, Type);
            //}
        }
    }
}
