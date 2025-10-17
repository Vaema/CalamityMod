using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalamityMod.Graphics.Primitives;
using CalamityMod.Items;
using CalamityMod.NPCs.NormalNPCs;
using CalamityMod.Projectiles.Magic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Systems.Mechanic
{
    public class ArenaWallSystem : ModSystem
    {
        public static List<Box> ActiveBoxes = [];
        public class Box
        {

            public Func<bool> RemovalCondition = () => false;

            public Vector2 position;

            public Vector4 boxDimensions;

            public Vector4 NewDimensions;

            public Vector2 NewPosition;

            public float borderThickness;

            public Color borderColor = Color.Red;
            public float DistanceUp => boxDimensions.X;
            public float DistanceRight => boxDimensions.Y;
            public float DistanceDown => boxDimensions.Z;
            public float DistanceLeft => boxDimensions.W;

            public Vector2 TopLeft => position + new Vector2(-DistanceLeft, -DistanceUp);
            public Vector2 TopRight => position + new Vector2( DistanceRight, -DistanceUp);
            public Vector2 BottomLeft => position + new Vector2(-DistanceLeft, DistanceDown);
            public Vector2 BottomRight => position + new Vector2(DistanceRight, DistanceDown);

            public Vector2 Center => (TopLeft + BottomRight) * 0.5f;

            public Vector2 Size => new Vector2(DistanceLeft + DistanceRight, DistanceUp + DistanceDown);

            public Vector4 Hitbox => new Vector4(TopLeft.X, TopLeft.Y, Size.X, Size.Y);

            public Box oldData = null;

            public void SetOldData()
            {
                oldData = new Box() { borderColor = borderColor, borderThickness = borderThickness, boxDimensions = boxDimensions, position = position };
            }
            public bool Contains(Player player)
            {
                return Collision.CheckAABBvAABBCollision(TopLeft - new Vector2(borderThickness * 0.5f), Size + new Vector2(borderThickness), player.TopLeft, player.Size);
            }

            public bool ShouldEffectPlayer(Player player) => Collision.CheckAABBvAABBCollision(TopLeft - new Vector2(borderThickness + 300), Size + new Vector2(borderThickness + 300)*2, player.TopLeft, player.Size);
        }
        public override void PostDrawTiles() //Later all of this method should be stored in the box instance for more customizability.
        {
            void DrawBoxWithOffset(Box box, float Offset, float Thickness, Color color)
            {

                CalamityUtils.DrawLineBetter(Main.spriteBatch, box.TopLeft + new Vector2( -(Offset + Thickness * 0.5f), -Offset), box.TopRight + new Vector2((Offset + Thickness * 0.5f), -Offset), color, Thickness);
                CalamityUtils.DrawLineBetter(Main.spriteBatch, box.BottomLeft + new Vector2(-(Offset + Thickness * 0.5f), Offset), box.BottomRight + new Vector2((Offset + Thickness * 0.5f), Offset), color, Thickness);
                CalamityUtils.DrawLineBetter(Main.spriteBatch, box.TopLeft + new Vector2(-Offset, -(Offset - Thickness * 0.5f)), box.BottomLeft + new Vector2(-Offset, (Offset - Thickness * 0.5f)), color, Thickness);
                CalamityUtils.DrawLineBetter(Main.spriteBatch, box.BottomRight + new Vector2(Offset, (Offset - Thickness * 0.5f)), box.TopRight + new Vector2(Offset, -(Offset - Thickness * 0.5f)), color, Thickness);
            }

            Main.spriteBatch.Begin(
                SpriteSortMode.Deferred,
                BlendState.AlphaBlend,
                SamplerState.PointClamp,
                DepthStencilState.None,
                RasterizerState.CullNone,
                null,
                Main.GameViewMatrix.ZoomMatrix
            );
            foreach (var box in ActiveBoxes)
            {

                var color = Color.Black * 0.75f;
                //Inside Fill
                DrawBoxWithOffset(box, box.borderThickness * 0.5f, box.borderThickness, Color.Black * 0.75f);
                //Inner Border
                DrawBoxWithOffset(box, 4, 8, box.borderColor);
                //Inner Border Clones
                float amount = 4;
                float totalDistance = 64f;
                for (var i = Main.GlobalTimeWrappedHourly % 1; i < amount; i++)
                {
                    DrawBoxWithOffset(box, totalDistance * (i / amount) + 4, 4, box.borderColor * (1-i / amount));
                }
                //Outer Border
                DrawBoxWithOffset(box, box.borderThickness-4, 4, box.borderColor);
            }
            Main.spriteBatch.End();
        }

        public override void PreUpdateEntities()
        {
            for (var i = 0; i < ActiveBoxes.Count; i++)
            {
                var box = ActiveBoxes[i];
                if (box.RemovalCondition())
                {
                    ActiveBoxes.Remove(box);
                    i--;
                    continue;
                }

                box.SetOldData();
                if (box.NewDimensions != Vector4.Zero)
                {
                    box.boxDimensions = box.NewDimensions;
                }
                if (box.NewPosition != Vector2.Zero)
                {
                    box.position = box.NewPosition;
                }

                //These VFX need to become per-box eventually.
                for (var i2 = 0; i2 < box.Size.Y / 400f; i2++)
                {
                    var p = Vector2.Lerp(box.BottomRight, box.TopRight, Main.rand.NextFloat());
                    Dust.NewDustPerfect(p, DustID.Clentaminator_Red, p.DirectionFrom(box.Center) * Main.rand.NextFloat(0, 5), Scale: Main.rand.NextFloat(0.1f, 1f), newColor: Color.Crimson);

                    p = Vector2.Lerp(box.TopLeft, box.BottomLeft, Main.rand.NextFloat());
                    Dust.NewDustPerfect(p, DustID.Clentaminator_Red, p.DirectionFrom(box.Center) * Main.rand.NextFloat(0, 5), Scale: Main.rand.NextFloat(0.1f, 1f), newColor: Color.Crimson);

                }
                for (var i2 = 0; i2 < box.Size.X / 400f; i2++)
                {
                    var p = Vector2.Lerp(box.TopLeft, box.TopRight, Main.rand.NextFloat());
                    Dust.NewDustPerfect(p, DustID.Clentaminator_Red, p.DirectionFrom(box.Center) * Main.rand.NextFloat(0, 5), Scale: Main.rand.NextFloat(0.1f, 1f), newColor: Color.Crimson);
                    p = Vector2.Lerp(box.BottomRight, box.BottomLeft, Main.rand.NextFloat());
                    Dust.NewDustPerfect(p, DustID.Clentaminator_Red, p.DirectionFrom(box.Center) * Main.rand.NextFloat(0, 5), Scale: Main.rand.NextFloat(0.1f, 1f),newColor: Color.Crimson);
                }
            }
        }
    }

    public class ArenaWallPlayer : ModPlayer
    {
        public Vector2 touchingSides = new Vector2();
        public override void PreUpdateMovement()
        {


            foreach (var box in ArenaWallSystem.ActiveBoxes)
            {
                if (box.ShouldEffectPlayer(Player))
                {
                    if (box.Contains(Player))
                        ContainPlayerLogic(box);
                }
            }
        }

        void ContainPlayerLogic(ArenaWallSystem.Box box)
        {
            if (box.oldData is not null && false)
            {
                if (touchingSides.X > 0)
                {
                    Player.position.Y += ((box.oldData.TopLeft.Y - box.oldData.BottomRight.Y) - (box.TopLeft.Y - box.BottomRight.Y)) * touchingSides.X;
                }

                if (touchingSides.Y > 0)
                {
                    Player.position.X += ((box.oldData.TopLeft.X - box.oldData.BottomRight.X) - (box.TopLeft.X - box.BottomRight.X)) * touchingSides.Y;
                }
            }

            touchingSides = Vector2.Zero;
            #region Snapping
            if (Player.Left.X < box.TopLeft.X)
            {
                Player.position.X = box.TopLeft.X;
                touchingSides.X = Utils.Remap(Player.Center.Y, box.TopLeft.Y, box.BottomRight.Y, 0, 1, true);
                touchingSides.Y = Utils.Remap(Player.Center.X, box.TopLeft.X, box.BottomRight.X, 0, 1, true);
            }
            if (Player.Right.X > box.BottomRight.X)
            {
                Player.position.X = box.BottomRight.X - Player.width;
                touchingSides.X = Utils.Remap(Player.Center.Y, box.TopLeft.Y, box.BottomRight.Y, 0, 1, true);
                touchingSides.Y = Utils.Remap(Player.Center.X, box.TopLeft.X, box.BottomRight.X, 0, 1, true);
            }
            if (Player.TopLeft.Y < box.TopLeft.Y)
            {
                Player.position.Y = box.TopLeft.Y;
                touchingSides.X = Utils.Remap(Player.Center.Y, box.TopLeft.Y, box.BottomRight.Y, 0, 1, true);
                touchingSides.Y = Utils.Remap(Player.Center.X, box.TopLeft.X, box.BottomRight.X, 0, 1, true);
            }
            if (Player.BottomRight.Y > box.BottomRight.Y)
            {
                Player.position.Y = box.BottomRight.Y - Player.height;
                touchingSides.X = Utils.Remap(Player.Center.Y, box.TopLeft.Y, box.BottomRight.Y, 0, 1, true);
                touchingSides.Y = Utils.Remap(Player.Center.X, box.TopLeft.X, box.BottomRight.X, 0, 1, true);
            }
            #endregion
            void spawnShoeDust()
            {

                int num4 = Dust.NewDust(new Vector2(Player.position.X + (float)(Player.width / 2) + (float)((Player.width / 2 - 4) * Player.slideDir), Player.position.Y + (float)(Player.height / 2) + (float)(Player.height / 2 - 4) * Player.gravDir), 8, 8, 31);
                if (Player.slideDir < 0)
                {
                    Main.dust[num4].position.X -= 10f;
                }
                if (Player.gravDir < 0f)
                {
                    Main.dust[num4].position.Y -= 12f;
                }
                Main.dust[num4].velocity *= 0.1f;
                Main.dust[num4].scale *= 1.2f;
                Main.dust[num4].noGravity = true;
                Main.dust[num4].shader = GameShaders.Armor.GetSecondaryShader(Player.cShoe, Player);
            }
            void applyShoeSpikes()
            {
                if (Player.spikedBoots == 0 || Player.velocity.Y < 0 || Player.mount.Active)
                    return;
                if (Player.controlDown && Player.spikedBoots > 0)
                {
                    Player.velocity.Y = 4f * Player.gravDir;
                    spawnShoeDust();
                }
                else if (Player.spikedBoots <= 2)
                {
                    Player.velocity.Y = 0;
                }
                else if (Player.spikedBoots == 1)
                {

                    Player.velocity.Y = 0.5f * Player.gravDir;
                    spawnShoeDust();
                }
            }
            #region Velocity

            var originalVelocity = Player.velocity;
            var originalTopLeft = Player.TopLeft;
            var originalBottomRight = Player.BottomRight;
            Player.position += originalVelocity;

            if (Player.Left.X < box.TopLeft.X)
            {
                Player.velocity.X = box.TopLeft.X - originalTopLeft.X;
                if (Player.controlLeft)
                {
                    Player.slideDir = -1;
                    applyShoeSpikes();
                }
            }
            if (Player.Right.X > box.BottomRight.X)
            {
                Player.velocity.X = box.BottomRight.X - originalBottomRight.X;
                if (Player.controlRight)
                {
                    Player.slideDir = 1;
                    applyShoeSpikes();
                }
            }
            if (Player.TopLeft.Y < box.TopLeft.Y)
            {

                Player.velocity.Y = box.TopLeft.Y - originalTopLeft.Y;
                if (Player.velocity.Y == 0)
                    Player.velocity.Y += 0.001f;
                touchingSides.Y = Utils.Remap(Player.Center.X, box.TopLeft.X, box.BottomRight.X, 0, 1, true);
            } 
            
            if (Player.BottomRight.Y > box.BottomRight.Y)
            {
                Player.velocity.Y = box.BottomRight.Y - originalBottomRight.Y;
                touchingSides.Y = Utils.Remap(Player.Center.X, box.TopLeft.X, box.BottomRight.X, 0, 1, true);
            }
            Player.position -= originalVelocity;
            #endregion
        }
    }
}
