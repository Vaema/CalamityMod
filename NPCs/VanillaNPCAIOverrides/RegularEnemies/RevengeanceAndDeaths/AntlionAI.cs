using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.NPCs.VanillaNPCAIOverrides.RegularEnemies;

public static partial class RevengeanceAndDeathAI
{
    public class AntlionAI : VanillaAIOverride
    {
        public override bool AI(Mod mod)
        {
            NPC.TargetClosest();

            // Calculate speed and velocity of the sand balls
            float speed = 12f;
            float xVel = Main.player[NPC.target].Center.X - NPC.Center.X;
            float yVel = Main.player[NPC.target].position.Y - NPC.Center.Y;
            Vector2 velocity = new Vector2(xVel, yVel);
            float targetDist = velocity.Length();

            targetDist = speed / targetDist;
            velocity.X *= targetDist;
            velocity.Y *= targetDist;

            // Adjust rotation and velocity
            bool canShoot = false;
            if (NPC.directionY < 0)
            {
                // Set rotation based on the target location
                NPC.rotation = velocity.ToRotation() + MathHelper.PiOver2;

                // Antlions can only shoot if rotated between a certain cone of spread based on the target location
                canShoot = Math.Abs(NPC.rotation) <= 1.2f;

                // Hardcap rotation so it doesn't look weird, but since the above calculation happens first, it ignores this cap
                if (NPC.rotation < -0.8f)
                    NPC.rotation = -0.8f;
                else if (NPC.rotation > 0.8f)
                    NPC.rotation = 0.8f;

                // Antlions generally don't move horizontally so prevent that as needed
                if (NPC.velocity.X != 0f)
                {
                    NPC.velocity.X *= 0.9f;
                    if (Math.Abs(NPC.velocity.X) < 0.1f)
                    {
                        NPC.netUpdate = true;
                        NPC.velocity.X = 0f;
                    }
                }
            }

            bool lineofSight = Collision.CanHit(NPC.position, NPC.width, NPC.height, Main.player[NPC.target].position, Main.player[NPC.target].width, Main.player[NPC.target].height);

            if (NPC.justHit || !lineofSight)
                NPC.ai[0] = AntlionSandSpitGateValue - 1f;

            // Decrement the firing cooldown, play a sound if at full meaning it just fired
            if (NPC.ai[0] > 0f)
            {
                if (NPC.ai[0] == AntlionSandSpitGateValue)
                    SoundEngine.PlaySound(SoundID.NPCDeath13, NPC.Center);

                NPC.ai[0] -= 1f;
            }

            // Emit sand dust from mouth when about to fire
            if (NPC.ai[0] <= AntlionSandSpitTelegraphTime)
            {
                Dust dust = Dust.NewDustDirect(NPC.Center + velocity.SafeNormalize(-Vector2.UnitY) * 16f + Main.rand.NextVector2CircularEdge(6f, 6f), 1, 1, DustID.Sand, 0f, 0f, 0, default, 1.5f);
                dust.noGravity = true;
                dust.velocity *= 0f;
            }

            // Antlions should only fire if the target is in the shooting cone and has a line of sight as well as not being on cooldown.
            if (Main.netMode != NetmodeID.MultiplayerClient && canShoot && NPC.ai[0] == 0f && lineofSight)
            {
                // Reset the firing cooldown to 3.3333 seconds
                NPC.ai[0] = AntlionSandSpitGateValue;

                // With the Rev and Death damage calculations, this becomes 56 damage.
                int damage = 10;

                int projType = ProjectileID.SandBallFalling;

                // In FTW, can fire 8 sand balls (100 in stupid meme seed)
                int projAmt = Main.zenithWorld ? 100 : Main.getGoodWorld ? 8 : 1;
                for (int i = 0; i < projAmt; i++)
                {
                    // Adjust the velocity to make it a shotgun-like spread
                    velocity.X += (float)Main.rand.Next(-30, 31) * 0.05f;
                    velocity.Y += (float)Main.rand.Next(-30, 31) * 0.05f;

                    int sandBall = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, velocity, projType, damage, 0f, Main.myPlayer);
                    Main.projectile[sandBall].ai[0] = 2f;
                    Main.projectile[sandBall].timeLeft = 300;
                    Main.projectile[sandBall].friendly = false;
                    NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, sandBall, 0f, 0f, 0f, 0, 0, 0);
                }

                NPC.netUpdate = true;
            }

            try
            {
                // This tile checking behavior is used for when Antlions cover themselves in sand and need to rise upward to get to the surface
                int xLeft = (int)NPC.position.X / 16;
                int xCenter = (int)NPC.Center.X / 16;
                int xRight = (int)(NPC.position.X + (float)NPC.width) / 16;
                int y = (int)(NPC.position.Y + (float)NPC.height) / 16;
                bool tileClimbing = false;
                if ((Main.tile[xLeft, y].HasUnactuatedTile && Main.tileSolid[(int)Main.tile[xLeft, y].TileType]) || (Main.tile[xCenter, y].HasUnactuatedTile && Main.tileSolid[(int)Main.tile[xCenter, y].TileType]) || (Main.tile[xRight, y].HasUnactuatedTile && Main.tileSolid[(int)Main.tile[xRight, y].TileType]))
                    tileClimbing = true;

                if (tileClimbing)
                {
                    NPC.noGravity = true;
                    NPC.noTileCollide = true;
                    NPC.velocity.Y = -0.2f;
                }

                // If not rising up through tiles, occasionally spawn some dust
                else
                {
                    NPC.noGravity = false;
                    NPC.noTileCollide = false;
                    if (Main.rand.NextBool())
                    {
                        int sand = Dust.NewDust(new Vector2(NPC.position.X - 4f, NPC.position.Y + (float)NPC.height - 8f), NPC.width + 8, 24, DustID.Sand, 0f, NPC.velocity.Y / 2f, 0, default(Color), 1f);
                        Dust dust = Main.dust[sand];
                        dust.velocity.X *= 0.4f;
                        dust.velocity.Y *= -1f;
                        if (Main.rand.NextBool())
                        {
                            dust.noGravity = true;
                            dust.scale += 0.2f;
                        }
                    }
                }
            }
            catch
            {
            }

            return false;
        }
    }
}
