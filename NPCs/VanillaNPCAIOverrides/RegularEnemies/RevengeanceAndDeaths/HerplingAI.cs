using System;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.NPCs.VanillaNPCAIOverrides.RegularEnemies;

public static partial class RevengeanceAndDeathAI
{
    public class HerplingAI : VanillaAIOverride
    {
        public override bool AI(Mod mod)
        {
            if (NPC.ai[2] > 1f)
            {
                NPC.ai[2] -= 1f;
            }

            if (NPC.ai[2] == 0f)
            {
                NPC.ai[0] = -100f;
                NPC.ai[2] = 1f;
                NPC.TargetClosest(true);
                NPC.spriteDirection = NPC.direction;
            }

            if (NPC.type == NPCID.ChatteringTeethBomb)
            {
                Vector2 dustOffset = new Vector2(-6f, -10f);
                dustOffset.X *= NPC.spriteDirection;
                if (NPC.ai[1] != 5f && Main.rand.NextBool(3))
                {
                    NPC.position += NPC.netOffset;
                    int dustID = Dust.NewDust(NPC.Center + dustOffset - Vector2.One * 5f, 4, 4, DustID.Torch);
                    Dust dust = Main.dust[dustID];
                    dust.scale = 1.5f;
                    dust.noGravity = true;
                    dust.velocity = dust.velocity * 0.25f + Vector2.Normalize(dustOffset) * 1f;
                    dust.velocity = dust.velocity.RotatedBy(-(float)Math.PI / 2f * (float)NPC.direction);
                    NPC.position -= NPC.netOffset;
                }
                if (NPC.ai[1] == 5f)
                {
                    NPC.velocity = Vector2.Zero;
                    NPC.position.X += NPC.width / 2;
                    NPC.position.Y += NPC.height / 2;
                    NPC.width = 160;
                    NPC.height = 160;
                    NPC.position.X -= NPC.width / 2;
                    NPC.position.Y -= NPC.height / 2;
                    NPC.dontTakeDamage = true;
                    NPC.position += NPC.netOffset;
                    if (NPC.ai[2] > 7f)
                    {
                        for (int i = 0; i < 8; i++)
                        {
                            Dust.NewDust(new Vector2(NPC.position.X, NPC.position.Y), NPC.width, NPC.height, DustID.Smoke, 0f, 0f, 100, default(Color), 1.5f);
                        }
                        for (int i = 0; i < 32; i++)
                        {
                            int dustID = Dust.NewDust(new Vector2(NPC.position.X, NPC.position.Y), NPC.width, NPC.height, DustID.Torch, 0f, 0f, 100, default(Color), 2.5f);
                            Dust dust = Main.dust[dustID];
                            dust.velocity *= 3f;
                            dust.noGravity = true;
                            dustID = Dust.NewDust(new Vector2(NPC.position.X, NPC.position.Y), NPC.width, NPC.height, DustID.Torch, 0f, 0f, 100, default(Color), 1.5f);
                            dust = Main.dust[dustID];
                            dust.velocity *= 2f;
                            dust.noGravity = true;
                        }
                        for (int i = 0; i < 2; i++)
                        {
                            int goreID = Gore.NewGore(NPC.GetSource_FromThis(), NPC.position + new Vector2((float)(NPC.width * Main.rand.Next(100)) / 100f, (float)(NPC.height * Main.rand.Next(100)) / 100f) - Vector2.One * 10f, default(Vector2), Main.rand.Next(GoreID.Smoke1, GoreID.Smoke3 + 1));
                            Gore gore = Main.gore[goreID];
                            gore.velocity *= 0.3f;
                            gore.velocity.X += (float)Main.rand.Next(-10, 11) * 0.05f;
                            gore.velocity.Y += (float)Main.rand.Next(-10, 11) * 0.05f;
                        }
                        if (NPC.ai[2] == 9f)
                        {
                            SoundEngine.PlaySound(SoundID.Item14, NPC.position);
                        }
                    }
                    if (NPC.ai[2] == 1f)
                    {
                        NPC.life = -1;
                        NPC.HitEffect();
                        NPC.active = false;
                    }
                    NPC.position -= NPC.netOffset;
                    return false;
                }
            }

            if (NPC.type == NPCID.ChatteringTeethBomb && NPC.ai[1] != 5f)
            {
                if (NPC.wet || Vector2.Distance(NPC.Center, Main.player[NPC.target].Center) < 64f)
                {
                    NPC.ai[1] = 5f;
                    NPC.ai[2] = 10f;
                    NPC.netUpdate = true;
                    return false;
                }
            }
            else if (NPC.wet && NPC.type != NPCID.Derpling)
            {
                if (NPC.collideX)
                {
                    NPC.direction *= -1; // Fixed bug where herplings didn't change direction from left to right
                    NPC.spriteDirection = NPC.direction;
                }

                if (NPC.collideY)
                {
                    NPC.TargetClosest(true);

                    if (NPC.oldVelocity.Y < 0f)
                    {
                        NPC.velocity.Y = 5f;
                    }
                    else
                    {
                        NPC.velocity.Y = NPC.velocity.Y - 2f;
                    }

                    NPC.spriteDirection = NPC.direction;
                }

                if (NPC.velocity.Y > 4f)
                {
                    NPC.velocity.Y = NPC.velocity.Y * 0.9f;
                }

                NPC.velocity.Y = NPC.velocity.Y - 0.45f;

                if (NPC.velocity.Y < -6f)
                {
                    NPC.velocity.Y = -6f;
                }
            }

            // Avoid cheap bullshit
            NPC.damage = (NPC.velocity.Y == 0f || NPC.velocity.Length() < 3f) ? 0 : NPC.defDamage;

            if (NPC.velocity.Y == 0f)
            {
                if (NPC.ai[3] == NPC.position.X)
                {
                    NPC.direction *= -1;
                    NPC.ai[2] = 300f;
                }

                NPC.ai[3] = 0f;

                NPC.velocity.X = NPC.velocity.X * 0.8f;
                if ((double)NPC.velocity.X > -0.1 && (double)NPC.velocity.X < 0.1)
                {
                    NPC.velocity.X = 0f;
                }

                if (NPC.type == NPCID.Derpling)
                {
                    NPC.ai[0] += 3f;
                }
                else
                {
                    NPC.ai[0] += 10f;
                }

                Vector2 herplingPosition = new Vector2(NPC.position.X + (float)NPC.width * 0.5f, NPC.position.Y + (float)NPC.height * 0.5f);
                float herplingTargetX = Main.player[NPC.target].position.X + (float)Main.player[NPC.target].width * 0.5f - herplingPosition.X;
                float herplingTargetY = Main.player[NPC.target].position.Y + (float)Main.player[NPC.target].height * 0.5f - herplingPosition.Y;
                float herplingTargetDist = (float)Math.Sqrt((double)(herplingTargetX * herplingTargetX + herplingTargetY * herplingTargetY));
                float herplingJumpHeight = 400f / herplingTargetDist;

                if (NPC.type == NPCID.Derpling)
                {
                    herplingJumpHeight *= 5f;
                }
                else
                {
                    herplingJumpHeight *= 10f;
                }

                if (herplingJumpHeight > 30f)
                {
                    herplingJumpHeight = 30f;
                }

                NPC.ai[0] += (float)((int)herplingJumpHeight);

                if (NPC.ai[0] >= 0f)
                {
                    NPC.netUpdate = true;

                    if (NPC.ai[2] == 1f)
                    {
                        NPC.TargetClosest(true);
                    }

                    if (NPC.type == NPCID.Derpling)
                    {
                        if (NPC.ai[1] == 2f)
                        {
                            NPC.velocity.Y = -14f;
                            NPC.velocity.X = NPC.velocity.X + 3f * (float)NPC.direction;
                            if (herplingTargetDist < 350f && herplingTargetDist > 200f)
                            {
                                NPC.velocity.X = NPC.velocity.X + (float)NPC.direction;
                            }
                            NPC.ai[0] = CalamityWorld.death ? -100f : -200f;
                            NPC.ai[1] = 0f;
                            NPC.ai[3] = NPC.position.X;
                        }
                        else
                        {
                            NPC.velocity.Y = -10f;
                            NPC.velocity.X = NPC.velocity.X + (float)(5 * NPC.direction);
                            if (herplingTargetDist < 350f && herplingTargetDist > 200f)
                            {
                                NPC.velocity.X = NPC.velocity.X + (float)NPC.direction;
                            }
                            NPC.ai[0] = CalamityWorld.death ? -60f : -120f;
                            NPC.ai[1] += 1f;
                        }
                    }
                    else
                    {
                        if (NPC.type == NPCID.ChatteringTeethBomb)
                        {
                            SoundEngine.PlaySound(SoundID.Zombie124, NPC.position);
                        }
                        if (NPC.ai[1] == 3f)
                        {
                            NPC.velocity.Y = -9f;
                            NPC.velocity.X = NPC.velocity.X + (float)(2 * NPC.direction);
                            if (herplingTargetDist < 350f && herplingTargetDist > 200f)
                            {
                                NPC.velocity.X = NPC.velocity.X + (float)NPC.direction;
                            }
                            NPC.ai[0] = CalamityWorld.death ? -100f : -200f;
                            NPC.ai[1] = 0f;
                            NPC.ai[3] = NPC.position.X;
                        }
                        else
                        {
                            NPC.velocity.Y = -5f;
                            NPC.velocity.X = NPC.velocity.X + (float)(4 * NPC.direction);
                            if (herplingTargetDist < 350f && herplingTargetDist > 200f)
                            {
                                NPC.velocity.X = NPC.velocity.X + (float)NPC.direction;
                            }
                            NPC.ai[0] = CalamityWorld.death ? -60f : -120f;
                            NPC.ai[1] += 1f;
                        }
                    }
                }
                else if (NPC.ai[0] >= -30f)
                {
                    NPC.aiAction = 1;
                }

                NPC.spriteDirection = NPC.direction;

                return false;
            }

            if (NPC.target < Main.maxPlayers)
            {
                if (NPC.type == NPCID.Derpling)
                {
                    bool derplingDropOnTarget = false;
                    if (NPC.position.Y + (float)NPC.height < Main.player[NPC.target].position.Y && NPC.position.X + (float)NPC.width > Main.player[NPC.target].position.X && NPC.position.X < Main.player[NPC.target].position.X + (float)Main.player[NPC.target].width)
                    {
                        derplingDropOnTarget = true;
                        NPC.velocity.X = NPC.velocity.X * 0.9f;
                        if (NPC.velocity.Y < 0f)
                        {
                            NPC.velocity.Y = NPC.velocity.Y * 0.9f;
                            NPC.velocity.Y = NPC.velocity.Y + 0.15f;
                        }
                    }

                    if (!derplingDropOnTarget && ((NPC.direction == 1 && NPC.velocity.X < 4f) || (NPC.direction == -1 && NPC.velocity.X > -4f)))
                    {
                        if ((NPC.direction == -1 && (double)NPC.velocity.X < 0.1) || (NPC.direction == 1 && (double)NPC.velocity.X > -0.1))
                        {
                            NPC.velocity.X = NPC.velocity.X + 0.3f * (float)NPC.direction;
                            return false;
                        }
                        NPC.velocity.X = NPC.velocity.X * 0.9f;
                    }
                }
                else if ((NPC.direction == 1 && NPC.velocity.X < 3f) || (NPC.direction == -1 && NPC.velocity.X > -3f))
                {
                    if ((NPC.direction == -1 && (double)NPC.velocity.X < 0.1) || (NPC.direction == 1 && (double)NPC.velocity.X > -0.1))
                    {
                        NPC.velocity.X = NPC.velocity.X + 0.3f * (float)NPC.direction;
                        return false;
                    }
                    NPC.velocity.X = NPC.velocity.X * 0.9f;
                }
            }

            return false;
        }
    }
}
