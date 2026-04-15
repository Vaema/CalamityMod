using CalamityMod.Items.Placeables.Furniture;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
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
            // Exit early if anyone of the following are true:
            // * Running on client that is not the local player
            // * The item is not a food, is not consumable, or is Delicious Slop (you cannot feed the slop back into the blender)
            // * The Gluttony Blender tile entity doesn't exist for some reason
            // * The player isn't within the tile's interaction range
            if (Main.LocalPlayer.whoAmI != player.whoAmI)
                return true;
            if (!BuffID.Sets.IsWellFed[item.buffType] || !item.consumable || item.type == ModContent.ItemType<DeliciousSlop>())
                return true;

            Point mouseTile = Main.MouseWorld.ToTileCoordinates();
            GluttonyBlenderTE entity = CalamityUtils.FindTileEntity<GluttonyBlenderTE>(mouseTile.X, mouseTile.Y, GluttonyBlenderTile.Width, GluttonyBlenderTile.Height);
            if (entity == null)
                return true;
            if (!player.IsInTileInteractionRange(mouseTile.X, mouseTile.Y, TileReachCheckSettings.Simple))
                return true;

            // Spawns a projectile to handle the visual animation of the food moving into the blender and the conversion to slop
            // The projectile doesn't use velocity, so the top of the blender tile is passed in as the velocity
            Projectile.NewProjectile(player.GetSource_TileInteraction(mouseTile.X, mouseTile.Y), player.Center, entity.BlenderTop, ModContent.ProjectileType<GluttonyBlenderAnimation>(),
                0, 0f, player.whoAmI, item.type);
            if (ItemLoader.ConsumeItem(item, player))
            {
                item.stack--;
                if (item.stack <= 0)
                    item.TurnToAir();
            }
            return false;
        }
    }

    public class GluttonyBlenderAnimation : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Typeless";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        private const int Lifetime = 120;
        private const int TimeToReachBlender = 60;
        private int ItemType => (int)Projectile.ai[0];
        private ref float Timer => ref Projectile.ai[1];
        private Vector2 Start;
        private Vector2 Destination => Projectile.velocity;

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 10;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = Lifetime;
        }

        public override bool ShouldUpdatePosition() => false;
        public override void AI()
        {
            // Initialization
            if (Timer == 0)
            {
                SoundEngine.PlaySound(SoundID.Item1, Projectile.Center);
                Start = Projectile.Center;
            }

            if (Timer < TimeToReachBlender)
            {
                Vector2 moveDistBeforeArc = Destination - Start;
                // The second half of this applies an arcing motion as the projectile moves
                Projectile.Center += (moveDistBeforeArc / (float)TimeToReachBlender) - Vector2.UnitY * (3f - (0.1f * Timer));
                Projectile.rotation = moveDistBeforeArc.X * 0.005f;

                if (Timer >= TimeToReachBlender - 4)
                    Projectile.scale -= 0.25f;
            }
            else
            {
                Projectile.Center = Destination;
                if (Timer == TimeToReachBlender)
                    SoundEngine.PlaySound(SoundID.Item22, Projectile.Center);

                Color[] dustArray = ItemID.Sets.FoodParticleColors[ItemType];
                if (dustArray == null || dustArray.Length == 0)
                    dustArray = ItemID.Sets.DrinkParticleColors[ItemType];
                if (dustArray != null && dustArray.Length != 0 && Main.rand.NextBool(4))
                {
                    Vector2 dustVel = -Vector2.UnitY.RotatedByRandom(MathHelper.Pi / 5f) * 2f;
                    Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.FoodPiece, dustVel, 0, dustArray[Main.rand.Next(dustArray.Length)], Main.rand.NextFloat(1.3f, 1.75f));
                    dust.fadeIn = 0f;
                }
            }

            if (Projectile.timeLeft == 1)
            {
                int itemDrop = Main.rand.NextBool(GluttonyBlender.OneInXChanceForGoodSlop) ? ModContent.ItemType<DeliciousSlop>() : ModContent.ItemType<DisgustingSlop>();
                int i = Item.NewItem(Projectile.GetItemSource_DropAsItem(), Projectile.Center, itemDrop);
                Main.item[i].GetGlobalItem<GluttonyBlenderGlobalItem>().FromGluttonyBlender = true;
            }
            Timer++;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (Timer >= TimeToReachBlender)
                return false;

            Texture2D tex = TextureAssets.Item[ItemType].Value;
            Rectangle frame = tex.Frame(1, Main.itemAnimations[ItemType].FrameCount, 0, 0);
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, frame, lightColor, Projectile.rotation, frame.Size() * 0.5f, Projectile.scale, SpriteEffects.None);
            return false;
        }
    }
}
