using System;
using CalamityMod.Items.Placeables.SunkenSea;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Tiles.SunkenSea
{
    public class SeaPrismCrystals : ModTile
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
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            var tile = Main.tile[i, j];
            bool lessLightDueToLowWater = tile.LiquidAmount > 50 && tile.LiquidType == LiquidID.Water;
            int xframe = tile.TileFrameX / 18;
            if (xframe > 7 && xframe < 14)
            {
                //Purple
                Color darkviolet = new Color(75, 54, 103);
                Color violet = new Color(231, 205, 245);
                Color value = Color.Lerp(darkviolet, violet, (MathF.Sin(-j / 80f + Main.GameUpdateCount * 0.037f + i / 40f) + 1f) / 2f);
                Color value1 = Color.Lerp(darkviolet, violet, (MathF.Sin((j - 120) / 50f + Main.GameUpdateCount * 0.014f + -i / 30f) + 1f) / 2f);
                r = lessLightDueToLowWater ? 0.36f : 0.48f;
                g = lessLightDueToLowWater ? 0.315f : 0.42f;
                b = lessLightDueToLowWater ? 0.405f : 0.54f;
                r *= (value.R + value1.R) / 300f;
                g *= (value.G + value1.G) / 300f;
                b *= (value.B + value1.B) / 300f;
                
            }
            else if (xframe >= 14)
            {
                //Green
                Color murkeygreen = new Color(28, 68, 54);
                Color lightgreen = new Color(166, 234, 198);
                Color value2 = Color.Lerp(murkeygreen, lightgreen, (MathF.Sin(-j / 80f + Main.GameUpdateCount * 0.027f + i / 40f) + 1f) / 2f);
                Color value3 = Color.Lerp(murkeygreen, lightgreen, (MathF.Sin((j - 80) / 50f + Main.GameUpdateCount * 0.034f + -i / 30f) + 1f) / 2f);
                r = lessLightDueToLowWater ? 0.225f : 0.3f;
                g = lessLightDueToLowWater ? 0.405f : 0.54f;
                b = lessLightDueToLowWater ? 0.315f : 0.42f;
                r *= (value2.R + value3.R) / 300f;
                g *= (value2.G + value3.G) / 300f;
                b *= (value2.B + value3.B) / 300f;
            }
            else
            {
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
        }

        public override bool CanPlace(int i, int j)
        {
            Tile belowTile = Main.tile[i, j + 1];
            Tile aboveTile = Main.tile[i, j - 1];
            Tile rightTile = Main.tile[i + 1, j];
            Tile leftTile = Main.tile[i - 1, j];

            if ((belowTile.Slope == SlopeType.Solid && !belowTile.IsHalfBlock && belowTile.HasTile && belowTile.IsTileSolid()) ||
                (aboveTile.Slope == SlopeType.Solid && !aboveTile.IsHalfBlock && aboveTile.HasTile && aboveTile.IsTileSolid()) ||
                (rightTile.Slope == SlopeType.Solid && !rightTile.IsHalfBlock && rightTile.HasTile && rightTile.IsTileSolid()) ||
                (leftTile.Slope == SlopeType.Solid && !leftTile.IsHalfBlock && leftTile.HasTile && leftTile.IsTileSolid()))
                return true;

            return false;
        }

        public override void PlaceInWorld(int i, int j, Item item)
        {
            Tile belowTile = Main.tile[i, j + 1];
            Tile aboveTile = Main.tile[i, j - 1];
            Tile rightTile = Main.tile[i + 1, j];
            Tile leftTile = Main.tile[i - 1, j];

            if (belowTile.Slope == SlopeType.Solid && !belowTile.IsHalfBlock && belowTile.HasTile && belowTile.IsTileSolid())
                Main.tile[i, j].TileFrameY = 0;
            else if (aboveTile.Slope == SlopeType.Solid && !aboveTile.IsHalfBlock && aboveTile.HasTile && aboveTile.IsTileSolid())
                Main.tile[i, j].TileFrameY = 18;
            else if (rightTile.Slope == SlopeType.Solid && !rightTile.IsHalfBlock && rightTile.HasTile && rightTile.IsTileSolid())
                Main.tile[i, j].TileFrameY = 36;
            else if (leftTile.Slope == SlopeType.Solid && !leftTile.IsHalfBlock && leftTile.HasTile && leftTile.IsTileSolid())
                Main.tile[i, j].TileFrameY = 54;

            Main.tile[i, j].TileFrameX = (short)(WorldGen.genRand.Next(18) * 18);
        }
    }
}
