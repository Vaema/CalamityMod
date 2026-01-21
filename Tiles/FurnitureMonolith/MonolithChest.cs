using CalamityMod.Dusts;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent.ObjectInteractions;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace CalamityMod.Tiles.FurnitureMonolith
{
    public class MonolithChest : ModTile
    {
        public Asset<Texture2D> GlowTexture;

        public override void SetStaticDefaults()
        {
            this.SetUpChest(ModContent.ItemType<Items.Placeables.FurnitureMonolith.MonolithChest>());
            AddMapEntry(new Color(191, 142, 111), CalamityUtils.GetItemName<Items.Placeables.FurnitureMonolith.MonolithChest>(), FurnitureCommon.GetMapChestName);
        }

        public override bool CreateDust(int i, int j, ref int type)
        {
            Dust.NewDust(new Vector2(i, j) * 16f, 16, 16, ModContent.DustType<AstralBasic>(), 0f, 0f, 1, new Color(255, 255, 255), 1f);
            return false;
        }
        public override bool HasSmartInteract(int i, int j, SmartInteractScanSettings settings) => true;
        public override void NumDust(int i, int j, bool fail, ref int num) => num = fail ? 1 : 3;

        public override LocalizedText DefaultContainerName(int frameX, int frameY) => CalamityUtils.GetItemName<Items.Placeables.FurnitureMonolith.MonolithChest>();
        public override void MouseOver(int i, int j) => FurnitureCommon.ChestMouseOver<Items.Placeables.FurnitureMonolith.MonolithChest>(i, j);
        public override void MouseOverFar(int i, int j) => FurnitureCommon.ChestMouseFar<Items.Placeables.FurnitureMonolith.MonolithChest>(i, j);
        public override void KillMultiTile(int i, int j, int frameX, int frameY) => Chest.DestroyChest(i, j);
        public override bool RightClick(int i, int j)
        {
            // Glowmask animation & custom sound
            Player player = Main.LocalPlayer;
            Tile tile = Main.tile[i, j];
            Main.mouseRightRelease = false;
            int left = i;
            int top = j;
            if (tile.TileFrameX % 36 != 0)
            {
                left--;
            }
            if (tile.TileFrameY != 0)
            {
                top--;
            }
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                int chest = Chest.FindChest(left, top);
                if (chest >= 0)
                {
                    if (player.chest < 0)
                    {
                        SoundEngine.PlaySound(SoundID.NPCDeath22 with { Volume = SoundID.NPCDeath22.Volume * 0.5f });
                    }
                }
            }

            return FurnitureCommon.ChestRightClick(i, j);
        }

        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            Tile tile = Main.tile[i, j];
            if (tile.IsTileActuallyInvisible())
                return;

            int xPos = tile.TileFrameX;
            int yPos = tile.TileFrameY;
            int chestIndex = Chest.FindChest(i - (xPos / 18), j - (yPos / 18));
            if (chestIndex == -1)
                return;

            int yOffset = TileObjectData.GetTileData(tile).DrawYOffset;
            GlowTexture ??= ModContent.Request<Texture2D>("CalamityMod/Tiles/FurnitureMonolith/MonolithChestGlow");
            Color drawColour = CalamityUtils.ApplyPaint(Main.tile[i, j].TileColor, new Color(100, 100, 100, 100));
            Vector2 drawOffset = new Vector2(i * 16 - Main.screenPosition.X, j * 16 - Main.screenPosition.Y + yOffset) + (Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange));
            Rectangle frame = new Rectangle(xPos, yPos + Main.chest[chestIndex].frame * 38, 18, 18);
            Main.spriteBatch.Draw(GlowTexture.Value, drawOffset, frame, drawColour, 0.0f, Vector2.Zero, 1f, SpriteEffects.None, 0.0f);
        }
    }
}
