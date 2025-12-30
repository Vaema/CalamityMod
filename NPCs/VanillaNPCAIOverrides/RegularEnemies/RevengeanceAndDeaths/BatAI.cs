using CalamityMod.NPCs.PlagueEnemies;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.NPCs.VanillaNPCAIOverrides.RegularEnemies;

public static partial class RevengeanceAndDeathAI
{
    public class BatAI : VanillaAIOverride
    {
        public override bool AI(Mod mod)
        {
            if (NPC.type == NPCID.Hellbat || NPC.type == NPCID.Lavabat)
            {
                int lavaDust = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Torch, NPC.velocity.X * 0.2f, NPC.velocity.Y * 0.2f, 100, default(Color), 2f);
                Main.dust[lavaDust].noGravity = true;
            }

            if (NPC.type == NPCID.IceBat && Main.rand.NextBool(10))
            {
                int iceDust = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.IceRod, NPC.velocity.X * 0.5f, NPC.velocity.Y * 0.5f, 90, default(Color), 1.5f);
                Main.dust[iceDust].noGravity = true;
                Dust dust = Main.dust[iceDust];
                dust.velocity *= 0.2f;
                Main.dust[iceDust].noLight = true;
            }

            NPC.noGravity = true;

            // Collision on the X axis.
            if (NPC.collideX)
            {
                NPC.velocity.X = NPC.oldVelocity.X * -0.5f;
                if (NPC.direction == -1 && NPC.velocity.X > 0f && NPC.velocity.X < 2f)
                    NPC.velocity.X = 2f;
                if (NPC.direction == 1 && NPC.velocity.X < 0f && NPC.velocity.X > -2f)
                    NPC.velocity.X = -2f;
            }

            // Collision on Y axis.
            if (NPC.collideY)
            {
                NPC.velocity.Y = NPC.oldVelocity.Y * -0.5f;
                if (NPC.velocity.Y > 0f && NPC.velocity.Y < 1f)
                    NPC.velocity.Y = 1f;
                if (NPC.velocity.Y < 0f && NPC.velocity.Y > -1f)
                    NPC.velocity.Y = -1f;
            }

            if (NPC.type == NPCID.FlyingSnake)
            {
                int direction = 1;
                int directionY = 1;
                if (NPC.velocity.X < 0f)
                    direction = -1;
                if (NPC.velocity.Y < 0f)
                    directionY = -1;

                NPC.TargetClosest();

                if (!Collision.CanHit(NPC.position, NPC.width, NPC.height, Main.player[NPC.target].position, Main.player[NPC.target].width, Main.player[NPC.target].height))
                {
                    NPC.direction = direction;
                    NPC.directionY = directionY;
                }
            }
            else
                NPC.TargetClosest();

            float maxSpeedX = CalamityWorld.death ? 6f : CalamityWorld.revenge ? 5f : 4f;
            float maxSpeedY = CalamityWorld.death ? 2.5f : CalamityWorld.revenge ? 2f : 1.5f;

            float xAccel = CalamityWorld.revenge ? 0.12f : 0.1f;
            float xAccelBoost1 = CalamityWorld.revenge ? 0.12f : 0.1f;
            float xAccelBoost2 = CalamityWorld.revenge ? 0.06f : 0.04f;

            float yAccel = CalamityWorld.revenge ? 0.06f : 0.04f;
            float yAccelBoost1 = CalamityWorld.revenge ? 0.07f : 0.05f;
            float yAccelBoost2 = CalamityWorld.revenge ? 0.05f : 0.03f;

            if (NPC.type == NPCID.VampireBat)
            {
                if (NPC.position.Y < Main.worldSurface * 16.0 && Main.dayTime && !Main.eclipse)
                {
                    NPC.directionY = -1;
                    NPC.direction *= -1;
                }

                maxSpeedX = maxSpeedY = CalamityWorld.death ? 11f : 9f;
                xAccel = yAccel = 0.3f;
                xAccelBoost1 = yAccelBoost1 = 0.12f;
                xAccelBoost2 = yAccelBoost2 = 0.07f;
            }
            else if (NPC.type == NPCID.FlyingSnake)
            {
                maxSpeedX = CalamityWorld.death ? 9f : 6f;
                maxSpeedY = CalamityWorld.death ? 5f : 3.5f;

                xAccel = 0.3f;
                xAccelBoost1 = 0.12f;
                xAccelBoost2 = 0.07f;

                yAccel = 0.12f;
                yAccelBoost1 = 0.07f;
                yAccelBoost2 = 0.05f;
            }

