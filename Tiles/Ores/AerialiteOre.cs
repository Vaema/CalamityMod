using CalamityMod.Systems;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Tiles.Ores
{
    public class AerialiteOre : ModTile
    {
        public static readonly SoundStyle MineSound = new("CalamityMod/Sounds/Custom/MagicalRockMine", 3);
        internal static Texture2D GlowTexture;


        public override void SetStaticDefaults()
        {
            if (!Main.dedServ)
                GlowTexture = ModContent.Request<Texture2D>("CalamityMod/Tiles/Ores/AerialiteOre", AssetRequestMode.ImmediateLoad).Value;
            Main.tileOreFinderPriority[Type] = 450;
            Main.tileBlockLight[Type] = false;
            Main.tileSolid[Type] = true;
            Main.tileLighted[Type] = true;
            Main.tileNoSunLight[Type] = false;

            TileID.Sets.Ore[Type] = true;

            //CalamityUtils.MergeWithGeneral(Type);
            CalamityUtils.SetMerge(Type, TileID.Cloud);
            CalamityUtils.SetMerge(Type, TileID.RainCloud);
            CalamityUtils.SetMerge(Type, TileID.SnowCloud);
            //Main.tileMerge[TileID.Cloud][ModContent.TileType<AerialiteOre>()] = true;

            Main.tileShine[Type] = 3500;
            Main.tileShine2[Type] = false;

            TileID.Sets.ChecksForMerge[Type] = true;
            DustType = 33;
            AddMapEntry(new Color(145, 255, 255), CreateMapEntryName());
            MinPick = 65;
            HitSound = MineSound;
            Main.tileSpelunker[Type] = true;

            this.RegisterBlendMergeWith(TileID.Cloud);
            this.RegisterBlendMergeWith(TileID.RainCloud);
            this.RegisterBlendMergeWith(TileID.SnowCloud);
            this.RegisterBlendMergeWith(TileID.Dirt);
        }
        public override void PostSetDefaults()
        {
            Main.tileNoSunLight[Type] = false;
        }

        int animationFrameWidth = 234;

        public override void NumDust(int i, int j, bool fail, ref int num)
        {
            num = fail ? 1 : 3;
        }

        public override void ModifyLight(int i, int j, ref float r, ref float g, ref float b)
        {
            r = 0.14f;
            g = 0.346f;
            b = 0.42f;
        }
        public override void AnimateIndividualTile(int type, int i, int j, ref int frameXOffset, ref int frameYOffset)
        {
            frameXOffset = animationFrameWidth * TileFramingSystem.GetVariation4x4_012_Low0(i, j);
        }
        public override void PostDraw(int i, int j, SpriteBatch spriteBatch)
        {
            if (GlowTexture is null)
                return;

            var tile = Main.tile[i, j];
            if (tile.IsTileActuallyInvisible())
                return;

            int xPos = tile.TileFrameX;
            int yPos = tile.TileFrameY;
            int xOffset = animationFrameWidth * TileFramingSystem.GetVariation4x4_012_Low0(i, j);
            xPos += xOffset;
            Vector2 zero = Main.drawToScreen ? Vector2.Zero : new Vector2(Main.offScreenRange);
            Vector2 drawOffset = new Vector2(i * 16 - Main.screenPosition.X, j * 16 - Main.screenPosition.Y) + zero;
            Color drawColour = CalamityUtils.ApplyPaint(Main.tile[i, j].TileColor, new Color(100, 100, 100, 50));
            if (!tile.IsHalfBlock && tile.Slope == 0)
            {
                Main.spriteBatch.Draw(GlowTexture, drawOffset, new Rectangle?(new Rectangle(xPos, yPos, 18, 18)), drawColour, 0.0f, Vector2.Zero, 1f, SpriteEffects.None, 0.0f);
            }
            else if (tile.IsHalfBlock)
            {
                Main.spriteBatch.Draw(GlowTexture, drawOffset + new Vector2(0f, 8f), new Rectangle?(new Rectangle(xPos, yPos, 18, 8)), drawColour, 0.0f, Vector2.Zero, 1f, SpriteEffects.None, 0.0f);
            }
        }
    }
}
