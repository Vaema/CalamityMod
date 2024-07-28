using System;
using System.Collections.Generic;
using System.IO;
using CalamityMod.BiomeManagers;
using CalamityMod.Projectiles.Enemy;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.Bestiary;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.Utilities;
using static CalamityMod.CalamityUtils;

namespace CalamityMod.NPCs.SunkenSea
{
    public class Sharkoon : ModNPC
    {
        public static float TargetDistanceDetection = 160f;
        public static int IdleMovementUnlikeliness = 150;
        public static float MaxIdleSpeed = 3f;
        public static float IdleMovementMaxRange = 1600f;

        #region Fields & Properties

        /// <summary>
        /// The <see cref="Player"/> that this <see cref="NPC"/> has detected as its target.
        /// </summary>
        private Player PlayerTarget
        {
            get => playerTarget;
            set
            {
                playerTarget = value;
                NetUpdate();
            }
        }
        private Player playerTarget;

        /// <summary>
        /// The different AI states this enemy can have.
        /// </summary>
        private enum AIState { OutsideWater, Normal, Exploding }

        /// <summary>
        /// The current AI state of this enemy.
        /// </summary>
        private AIState State
        {
            get => (AIState)NPC.ai[0];
            set
            {
                NPC.ai[0] = (float)value;

                if (value == AIState.Exploding)
                    Animation = AnimationState.Explosion;

                // When outside of water, it'll gravitate down, can't swim.
                NPC.noGravity = value != AIState.OutsideWater;

                NetUpdate();
            }
        }

        /// <summary>
        /// The different types of animation this enemy can have.
        /// </summary>
        private enum AnimationState { Normal, Explosion, Shrunk }

        /// <summary>
        /// The current animation being played right now.
        /// </summary>
        private AnimationState Animation
        {
            get => (AnimationState)NPC.ai[1];
            set
            {
                NPC.ai[1] = (float)value;

                // In case an animation is being set constantly,
                // so we don't have it reset to the first frame every time,
                // it'll only reset the current frame when actually changing animations.
                if (value != Animation)
                {
                    NPC.frame.Y = 0;
                    NPC.frameCounter = 0;
                }

                switch (value)
                {
                    case AnimationState.Normal:
                        ChangeAnimation(animationFrames: 7, timePerAnimationFrame: 7, loops: true);
                        break;
                    case AnimationState.Explosion:
                        ChangeAnimation(animationFrames: 16, timePerAnimationFrame: 8, loops: false);
                        break;
                    case AnimationState.Shrunk:
                        ChangeAnimation(animationFrames: 6, timePerAnimationFrame: 6, loops: true);
                        break;
                }

                NetUpdate();
            }
        }

        /// <summary>
        /// The amount of frames that one column of sprites has.<br/>
        /// Defaults to 7 since that's the first column's amount of frames.
        /// </summary>
        private int AnimationFrames = 7;

        /// <summary>
        /// The amount, in frames, that it takes to go to the next animation frame.<br/>
        /// Defaults to 7.
        /// </summary>
        private int TimePerAnimationFrame = 7;

        /// <summary>
        /// Whether or not the animation loops or not.<br/>
        /// Defaults to <see langword="true"/>.
        /// </summary>
        private bool AnimationLoop = true;

        /// <summary>
        /// A netcode-synced boolean that is <see langword="true"/> when this is the first frame that it exists,<br/>
        /// meaning it just spawned.
        /// </summary>
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

        /// <summary>
        /// A list of <see cref="Vector2"/> which makes a path for the <see cref="NPC"/> to follow.
        /// </summary>
        private List<Vector2> PathPositions;

        #endregion

        #region Other Overridden Methods

        public override void SetStaticDefaults() => Main.npcFrameCount[Type] = 16;

        public override void SetDefaults()
        {
            NPC.damage = 20;
            NPC.lifeMax = 350;
            NPC.defense = 5;
            NPC.knockBackResist = 0.15f;
            NPC.chaseable = false;

            NPC.aiStyle = -1;
            AIType = -1;
            NPC.width = 52;
            NPC.height = 46;
            NPC.noGravity = true;

            NPC.HitSound = SoundID.NPCHit1;
            NPC.DeathSound = SoundID.NPCDeath1;
            NPC.value = Item.buyPrice(0, 0, 5, 0);
            // Banner = NPC.type;
            // BannerItem = ModContent.ItemType<SharkoonBanner>();

            SpawnModBiomes = new int[1] { ModContent.GetInstance<SunkenSeaBiome>().Type };

            NPC.Calamity().VulnerableToHeat = false;
            NPC.Calamity().VulnerableToSickness = true;
            NPC.Calamity().VulnerableToElectricity = true;
            NPC.Calamity().VulnerableToWater = false;
        }

