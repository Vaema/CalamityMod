using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;
using Terraria.DataStructures;
using Terraria.Enums;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace CalamityMod.Tiles.SunkenSea.Ambient
{
	public class WideScarletSeagrass : ModTile
	{
		public override void SetStaticDefaults()
		{
			Main.tileFrameImportant[Type] = true;
			Main.tileSolid[Type] = false;
            Main.tileLighted[Type] = true;
            TileObjectData.newTile.Width = 2;
			TileObjectData.newTile.Height = 2;
			TileObjectData.newTile.Origin = new Point16(2, 2);
			TileObjectData.newTile.CoordinateHeights = new int[] { 16, 16 };
			TileObjectData.newTile.StyleWrapLimit = 36;
			TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
			TileObjectData.newTile.CoordinateWidth = 16;
			TileObjectData.newTile.CoordinatePadding = 2;
			TileObjectData.newTile.DrawYOffset = 3;
			TileObjectData.addTile(Type);
            AddMapEntry(new Color(187, 43, 44));
            DustType = DustID.BlueMoss;
            HitSound = SoundID.Dig;
        }
        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            float brightness = 0.7f;
            float declareThisHereToPreventRunningTheSameCalculationMultipleTimes = Main.GameUpdateCount * 0.01f;
            brightness *= (float)MathF.Sin(-j / 8f + declareThisHereToPreventRunningTheSameCalculationMultipleTimes + i);
            brightness *= (float)MathF.Sin(-i / 8f + declareThisHereToPreventRunningTheSameCalculationMultipleTimes + j);
            brightness += 0.7f;
            brightness = MathHelper.Clamp(brightness, 0.1f, 0.5f);
            r = 187f / 255f;
            g = 43f / 255f;
            b = 44f / 255f;
            r *= brightness;
            g *= brightness;
            b *= brightness;
        }
    }

    public class WideScarletSeagrass2 : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileSolid[Type] = false;
            Main.tileLighted[Type] = true;
            TileObjectData.newTile.Width = 2;
            TileObjectData.newTile.Height = 2;
            TileObjectData.newTile.Origin = new Point16(2, 2);
            TileObjectData.newTile.CoordinateHeights = new int[] { 16, 16 };
            TileObjectData.newTile.StyleWrapLimit = 36;
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinatePadding = 2;
            TileObjectData.newTile.DrawYOffset = 3;
            TileObjectData.addTile(Type);
            AddMapEntry(new Color(187, 43, 44));
            DustType = 96;
            HitSound = SoundID.Dig;
        }
        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            float brightness = 0.7f;
            float declareThisHereToPreventRunningTheSameCalculationMultipleTimes = Main.GameUpdateCount * 0.01f;
            brightness *= (float)MathF.Sin(-j / 8f + declareThisHereToPreventRunningTheSameCalculationMultipleTimes + i);
            brightness *= (float)MathF.Sin(-i / 8f + declareThisHereToPreventRunningTheSameCalculationMultipleTimes + j);
            brightness += 0.7f;
            brightness = MathHelper.Clamp(brightness, 0.1f, 0.5f);
            r = 187f / 255f;
            g = 43f / 255f;
            b = 44f / 255f;
            r *= brightness;
            g *= brightness;
            b *= brightness;
        }
    }
    public class WideScarletSeagrass3 : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileSolid[Type] = false;
            Main.tileLighted[Type] = true;
            TileObjectData.newTile.Width = 2;
            TileObjectData.newTile.Height = 3;
            TileObjectData.newTile.Origin = new Point16(2, 3);
            TileObjectData.newTile.CoordinateHeights = new int[] { 16, 16, 16 };
            TileObjectData.newTile.StyleWrapLimit = 36;
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinatePadding = 2;
            TileObjectData.newTile.DrawYOffset = 3;
            TileObjectData.addTile(Type);
            AddMapEntry(new Color(187, 43, 44));
            DustType = 96;
            HitSound = SoundID.Dig;
        }
        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            float brightness = 0.7f;
            float declareThisHereToPreventRunningTheSameCalculationMultipleTimes = Main.GameUpdateCount * 0.01f;
            brightness *= (float)MathF.Sin(-j / 8f + declareThisHereToPreventRunningTheSameCalculationMultipleTimes + i);
            brightness *= (float)MathF.Sin(-i / 8f + declareThisHereToPreventRunningTheSameCalculationMultipleTimes + j);
            brightness += 0.7f;
            brightness = MathHelper.Clamp(brightness, 0.1f, 0.5f);
            r = 187f / 255f;
            g = 43f / 255f;
            b = 44f / 255f;
            r *= brightness;
            g *= brightness;
            b *= brightness;
        }
    }
    public class WideScarletSeagrass4 : ModTile
    {
        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileSolid[Type] = false;
            Main.tileLighted[Type] = true;
            TileObjectData.newTile.Width = 2;
            TileObjectData.newTile.Height = 3;
            TileObjectData.newTile.Origin = new Point16(2, 3);
            TileObjectData.newTile.CoordinateHeights = new int[] { 16, 16, 16 };
            TileObjectData.newTile.StyleWrapLimit = 36;
            TileObjectData.newTile.AnchorBottom = new AnchorData(AnchorType.SolidTile | AnchorType.SolidWithTop | AnchorType.SolidSide, TileObjectData.newTile.Width, 0);
            TileObjectData.newTile.CoordinateWidth = 16;
            TileObjectData.newTile.CoordinatePadding = 2;
            TileObjectData.newTile.DrawYOffset = 3;
            TileObjectData.addTile(Type);
            AddMapEntry(new Color(187, 43, 44));
            DustType = DustID.BlueMoss;
            HitSound = SoundID.Dig;
        }
        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            float brightness = 0.7f;
            float declareThisHereToPreventRunningTheSameCalculationMultipleTimes = Main.GameUpdateCount * 0.01f;
            brightness *= (float)MathF.Sin(-j / 8f + declareThisHereToPreventRunningTheSameCalculationMultipleTimes + i);
            brightness *= (float)MathF.Sin(-i / 8f + declareThisHereToPreventRunningTheSameCalculationMultipleTimes + j);
            brightness += 0.7f;
            brightness = MathHelper.Clamp(brightness, 0.1f, 0.5f);
            r = 187f / 255f;
            g = 43f / 255f;
            b = 44f / 255f;
            r *= brightness;
            g *= brightness;
            b *= brightness;
        }
    }
}