            DemonEyeAI.DemonEyeBatMovement(NPC, maxSpeedX, maxSpeedY, xAccel, xAccelBoost1, xAccelBoost2, yAccel, yAccelBoost1, yAccelBoost2);

            if (NPC.type == NPCID.CaveBat ||
                NPC.type == NPCID.JungleBat ||
                NPC.type == NPCID.Hellbat ||
                NPC.type == NPCID.Demon ||
                NPC.type == NPCID.VoodooDemon ||
                NPC.type == NPCID.GiantBat ||
                NPC.type == NPCID.IlluminantBat ||
                NPC.type == NPCID.IceBat ||
                NPC.type == NPCID.Lavabat ||
                NPC.type == NPCID.GiantFlyingFox ||
                NPC.type == ModContent.NPCType<Melter>())
            {
                maxSpeedX = CalamityWorld.death ? 6f : CalamityWorld.revenge ? 5f : 4f;
                maxSpeedY = CalamityWorld.death ? 2.5f : CalamityWorld.revenge ? 2f : 1.5f;
                if (NPC.wet)
                {
                    if (NPC.velocity.Y > 0f)
                        NPC.velocity.Y *= 0.95f;

                    NPC.velocity.Y -= CalamityWorld.revenge ? 0.6f : 0.5f;
                    if (NPC.velocity.Y < -5f)
                        NPC.velocity.Y = -5f;

                    NPC.TargetClosest();
                }

                if (NPC.type == NPCID.Hellbat)
                {
                    xAccel = 0.12f;
                    xAccelBoost1 = 0.09f;
                    xAccelBoost2 = 0.05f;

                    yAccel = 0.06f;
                    yAccelBoost1 = 0.05f;
                    yAccelBoost2 = 0.03f;
                }
                else
                {
                    xAccel = CalamityWorld.revenge ? 0.12f : 0.1f;
                    xAccelBoost1 = CalamityWorld.revenge ? 0.12f : 0.1f;
                    xAccelBoost2 = CalamityWorld.revenge ? 0.07f : 0.05f;

                    yAccel = CalamityWorld.revenge ? 0.06f : 0.04f;
                    yAccelBoost1 = CalamityWorld.revenge ? 0.07f : 0.05f;
                    yAccelBoost2 = CalamityWorld.revenge ? 0.05f : 0.03f;
                }

                DemonEyeAI.DemonEyeBatMovement(NPC, maxSpeedX, maxSpeedY, xAccel, xAccelBoost1, xAccelBoost2, yAccel, yAccelBoost1, yAccelBoost2);
            }

            if (NPC.type == NPCID.Harpy && NPC.wet)
            {
                NPC.ai[0] = 0f;

                if (NPC.velocity.Y > 0f)
                    NPC.velocity.Y *= 0.95f;

                NPC.velocity.Y -= CalamityWorld.revenge ? 0.6f : 0.5f;
                if (NPC.velocity.Y < -5f)
                    NPC.velocity.Y = -5f;

                NPC.TargetClosest();
            }

            // Turn back into a walking bat when possible
            if (NPC.type == NPCID.VampireBat && Main.netMode != NetmodeID.MultiplayerClient)
            {
                if (NPC.Distance(Main.player[NPC.target].Center) < 200f &&
                    NPC.Center.Y < Main.player[NPC.target].Center.Y &&
                    Collision.CanHit(NPC.position, NPC.width, NPC.height, Main.player[NPC.target].position, Main.player[NPC.target].width, Main.player[NPC.target].height))
                {
                    NPC.Transform(NPCID.Vampire);
                }
            }

            NPC.ai[1] += CalamityWorld.revenge ? 2f : 1f;
            if (NPC.type == NPCID.VampireBat)
                NPC.ai[1] += 1f;