        public override void SetBestiary(BestiaryDatabase database, BestiaryEntry bestiaryEntry)
        {
            bestiaryEntry.Info.AddRange(new IBestiaryInfoElement[]
            {
                new FlavorTextBestiaryInfoElement("Mods.CalamityMod.Bestiary.Sharkoon")
            });
        }

        public override float SpawnChance(NPCSpawnInfo spawnInfo)
        {
            if (spawnInfo.Player.Calamity().ZoneSunkenSea && spawnInfo.Water && !spawnInfo.Player.Calamity().clamity)
            {
                return SpawnCondition.CaveJellyfish.Chance * 0.9f;
            }
            return 0f;
        }

        public override void HitEffect(NPC.HitInfo hit)
        {
            for (int k = 0; k < 5; k++)
            {
                Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.RedMoss, hit.HitDirection, -1f, 0, default, 1f);
            }
            if (NPC.life <= 0)
            {
                for (int k = 0; k < 25; k++)
                {
                    Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.RedMoss, hit.HitDirection, -1f, 0, default, 1f);
                }
            }
        }

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
                case AIState.Normal:
                    NormalState();
                    break;
                case AIState.Exploding:
                    ExplodingState();
                    break;
                case AIState.OutsideWater:
                    OutsideWaterState();
                    return;
            }
        }

        /// <summary>
        /// The behavior of this NPC when it it's just doing normal behavior.
        /// </summary>
        private void NormalState()
        {
            // If the NPC's outside of water, switch the state.
            if (!NPC.wet)
            {
                State = AIState.OutsideWater;
                return;
            }

            // If the NPC finds a valid target and it still is normal, go, commit boom.
            if (Animation == AnimationState.Normal && FindsTarget())
            {
                State = AIState.Exploding;
                return;
            }

            if (PathPositions == null)
            {
                // When there's no path, it'll deacclerate and eventually stay still.
                NPC.velocity *= 0.95f;

                // At a random moment, it'll choose to move.
                if (Main.rand.NextBool(IdleMovementUnlikeliness))
                {
                    var grid = GetPathGrid();
                    AStar pathfinding = new AStar(grid, new(NPC.Center.ToSafeTileCoordinates()), new(grid[Main.rand.Next(grid.Count)]));
                    PathPositions = pathfinding.FindPath();
                    Timer = 0f;
                    NetUpdate();
                }
            }

            else
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

            NPC.velocity = Vector2.Clamp(NPC.velocity, -Vector2.One * MaxIdleSpeed, Vector2.One * MaxIdleSpeed);
            NPC.rotation = MathHelper.ToRadians(NPC.velocity.Length() * 3f) * MathF.Sign(NPC.velocity.X);
            NPC.spriteDirection = -MathF.Sign(NPC.velocity.X);
        }

        /// <summary>
        /// The behavior of this NPC when it is currently trying to explode.
        /// </summary>
        private void ExplodingState()
        {
            // Deaccelerate.
            NPC.velocity *= 0.95f;

            // On the exact frame that the NPC explodes, it'll will, indeed, explode.
            if (NPC.frame.Y == NPC.height * 9 && NPC.frameCounter == 0)
                Kaboom();

            // When the animation's finished, go back to being a normal fish.
            if (NPC.frame.Y >= NPC.height * 16)
            {
                Animation = AnimationState.Shrunk;
                State = AIState.Normal;
            }
        }

        /// <summary>
        /// The behavior of this NPC when it's outside of water.
        /// </summary>
        private void OutsideWaterState()
        {
            if (NPC.wet)
            {
                State = AIState.Normal;
                return;
            }
        }

        /// <summary>
        /// What happens when this NPC explodes.
        /// </summary>
        private void Kaboom()
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                Projectile.NewProjectileDirect(
                    NPC.GetSource_FromThis(),
                    NPC.Center,
                    Vector2.Zero,
                    ModContent.ProjectileType<SharkoonExplosion>(),
                    NPC.damage,
                    10f,
                    Main.myPlayer);
            }

            // VFX and sound effects go here,
            // a dedicated server doesn't need to load these.
            if (!Main.dedServ)
            {
                // HI XYK!
            }

            NetUpdate();
        }

        /// <summary>
        /// Finds a valid target for this NPC.
        /// </summary>
        /// <returns>Whether or not it has a found a valid target.</returns>
        private bool FindsTarget()
        {
            // When the NPC already had a valid target,
            // consider it null if it's not at a desired distance anymore.
            if (PlayerTarget is not null)
            {
                if (NPC.Center.Distance(PlayerTarget.Center) > TargetDistanceDetection)
                {
                    PlayerTarget = null;
                    return false;
                }

                return true;
            }

            // Finds the closest target.
            NPC.TargetClosest(false);
            PlayerTarget = Main.player[NPC.target];

            // If there was a target but not at a desired distance, we consider there was no target.
            if (PlayerTarget is not null && NPC.Center.Distance(PlayerTarget.Center) > TargetDistanceDetection)
            {
                PlayerTarget = null;
                return false;
            }

            return true;
        }

        /// <summary>
        /// Creates a list of points that constitutes the grid which the A* algorithm will use.
        /// </summary>
        private List<Point> GetPathGrid()
        {
            List<Point> grid = new();

            Point topLeftCorner = CalamityUtils.ToSafeTileCoordinates(NPC.position + new Vector2(-IdleMovementMaxRange, -IdleMovementMaxRange));
            Point bottomRightCorner = CalamityUtils.ToSafeTileCoordinates(NPC.position + new Vector2(IdleMovementMaxRange, IdleMovementMaxRange));

            for (int coordY = topLeftCorner.Y; coordY <= bottomRightCorner.Y; coordY++)
            {
                for (int coordX = topLeftCorner.X; coordX <= bottomRightCorner.X; coordX++)
                {
                    Point point = new(coordX, coordY);

                    if (Main.tile[point].IsTileSolid() || Main.tile[point].LiquidAmount != 255 || !AdjacentPointsValid(point))
                        continue;

                    grid.Add(point);
                }
            }

            return grid;
        }

        /// <summary>
        /// Checks whether or not path point is valid depending on its adjacent points.
        /// </summary>
        private bool AdjacentPointsValid(Point point)
        {
            foreach (var direction in Directions)
            {
                Point adjacentPoint = new(point.X + (int)direction.X, point.Y + (int)direction.Y);

                if (Main.tile[adjacentPoint].IsTileSolid())
                    return false;

                adjacentPoint = new(point.X + (int)(direction.X * 2f), point.Y + (int)(direction.Y * 2f));

                if (Main.tile[adjacentPoint].IsTileSolid())
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Syncs to the server.
        /// </summary>
        private void NetUpdate()
        {
            NPC.netUpdate = true;
            NPC.netSpam = 0;
        }

        #endregion

        #region Drawing & Animation

        /// <summary>
        /// A quick method to change all the animation's properties.
        /// </summary>
        private void ChangeAnimation(int animationFrames, int timePerAnimationFrame, bool loops)
        {
            AnimationFrames = animationFrames;
            TimePerAnimationFrame = timePerAnimationFrame;
            AnimationLoop = loops;
        }

        public override void FindFrame(int frameHeight)
        {
            NPC.frameCounter++;
            if (NPC.frameCounter > TimePerAnimationFrame)
            {
                NPC.frame.Y += frameHeight;
                NPC.frame.Y = Math.Min(NPC.frame.Y, AnimationFrames * frameHeight);
                NPC.frameCounter = 0;
                if (AnimationLoop)
                    NPC.frame.Y = NPC.frame.Y % (AnimationFrames * frameHeight);
            }
        }

        public override bool PreDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            Texture2D texture = TextureAssets.Npc[Type].Value;
            Vector2 drawPosition = NPC.Center - screenPos;
            Rectangle frame = texture.Frame(horizontalFrames: 3, verticalFrames: 16, frameX: (int)Animation, frameY: NPC.frame.Y / NPC.height);
            Vector2 anchorPoint = frame.Size() * 0.5f;
            SpriteEffects flip = NPC.spriteDirection == -1 ? SpriteEffects.FlipHorizontally : SpriteEffects.None;

            spriteBatch.Draw(texture, drawPosition, frame, NPC.GetAlpha(drawColor), NPC.rotation, anchorPoint, NPC.scale, flip, 0f);

            return false;
        }

        #endregion

        #region Syncing

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write7BitEncodedInt(AnimationFrames);
            writer.Write7BitEncodedInt(TimePerAnimationFrame);
            writer.Write(AnimationLoop);

            writer.Write7BitEncodedInt(PathPositions.Count);
            foreach (var position in PathPositions)
                writer.WritePackedVector2(position);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            AnimationFrames = reader.Read7BitEncodedInt();
            TimePerAnimationFrame = reader.Read7BitEncodedInt();
            AnimationLoop = reader.ReadBoolean();

            List<Vector2> receivedPathPositions = new();
            int receivedCount = reader.Read7BitEncodedInt();
            for (int i = 0; i < receivedCount; i++)
                receivedPathPositions.Add(reader.ReadPackedVector2());
            PathPositions = receivedPathPositions;
        }

        #endregion
    }
}
