using CalamityMod.World;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.NPCs.VanillaNPCAIOverrides.RegularEnemies;

public static partial class RevengeanceAndDeathAI
{
    public class SpikeBallAI : VanillaAIOverride
    {
        public override bool AI(Mod mod)
        {
            if (NPC.ai[0] == 0f)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    NPC.TargetClosest(true);
                    NPC.direction *= -1;
                    NPC.directionY *= -1;
                    NPC.position.Y = NPC.position.Y + (float)(NPC.height / 2 + 8);
                    NPC.ai[1] = NPC.position.X + (float)(NPC.width / 2);
                    NPC.ai[2] = NPC.position.Y + (float)(NPC.height / 2);
                    if (NPC.direction == 0)
                    {
                        NPC.direction = 1;
                    }
                    if (NPC.directionY == 0)
                    {
                        NPC.directionY = 1;
                    }
                    NPC.ai[3] = 1f + (float)Main.rand.Next(15) * 0.1f;
                    NPC.velocity.Y = (float)(NPC.directionY * 6) * NPC.ai[3];
                    NPC.ai[0] += 1f;
                    NPC.netUpdate = true;
                    return false;
                }
                NPC.ai[1] = NPC.position.X + (float)(NPC.width / 2);
                NPC.ai[2] = NPC.position.Y + (float)(NPC.height / 2);
            }
            else
            {
                float maxSpinSpeed = (CalamityWorld.death ? 12f : 9f) * NPC.ai[3];
                float spinAcceleration = (CalamityWorld.death ? 0.4f : 0.3f) * NPC.ai[3];
                float timeToReachMaxSpeed = maxSpinSpeed / spinAcceleration / 2f;
                if (NPC.ai[0] >= 1f && NPC.ai[0] < (float)((int)timeToReachMaxSpeed))
                {
                    NPC.velocity.Y = (float)NPC.directionY * maxSpinSpeed;
                    NPC.ai[0] += 1f;
                    return false;
                }
                if (NPC.ai[0] >= (float)((int)timeToReachMaxSpeed))
                {
                    NPC.velocity.Y = 0f;
                    NPC.directionY *= -1;
                    NPC.velocity.X = maxSpinSpeed * (float)NPC.direction;
                    NPC.ai[0] = -1f;
                    return false;
                }
                if (NPC.directionY > 0)
                {
                    if (NPC.velocity.Y >= maxSpinSpeed)
                    {
                        NPC.directionY *= -1;
                        NPC.velocity.Y = maxSpinSpeed;
                    }
                }
                else if (NPC.directionY < 0 && NPC.velocity.Y <= -maxSpinSpeed)
                {
                    NPC.directionY *= -1;
                    NPC.velocity.Y = -maxSpinSpeed;
                }
                if (NPC.direction > 0)
                {
                    if (NPC.velocity.X >= maxSpinSpeed)
                    {
                        NPC.direction *= -1;
                        NPC.velocity.X = maxSpinSpeed;
                    }
                }
                else if (NPC.direction < 0 && NPC.velocity.X <= -maxSpinSpeed)
                {
                    NPC.direction *= -1;
                    NPC.velocity.X = -maxSpinSpeed;
                }
                NPC.velocity.X = NPC.velocity.X + spinAcceleration * (float)NPC.direction;
                NPC.velocity.Y = NPC.velocity.Y + spinAcceleration * (float)NPC.directionY;
            }
            return false;
        }
    }
}
