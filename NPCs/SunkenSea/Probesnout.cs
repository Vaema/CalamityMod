using System;
using System.Collections.Generic;
using System.IO;
using CalamityMod.BiomeManagers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;
using static CalamityMod.CalamityUtils;

namespace CalamityMod.NPCs.SunkenSea
{
    public class Probesnout : ModNPC
    {
        public static float PathDetectionSize = 300f;

        #region Members

        private enum AIState { OutsideWater, Idle }

        private AIState State
        {
            get => (AIState)NPC.ai[0];
            set
            {
                NPC.ai[0] = (float)value;
                NetUpdate();
            }
        }

        private enum AnimationState { Idle, Attack }

        private AnimationState Animation
        {
            get => (AnimationState)NPC.ai[1];
            set
            {
                if (value != Animation)
                {
                    NPC.frame.Y = 0;
                    NPC.frameCounter = 0;
                }

                switch (value)
                {
                    case AnimationState.Idle:
                        AnimationFrames = 8;
                        TimePerAnimationFrame = 5;
                        break;
                    case AnimationState.Attack:
                        AnimationFrames = 12;
                        TimePerAnimationFrame = 7;
                        break;
                }

                NPC.ai[1] = (float)value;
                NetUpdate();
            }
        }

        private int AnimationFrames = 8;

        private int TimePerAnimationFrame = 5;

        private bool HasSpawned
        {
            get => NPC.ai[2] == 1f;
            set
            {
                NPC.ai[2] = value.ToInt();
                NetUpdate();
            }
        }

        private ref float Timer => ref NPC.ai[3];

        private List<Vector2> PathPositions;

        #endregion

        #region AI

        public override void AI()
        {
            if (!HasSpawned)
            {
                NPC.spriteDirection = Main.rand.NextBool().ToDirectionInt();
                NPC.GravityMultiplier *= 2f;
                NPC.MaxFallSpeedMultiplier *= 2f;
                HasSpawned = true;
            }

            switch (State)
            {
                case AIState.OutsideWater:
                    OutsideWaterState();
                    break;
                case AIState.Idle:
                    IdleState();
                    break;
            }

            NPC.rotation = MathHelper.ToRadians(NPC.velocity.Length() * 3f) * MathF.Sign(NPC.velocity.X);
            NPC.spriteDirection = -MathF.Sign(NPC.velocity.X);
        }

        private void OutsideWaterState()
        {
            if (NPC.wet)
            {
                State = AIState.Idle;
                return;
            }
        }

        private void IdleState()
        {
            if (!NPC.wet)
            {
                State = AIState.OutsideWater;
                return;
            }

            if (PathPositions is null)
            {
                NPC.velocity *= 0.95f;

                if (Main.rand.NextBool(125))
                {
                    var grid = DecidePathGrid();
                    PathPositions = AStar.GetPath(grid, NPC.Center.ToSafeTileCoordinates(), grid[Main.rand.Next(grid.Count)]);
                    Timer = 0f;
                    NetUpdate();
                }
            }

            else if (PathPositions is not null)
            {
                // Iterates through all the vectors in the path as time goes on.
                Vector2 followedPathPoint = PathPositions[(int)Math.Floor(Timer)];

                // Accelerates towards the followed point.
                NPC.velocity += NPC.Center.DirectionTo(followedPathPoint) * 0.03f;

                Timer += 0.07f;
                Timer = MathF.Min(Timer, PathPositions.Count - 1);

                // If very near the end point or too far from the followed path, reset the path.
                if (NPC.Center.DistanceSQ(PathPositions[^1]) < 14400f || NPC.Center.DistanceSQ(followedPathPoint) > 102400f)
                {
                    PathPositions = null;
                    NetUpdate();
                }
            }

            NPC.velocity = Vector2.Clamp(NPC.velocity, -Vector2.One * 3f, Vector2.One * 3f);
        }

