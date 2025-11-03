using System;
using CalamityMod.Items.Placeables.SunkenSea;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Tiles.SunkenSea
{
    public class SeaPrismCrystals : ModTile
    {
        private static Asset<Texture2D> DefaultCrystals;
        private static Asset<Texture2D> PurpleCrystals;
        private static Asset<Texture2D> GreenCrystals;

        public override void SetStaticDefaults()
        {
            Main.tileLighted[Type] = true;
            Main.tileNoFail[Type] = true;
            Main.tileFrameImportant[Type] = true;
            Main.tileObsidianKill[Type] = true;
            AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTorch);
            AddMapEntry(new Color(53, 136, 207), CalamityUtils.GetItemName<PrismShard>());
            HitSound = SoundID.Item27;
            DustType = 67;
            Main.tileSpelunker[Type] = true;
            MinPick = 55;

            DefaultCrystals = ModContent.Request<Texture2D>("CalamityMod/Tiles/SunkenSea/SeaPrismCrystals");
            PurpleCrystals = ModContent.Request<Texture2D>("CalamityMod/Tiles/SunkenSea/SeaPrismCrystals_Purple");
            GreenCrystals = ModContent.Request<Texture2D>("CalamityMod/Tiles/SunkenSea/SeaPrismCrystals_Green");
        }

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            float fade1 = GetFade1(i, j);
            float fade2 = GetFade2(i, j);

            Color baseColor = new Color(162, 216, 218); //Blue
            Color glow1 = new Color(171, 113, 215);    //Purple
            Color glow2 = new Color(56, 174, 117);     //Green

            Vector3 blended = baseColor.ToVector3();

            blended = Vector3.Lerp(blended, glow1.ToVector3(), fade1 * 0.5f);
            blended = Vector3.Lerp(blended, glow2.ToVector3(), fade2 * 0.5f);

            float brightness = 0.6f;
            blended *= brightness;

            r = blended.X;
            g = blended.Y;
            b = blended.Z;
        }

        private static float GetFade1(int i, int j) => (MathF.Sin(Main.GlobalTimeWrappedHourly * 0.2f) + 1f) / 2f;

        private static float GetFade2(int i, int j) => (MathF.Sin(Main.GlobalTimeWrappedHourly * 0.1f + i * 0.08f - j * 0.05f) + 1f) / 2f;
        
        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            Tile tile = Framing.GetTileSafely(i, j);
            Vector2 offScreen = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);
            Vector2 position = new Vector2(i * 16, j * 16) - Main.screenPosition + offScreen;

            Rectangle sourceRect = new Rectangle(tile.TileFrameX, tile.TileFrameY, 16, 16);

            spriteBatch.Draw(DefaultCrystals.Value, position, sourceRect, Lighting.GetColor(i, j) * 1.5f);            
            spriteBatch.Draw(PurpleCrystals.Value, position, sourceRect, Lighting.GetColor(i, j) * 1.5f * GetFade1(i, j));
            spriteBatch.Draw(GreenCrystals.Value, position, sourceRect, Lighting.GetColor(i, j) * 1.5f * GetFade2(i, j));
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
