using System;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.NPCs.VanillaNPCAIOverrides.RegularEnemies;

public static partial class RevengeanceAndDeathAI
{
    public class JellyfishAI : VanillaAIOverride
    {
        public override bool AI(Mod mod)
        {
            // Avoid cheap bullshit
            float damagingVelocity = NPC.type == NPCID.GreenJellyfish ? 7.2f : 5.6f;
            NPC.damage = (NPC.dontTakeDamage || NPC.velocity.Length() > damagingVelocity) ? NPC.defDamage : 0;

            // Stop moving because we're emitting electricity and don't take damage
            bool endEarly = false;
            if (NPC.wet && NPC.ai[1] == 1f)
            {
                endEarly = true;
            }
            else
            {
                NPC.dontTakeDamage = false;
            }
            if (NPC.type == NPCID.BlueJellyfish || NPC.type == NPCID.PinkJellyfish || NPC.type == NPCID.GreenJellyfish || NPC.type == NPCID.BloodJelly)
            {
                if (NPC.wet)
                {
                    if (NPC.target >= 0 && Main.player[NPC.target].wet && !Main.player[NPC.target].dead && (Main.player[NPC.target].Center - NPC.Center).Length() < 200f)
                    {
                        if (NPC.ai[1] == 0f)
                        {
                            NPC.ai[2] += 2f;
                        }
                        else
                        {
                            NPC.ai[2] -= 0.25f;
                        }
                    }
                    if (endEarly)
                    {
                        NPC.dontTakeDamage = true;
                        NPC.ai[2] += 1f;
                        if (NPC.ai[2] >= 90f)
                        {
                            NPC.ai[1] = 0f;
                        }
                    }
                    else
                    {
                        NPC.ai[2] += 1f;
                        if (NPC.ai[2] >= 300f)
                        {
                            NPC.ai[1] = 1f;
                            NPC.ai[2] = 0f;
                        }
                    }
                }
                else
                {
                    NPC.ai[1] = 0f;
                    NPC.ai[2] = 0f;
                }
            }
            float lightIntensity = 1f;
            if (endEarly)
            {
                lightIntensity += 0.5f;
            }
            if (NPC.type == NPCID.BlueJellyfish)
            {
                Lighting.AddLight((int)(NPC.Center.X) / 16, (int)(NPC.Center.Y) / 16, 0.05f * lightIntensity, 0.15f * lightIntensity, 0.4f * lightIntensity);
            }
            else if (NPC.type == NPCID.GreenJellyfish)
            {
                Lighting.AddLight((int)(NPC.Center.X) / 16, (int)(NPC.Center.Y) / 16, 0.05f * lightIntensity, 0.45f * lightIntensity, 0.1f * lightIntensity);
            }
            else if (NPC.type != NPCID.Squid && NPC.type != NPCID.BloodJelly)
            {
                Lighting.AddLight((int)(NPC.Center.X) / 16, (int)(NPC.Center.Y) / 16, 0.35f * lightIntensity, 0.05f * lightIntensity, 0.2f * lightIntensity);
            }
            if (NPC.direction == 0)
            {
                NPC.TargetClosest(true);
            }
            if (endEarly)
            {
                return false;
            }
            if (!NPC.wet)
            {
                NPC.rotation += NPC.velocity.X * 0.1f;
                if (NPC.velocity.Y == 0f)
                {
                    NPC.velocity.X *= 0.98f;
                    if (Math.Abs(NPC.velocity.X) < 0.01f)
                    {
                        NPC.velocity.X = 0f;
                    }
                }
                NPC.velocity.Y += 0.2f;
                if (NPC.velocity.Y > 10f)
                {
                    NPC.velocity.Y = 10f;
                }
                NPC.ai[0] = 1f;
                return false;
            }
            // Collision
            // Turn around on X collision
            if (NPC.collideX)
            {
                NPC.velocity.X *= 1f;
                NPC.direction *= -1;
            }
            // Manipulate the sign of the Y velocity
            if (NPC.collideY)
            {
                if (NPC.velocity.Y > 0f)
                {
                    NPC.velocity.Y = Math.Abs(NPC.velocity.Y) * -1f;
                    NPC.directionY = -1;
                    NPC.ai[0] = -1f;
                }
                else if (NPC.velocity.Y < 0f)
                {
                    NPC.velocity.Y = Math.Abs(NPC.velocity.Y);
                    NPC.directionY = 1;
                    NPC.ai[0] = 1f;
                }
            }
            bool targetInWater = false;
            if (!NPC.friendly)
            {
                NPC.TargetClosest(false);
                if ((Main.player[NPC.target].wet || (CalamityWorld.death && NPC.Distance(Main.player[NPC.target].Center) < 400f)) && !Main.player[NPC.target].dead)
                {
                    targetInWater = true;
                }
            }
            // Slow down. When slow enough, charge again.
            if (targetInWater)
            {
                NPC.localAI[2] = 1f;
                NPC.rotation = NPC.velocity.ToRotation() + MathHelper.PiOver2;
                NPC.velocity *= 0.96f;
                float minimumSpeed = 0.2f;
                if (NPC.type == NPCID.GreenJellyfish)
                {
                    NPC.velocity *= 0.98f;
                    minimumSpeed = 0.6f;
                }
                if (NPC.type == NPCID.Squid)
                {
                    NPC.velocity *= 0.99f;
                    minimumSpeed = 1f;
                }
                if (NPC.type == NPCID.BloodJelly)
                {
                    NPC.velocity *= 0.995f;
                    minimumSpeed = 3f;
                }
                minimumSpeed *= 0.8f;
                if (NPC.velocity.Length() < minimumSpeed)
                {
                    if (NPC.type == NPCID.Squid)
                    {
                        NPC.localAI[0] = 1f;
                    }
                    NPC.TargetClosest(true);

                    float lungeSpeed = NPC.type == NPCID.GreenJellyfish ? 18f : 14f;
                    NPC.velocity = NPC.SafeDirectionTo(Main.player[NPC.target].Center, -Vector2.UnitY) * lungeSpeed;
                }
            }
            // General floating around.
            else
            {
                NPC.localAI[2] = 0f;
                NPC.velocity.X += NPC.direction * 0.02f;
                NPC.rotation = NPC.velocity.X * 0.4f;
                if (NPC.velocity.X < -1f || NPC.velocity.X > 1f)
                {
                    NPC.velocity.X *= 0.95f;
                }
                if (NPC.ai[0] == -1f)
                {
                    NPC.velocity.Y -= 0.01f;
                    if (NPC.velocity.Y < -1f)
                    {
                        NPC.ai[0] = 1f;
                    }
                }
                else
                {
                    NPC.velocity.Y += 0.01f;
                    if (NPC.velocity.Y > 1f)
                    {
                        NPC.ai[0] = -1f;
                    }
                }
                int x = (int)NPC.Center.X / 16;
                int y = (int)NPC.Center.Y / 16;
                if (Main.tile[x, y - 1].LiquidAmount > 128)
                {
                    if (Main.tile[x, y + 1].HasTile)
                    {
                        NPC.ai[0] = -1f;
                    }
                    else if (Main.tile[x, y + 2].HasTile)
                    {
                        NPC.ai[0] = -1f;
                    }
                }
                else
                {
                    NPC.ai[0] = 1f;
                }
                if (Math.Abs(NPC.velocity.Y) > 1.2)
                {
                    NPC.velocity.Y *= 0.99f;
                }
            }
            return false;
        }
    }
}