        private List<Point> DecidePathGrid()
        {
            List<Point> grid = new();
            Point topLeftCorner = (NPC.Center + new Vector2(-PathDetectionSize, -PathDetectionSize)).ToSafeTileCoordinates();
            Point bottomRightCorner = (NPC.Center + new Vector2(PathDetectionSize, PathDetectionSize)).ToSafeTileCoordinates();

            for (int coordY = topLeftCorner.Y; coordY <= bottomRightCorner.Y; coordY++)
            {
                for (int coordX = topLeftCorner.X; coordX <= bottomRightCorner.X; coordX++)
                {
                    Point point = new(coordX, coordY);

                    if (Main.tile[point].IsTileSolid() || Main.tile[point].LiquidAmount != 255 || !AreAdjacentTilesValid(point))
                        continue;

                    grid.Add(point);
                }
            }

            return grid;
        }

        private bool AreAdjacentTilesValid(Point point)
        {
            Vector2[] adjacents = new Vector2[12]
            {
                Vector2.UnitX, -Vector2.UnitX, Vector2.UnitY, -Vector2.UnitY,
                new(1f, 1f), new(1f, -1f), new(-1f, -1f), new(-1f, 1f),
                Vector2.UnitX * 2f, -Vector2.UnitX * 2f, Vector2.UnitY * 2f, -Vector2.UnitY * 2f
            };

            foreach (var adjacent in adjacents)
            {
                Point adjacentPoint = new(point.X + (int)adjacent.X, point.Y + (int)adjacent.Y);
                if (Main.tile[adjacentPoint].IsTileSolid())
                    return false;
            }

            return true;
        }

        private void NetUpdate()
        {
            NPC.netUpdate = true;
            NPC.netSpam = 0;
        }

        #endregion

        #region Drawing & Animation

        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter++;
            if (NPC.frameCounter > TimePerAnimationFrame)
            {
                NPC.frame.Y += frameHeight;
                NPC.frame.Y = Math.Min(NPC.frame.Y, AnimationFrames * frameHeight);
                NPC.frame.Y = NPC.frame.Y % (AnimationFrames * frameHeight);
                NPC.frameCounter = 0;
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = TextureAssets.Npc[Type].Value;
            Vector2 drawPosition = NPC.Center - screenPos;
            Rectangle frame = texture.Frame(horizontalFrames: 2, verticalFrames: 12, frameX: (int)Animation, frameY: NPC.frame.Y / NPC.height);
            Vector2 anchorPoint = frame.Size() * 0.5f;
            SpriteEffects flip = NPC.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            spriteBatch.Draw(texture, drawPosition, frame, NPC.GetAlpha(drawColor), NPC.rotation, anchorPoint, NPC.scale, flip, 0f);

            return false;
        }

        #endregion

        #region Other ModNPC Overrides

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[Type] = 15;
            Main.npcCatchable[Type] = true;
            NPCID.Sets.CountsAsCritter[Type] = true;
        }

        public override void SetDefaults()
        {
            NPC.lifeMax = 5;
            
            NPC.aiStyle = -1;
            AIType = -1;
            NPC.npcSlots = 0.1f;
            NPC.noGravity = true;
            NPC.chaseable = false;

            NPC.width = 44;
            NPC.height = 55;

            SpawnModBiomes = new int[1] { ModContent.GetInstance<SunkenSeaBiome>().Type };
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                new FlavorTextBestiaryInfoElement("Mods.CalamityMod.Bestiary.Probesnaut")
            });
        }

        #endregion

        #region Syncing

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write7BitEncodedInt(AnimationFrames);
            writer.Write7BitEncodedInt(TimePerAnimationFrame);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            AnimationFrames = reader.Read7BitEncodedInt();
            TimePerAnimationFrame = reader.Read7BitEncodedInt();
        }

        #endregion
    }

    public class ProbesnoutGold : Probesnout
    {
        public override void SetStaticDefaults()
        {
            base.SetStaticDefaults();
            this.HideFromBestiary();
        }

        public override void SetDefaults()
        {
            base.SetDefaults();
            NPC.rarity = 3;
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            for (int i = 0; i < 5; i++)
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.GoldCritter, hit.HitDirection, -1f);
        }
    }
}