            if (NPC.ai[1] > 200f)
            {
                if (!Main.player[NPC.target].wet && Collision.CanHit(NPC.position, NPC.width, NPC.height, Main.player[NPC.target].position, Main.player[NPC.target].width, Main.player[NPC.target].height))
                    NPC.ai[1] = 0f;

                xAccel = CalamityWorld.revenge ? 0.25f : 0.2f;
                yAccel = CalamityWorld.revenge ? 0.12f : 0.1f;

                float maxVelocityX = CalamityWorld.revenge ? 4.6f : 4f;
                float maxVelocityY = CalamityWorld.revenge ? 1.8f : 1.5f;

                if (NPC.type == NPCID.Harpy || NPC.type == NPCID.Demon || NPC.type == NPCID.VoodooDemon)
                {
                    xAccel = CalamityWorld.revenge ? 0.15f : 0.12f;
                    yAccel = CalamityWorld.revenge ? 0.1f : 0.07f;
                    maxVelocityX = CalamityWorld.revenge ? 3.5f : 3f;
                    maxVelocityY = CalamityWorld.revenge ? 1.5f : 1.25f;
                }

                if (NPC.ai[1] > 1000f)
                    NPC.ai[1] = 0f;

                NPC.ai[2] += 1f;
                if (NPC.ai[2] > 0f)
                {
                    if (NPC.velocity.Y < maxVelocityY)
                        NPC.velocity.Y += yAccel;
                }
                else if (NPC.velocity.Y > -maxVelocityY)
                    NPC.velocity.Y -= yAccel;

                if (NPC.ai[2] < -150f || NPC.ai[2] > 150f)
                {
                    if (NPC.velocity.X < maxVelocityX)
                        NPC.velocity.X += xAccel;
                }
                else if (NPC.velocity.X > -maxVelocityX)
                    NPC.velocity.X -= xAccel;

                if (NPC.ai[2] > 300f)
                    NPC.ai[2] = -300f;
            }

