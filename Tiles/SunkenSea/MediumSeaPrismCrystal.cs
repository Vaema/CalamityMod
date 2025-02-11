using System;
using CalamityMod.Items.Placeables.SunkenSea;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.DataStructures;
using Terraria.Enums;
using Terraria.ObjectData;
using CalamityMod.Items.Weapons.Melee;

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
            Main.tileSpelunker[Type] = true;

            HitSound = SoundID.Item27;
            DustType = 67;
            MinPick = 55;

            AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTorch);
            AddMapEntry(new Color(48, 201, 214), CalamityUtils.GetItemName<PrismShard>());

            // Attach to ground
            TileObjectData.newTile.CopyFrom(TileObjectData.Style2x2);
            TileObjectData.newTile.StyleHorizontal = true; 
            TileObjectData.newTile.StyleMultiplier = 32; // total 32 frames, all should be same "itemStyle"
            TileObjectData.newTile.StyleWrapLimit = 8; // only 1 placement alternative per row
            TileObjectData.newTile.RandomStyleRange = 8; // 8 different style will be selected upon placing
            TileObjectData.newTile.Origin = new Point16(0, 1);

            // Attach to side (right)
            TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
            TileObjectData.newAlternate.AnchorRight = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
            TileObjectData.newAlternate.AnchorBottom = AnchorData.Empty;
            TileObjectData.newAlternate.Origin = new Point16(1, 0);
            TileObjectData.addAlternate(8);

            // Attach to ceiling
            TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
            TileObjectData.newAlternate.AnchorTop = new AnchorData(AnchorType.SolidTile | AnchorType.SolidBottom, TileObjectData.newTile.Width, 0);
            TileObjectData.newAlternate.AnchorBottom = AnchorData.Empty;
            TileObjectData.newAlternate.Origin = new Point16(0, 0);
            TileObjectData.addAlternate(16);

            // Attach to side (left)
            TileObjectData.newAlternate.CopyFrom(TileObjectData.newTile);
            TileObjectData.newAlternate.AnchorLeft = new AnchorData(AnchorType.SolidTile | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
            TileObjectData.newAlternate.AnchorBottom = AnchorData.Empty;
            TileObjectData.newAlternate.Origin = new Point16(0, 0);
            TileObjectData.addAlternate(24);
            TileObjectData.addTile(Type);
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
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
        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 32, 48, ModContent.ItemType<PrismShard>(), 4);
        }
    }
}
