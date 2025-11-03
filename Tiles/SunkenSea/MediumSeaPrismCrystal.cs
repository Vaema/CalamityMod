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
using Terraria.GameContent;
using CalamityMod.Systems;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;

namespace CalamityMod.Tiles.SunkenSea
{
    public class MediumSeaPrismCrystal : ModTile
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
            Main.tileSpelunker[Type] = true;
            Main.tileShine[Type] = 5600;
            Main.tileShine2[Type] = true;

            HitSound = SoundID.Item27;
            DustType = DustID.IceRod;
            MinPick = 55;

            AddToArray(ref TileID.Sets.RoomNeeds.CountsAsTorch);
            AddMapEntry(new Color(53, 136, 207), CalamityUtils.GetItemName<PrismShard>());

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

            DefaultCrystals = ModContent.Request<Texture2D>("CalamityMod/Tiles/SunkenSea/MediumSeaPrismCrystal_Blue");
            PurpleCrystals = ModContent.Request<Texture2D>("CalamityMod/Tiles/SunkenSea/MediumSeaPrismCrystal_Purple");
            GreenCrystals = ModContent.Request<Texture2D>("CalamityMod/Tiles/SunkenSea/MediumSeaPrismCrystal_Green");
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
        public override void KillMultiTile(int i, int j, int frameX, int frameY)
        {
            Item.NewItem(new EntitySource_TileBreak(i, j), i * 16, j * 16, 32, 48, ModContent.ItemType<PrismShard>(), 4);
        }
    }
}