            if (NPC.type == NPCID.Harpy)
            {
                // Emit feather dust from center when about to shoot
                if (NPC.ai[0] > HarpyFeatherGateValue - HarpyFeatherTelegraphTime)
                {
                    Dust dust = Dust.NewDustDirect(NPC.Center + Main.rand.NextVector2CircularEdge(5f, 5f), 1, 1, DustID.DungeonWater, 0f, 0f, 0, default, 1.5f);
                    dust.noGravity = true;
                    dust.velocity *= 0f;
                }
            }
            else if (NPC.type == NPCID.Demon || NPC.type == NPCID.VoodooDemon)
            {
                // Emit shadowflame dust from center when about to shoot
                if (NPC.ai[0] > DemonScytheGateValue - DemonScytheTelegraphTime)
                {
                    Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Shadowflame, 0f, 0f, 100, default, 3f);
                    dust.noGravity = true;
                    dust.velocity *= 0f;
                }
            }
            else if (NPC.type == NPCID.RedDevil)
            {
                // Emit shadowflame dust from center when about to shoot
                if (NPC.ai[0] > RedDevilTridentGateValue - RedDevilTridentTelegraphTime)
                {
                    Dust dust = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Shadowflame, 0f, 0f, 100, default, 3f);
                    dust.noGravity = true;
                    dust.velocity *= 0f;
                }
            }

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                if (NPC.type == NPCID.Harpy)
                {
                    float featherShootCutOffValue = CalamityWorld.revenge ? 90f : 60f;
                    if (NPC.justHit || !Collision.CanHit(NPC.position, NPC.width, NPC.height, Main.player[NPC.target].position, Main.player[NPC.target].width, Main.player[NPC.target].height))
                        NPC.ai[0] = featherShootCutOffValue + 1f;

                    if (NPC.ai[0] >= HarpyFeatherGateValue)
                    {
                        NPC.ai[0] = 0f;
                    }
                    else if (NPC.ai[0] % 30f == 0f && NPC.ai[0] <= featherShootCutOffValue)
                    {
                        NPC.ai[0] += 1f;
                        if (Collision.CanHit(NPC.position, NPC.width, NPC.height, Main.player[NPC.target].position, Main.player[NPC.target].width, Main.player[NPC.target].height))
                        {
                            int damage = 15;
                            int type = ProjectileID.HarpyFeather;
                            Vector2 featherVelocity = NPC.SafeDirectionTo(Main.player[NPC.target].Center) * (CalamityWorld.death ? 4f : 6f);

                            int feather = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, featherVelocity, type, damage, 0f, Main.myPlayer);
                            Main.projectile[feather].timeLeft = 300;
                            if (CalamityWorld.death)
                            {
                                Main.projectile[feather].extraUpdates += 1;
                                Main.projectile[feather].timeLeft = 600;
                            }
                        }
                    }
                    else
                        NPC.ai[0] += 1f;
                }

                if (NPC.type == NPCID.Demon || NPC.type == NPCID.VoodooDemon)
                {
                    float scytheShootCutOffValue = CalamityWorld.revenge ? 80f : 60f;
                    if (NPC.justHit || !Collision.CanHit(NPC.position, NPC.width, NPC.height, Main.player[NPC.target].position, Main.player[NPC.target].width, Main.player[NPC.target].height))
                        NPC.ai[0] = scytheShootCutOffValue + 1f;

                    if (NPC.ai[0] >= DemonScytheGateValue)
                    {
                        NPC.ai[0] = 0f;
                    }
                    else if (NPC.ai[0] % 20f == 0f && NPC.ai[0] <= scytheShootCutOffValue)
                    {
                        NPC.ai[0] += 1f;
                        if (Collision.CanHit(NPC.position, NPC.width, NPC.height, Main.player[NPC.target].position, Main.player[NPC.target].width, Main.player[NPC.target].height))
                        {
                            int damage = 21;
                            int type = ProjectileID.DemonSickle;
                            Vector2 sickleVelocity = NPC.SafeDirectionTo(Main.player[NPC.target].Center) * (CalamityWorld.death ? 0.15f : 0.2f);

                            int sickle = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, sickleVelocity, type, damage, 0f, Main.myPlayer);
                            Main.projectile[sickle].timeLeft = 300;
                            if (CalamityWorld.death)
                            {
                                Main.projectile[sickle].extraUpdates += 1;
                                Main.projectile[sickle].timeLeft = 600;
                            }
                        }
                    }
                    else
                        NPC.ai[0] += 1f;
                }

                if (NPC.type == NPCID.RedDevil)
                {
                    float tridentShootCutOffValue = 80f;
                    if (NPC.justHit || !Collision.CanHit(NPC.position, NPC.width, NPC.height, Main.player[NPC.target].position, Main.player[NPC.target].width, Main.player[NPC.target].height))
                        NPC.ai[0] = tridentShootCutOffValue + 1f;

                    if (NPC.ai[0] >= RedDevilTridentGateValue)
                    {
                        NPC.ai[0] = 0f;
                    }
                    else if (NPC.ai[0] % 20f == 0f && NPC.ai[0] <= tridentShootCutOffValue)
                    {
                        NPC.ai[0] += 1f;
                        if (Collision.CanHit(NPC.position, NPC.width, NPC.height, Main.player[NPC.target].position, Main.player[NPC.target].width, Main.player[NPC.target].height))
                        {
                            Vector2 spawnPosition = NPC.Center;

                            float tridentSpeed = CalamityWorld.death ? 0.15f : 0.2f;
                            Vector2 tridentVelocity = NPC.SafeDirectionTo(Main.player[NPC.target].Center) * tridentSpeed;
                            spawnPosition += NPC.velocity * 5f;

                            int damage = 80;
                            int type = ProjectileID.UnholyTridentHostile;
                            int trident = Projectile.NewProjectile(NPC.GetSource_FromAI(), spawnPosition + tridentVelocity * 100f, tridentVelocity, type, damage, 3f, Main.myPlayer);
                            Main.projectile[trident].timeLeft = 300;
                            if (CalamityWorld.death)
                            {
                                Main.projectile[trident].extraUpdates += 1;
                                Main.projectile[trident].timeLeft = 600;
                            }
                        }
                    }
                    else
                        NPC.ai[0] += 1f;
                }
            }
            return false;
        }
    }
}
