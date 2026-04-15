using CalamityMod.Items.Placeables.Furniture;
using CalamityMod.Tiles.DraedonStructures;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ObjectData;

namespace CalamityMod.Tiles.Furniture
{
    public class GluttonyBlenderTile : ModTile
    {
        public const int Width = 3;
        public const int Height = 3;

        public override string Texture => "CalamityMod/Tiles/Furniture/GluttonyPlaceholder";

        public override void SetStaticDefaults()
        {
            Main.tileFrameImportant[Type] = true;
            Main.tileLavaDeath[Type] = true;
            TileObjectData.newTile.CopyFrom(TileObjectData.Style3x3);
            ModTileEntity entity = ModContent.GetInstance<GluttonyBlenderTE>();
            TileObjectData.newTile.HookPostPlaceMyPlayer = new PlacementHook(entity.Hook_AfterPlacement, -1, 0, true);
            TileObjectData.addTile(Type);

            AddMapEntry(new Color(128, 128, 128), CalamityUtils.GetItemName<GluttonyBlender>());
        }

        public override bool CreateDust(int i, int j, ref int type) => false;

        public override void SetDrawPositions(int i, int j, ref int width, ref int offsetY, ref int height, ref short tileFrameX, ref short tileFrameY) => offsetY = 2;

        /*public override void AnimateTile(ref int frame, ref int frameCounter)
        {
            base.AnimateTile(ref frame, ref frameCounter);
        }*/

        public override void MouseOver(int i, int j)
        {
            Player p = Main.LocalPlayer;
            // You shouldn't be able to put the slop back into the blender
            if (BuffID.Sets.IsWellFed[p.HeldItem.buffType] && p.HeldItem.type != ModContent.ItemType<DeliciousSlop>())
            {
                p.noThrow = 2;
                p.cursorItemIconEnabled = true;
                p.cursorItemIconID = p.HeldItem.type;
            }
        }
    }

    public class GluttonyBlenderTE : ModTileEntity
    {
        public Vector2 BlenderTop => Position.ToWorldCoordinates(8 * GluttonyBlenderTile.Width, 0f);

        public override bool IsTileValidForEntity(int x, int y)
        {
            Tile tile = Main.tile[x, y];
            return tile.HasTile && tile.TileType == ModContent.TileType<GluttonyBlenderTile>() && tile.TileFrameX == 0 && tile.TileFrameY == 0;
        }

        public override int Hook_AfterPlacement(int i, int j, int type, int style, int direction, int alternate)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient)
            {
                NetMessage.SendTileSquare(Main.myPlayer, i, j, GluttonyBlenderTile.Width, GluttonyBlenderTile.Height);
                NetMessage.SendData(MessageID.TileEntityPlacement, -1, -1, null, i, j, Type);
                return -1;
            }
            return Place(i, j);
        }
    }

    public class GluttonyBlenderGlobalItem : GlobalItem
    {
        public override bool InstancePerEntity => true;

        internal bool FromGluttonyBlender = false;

        public override bool CanUseItem(Item item, Player player)
        {
            Point mouseTile = Main.MouseWorld.ToTileCoordinates();
            if (Main.LocalPlayer.whoAmI == player.whoAmI && BuffID.Sets.IsWellFed[item.buffType] &&
                CalamityUtils.FindTileEntity<GluttonyBlenderTE>(mouseTile.X, mouseTile.Y, GluttonyBlenderTile.Width, GluttonyBlenderTile.Height) != null &&
                player.IsInTileInteractionRange(mouseTile.X, mouseTile.Y, TileReachCheckSettings.Simple))
            {
                player.ApplyItemAnimation(item);
                return false;
            }
            return base.CanUseItem(item, player);
        }
        public override bool? UseItem(Item item, Player player)
        {
            Main.NewText("g");

            if (Main.LocalPlayer.whoAmI != player.whoAmI)
                return null;

            if (!BuffID.Sets.IsWellFed[item.buffType])
                return null;

            Point mouseTile = Main.MouseWorld.ToTileCoordinates();
            GluttonyBlenderTE entity = CalamityUtils.FindTileEntity<GluttonyBlenderTE>(mouseTile.X, mouseTile.Y, GluttonyBlenderTile.Width, GluttonyBlenderTile.Height);
            if (entity == null)
                return null;

            if (!player.IsInTileInteractionRange(mouseTile.X, mouseTile.Y, TileReachCheckSettings.Simple))
                return null;

            // Velocity doesn't get used for this thing, so it's used here to pass in the position of the top of the blender
            Projectile.NewProjectile(player.GetSource_TileInteraction(mouseTile.X, mouseTile.Y), player.Center, entity.BlenderTop, ModContent.ProjectileType<GluttonyBlenderAnimation>(),
                0, 0f, player.whoAmI, item.type);
            //player.ApplyItemTime(item, 1, false);

            return false;
        }
    }

    public class GluttonyBlenderAnimation : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Typeless";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public ref float ItemType => ref Projectile.ai[0];

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 10;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 120;
        }

        public override bool ShouldUpdatePosition() => false;
        public override void AI()
        {
            if (Projectile.timeLeft == 120)
            {
                Main.NewText("s");
                Projectile.ai[1] = Projectile.velocity.X;
                Projectile.ai[2] = Projectile.velocity.Y;
                Projectile.velocity = Vector2.Zero;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = TextureAssets.Item[(int)ItemType].Value;
            Rectangle frame = tex.Frame(1, Main.itemAnimations[(int)ItemType].FrameCount, 0, 0);
            Main.EntitySpriteDraw(tex, Projectile.Center, frame, lightColor, Projectile.rotation, Projectile.Size * 0.5f, Projectile.scale, SpriteEffects.None);
            return false;
        }
    }
}
