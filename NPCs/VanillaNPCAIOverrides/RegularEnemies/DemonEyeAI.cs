using System.Collections.Generic;
using CalamityMod.NPCs.Crags;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.NPCs.VanillaNPCAIOverrides.RegularEnemies;

public class DemonEyeAI : VanillaAIOverride
{
    // Subtypes of enemies with this AI. Made for programmer convenience.
    public static List<int> NightTimeEnemies => new List<int>()
    {
        NPCID.DemonEye,
        NPCID.WanderingEye,
        NPCID.CataractEye,
        NPCID.SleepyEye,
        NPCID.DialatedEye,
        NPCID.GreenEye,
        NPCID.PurpleEye,
        NPCID.DemonEyeOwl,
        NPCID.DemonEyeSpaceship,
    };

    public static List<int> Pigrons => new List<int>()
    {
        NPCID.PigronCorruption,
        NPCID.PigronCrimson,
        NPCID.PigronHallow
    };

    public const int FadeThroughWallsDelay = 300;

    public static void DemonEyeBatMovement(NPC npc, float maxXSpeed = 6f, float maxYSpeed = 3.5f,
        float xAccel = 0.1f, float xAccelBoost1 = 0.06f, float xAccelBoost2 = 0.25f,
        float yAccel = 0.12f, float yAccelBoost1 = 0.07f, float yAccelBoost2 = 0.2f)
    {
        if (npc.direction == -1 && npc.velocity.X > -maxXSpeed)
        {
            npc.velocity.X -= xAccel;
            if (npc.velocity.X > maxXSpeed)
            {
                npc.velocity.X -= xAccelBoost1;
            }
            else if (npc.velocity.X > 0f)
            {
                npc.velocity.X -= xAccelBoost2;
            }
            if (npc.velocity.X < -maxXSpeed)
            {
                npc.velocity.X = -maxXSpeed;
            }
        }
        else if (npc.direction == 1 && npc.velocity.X < maxXSpeed)
        {
            npc.velocity.X += xAccel;
            if (npc.velocity.X < -maxXSpeed)
            {
                npc.velocity.X += xAccelBoost1;
            }
            else if (npc.velocity.X < 0f)
            {
                npc.velocity.X += xAccelBoost2;
            }
            if (npc.velocity.X > maxXSpeed)
            {
                npc.velocity.X = maxXSpeed;
            }
        }
        if (npc.directionY == -1 && npc.velocity.Y > -maxYSpeed)
        {
            npc.velocity.Y -= yAccel;
            if (npc.velocity.Y > maxYSpeed)
            {
                npc.velocity.Y -= yAccelBoost1;
            }
            else if (npc.velocity.Y > 0f)
            {
                npc.velocity.Y -= yAccelBoost2;
            }
            if (npc.velocity.Y < -maxYSpeed)
            {
                npc.velocity.Y = -maxYSpeed;
            }
        }
        else if (npc.directionY == 1 && npc.velocity.Y < maxYSpeed)
        {
            npc.velocity.Y += yAccel;
            if (npc.velocity.Y < -maxYSpeed)
            {
                npc.velocity.Y += yAccelBoost1;
            }
            else if (npc.velocity.Y < 0f)
            {
                npc.velocity.Y += yAccelBoost2;
            }
            if (npc.velocity.Y > maxYSpeed)
            {
                npc.velocity.Y = maxYSpeed;
            }
        }
    }

    public override bool AI(Mod mod)
    {
        // Randomly play pigron noises if the NPC is a pigron.
        if (Pigrons.Contains(NPC.type) && Main.rand.NextBool(1000))
            SoundEngine.PlaySound(SoundID.Zombie9, NPC.Center);

        // Disable gravity.
        NPC.noGravity = true;

        // Handle tile collision rebounds if applicable.
        if (!NPC.noTileCollide)
        {
            // Bounce off of tiles on the X axis.
            if (NPC.collideX)
            {
                NPC.velocity.X = NPC.oldVelocity.X * -0.5f;
                if (NPC.direction == -1 && NPC.velocity.X > 0f && NPC.velocity.X < 2f)
                    NPC.velocity.X = 2f;

                if (NPC.direction == 1 && NPC.velocity.X < 0f && NPC.velocity.X > -2f)
                {
                    NPC.velocity.X = -2f;
                }
            }
            // Bounce off of tiles on the Y axis.
            if (NPC.collideY)
            {
                NPC.velocity.Y = NPC.oldVelocity.Y * -0.5f;
                if (NPC.velocity.Y > 0f && NPC.velocity.Y < 1f)
                    NPC.velocity.Y = 1f;
                if (NPC.velocity.Y < 0f && NPC.velocity.Y > -1f)
                    NPC.velocity.Y = -1f;
            }
        }

        // If the NPC is supposed to only appear during the night, disappear if above-ground and it's daytime.
        if (Main.dayTime && NPC.position.Y <= Main.worldSurface * 16.0 && NightTimeEnemies.Contains(NPC.type))
        {
            if (NPC.timeLeft > 10)
                NPC.timeLeft = 10;

            // Adjust directions
            NPC.direction = (NPC.velocity.X > 0f).ToDirectionInt();
            NPC.directionY = (NPC.velocity.X > 0f).ToDirectionInt();
        }

        // Otherwise constantly search for the closest target.
        else
            NPC.TargetClosest();

        Player target = Main.player[NPC.target];
        ref float fadeThroughWallsTimer = ref NPC.ai[0];
        ref float fadeThroughWallsFlag = ref NPC.ai[1];

        // Fade away and go through walls if the NPC is a pigron or Calamity eye.
        if (Pigrons.Contains(NPC.type) || NPC.type == ModContent.NPCType<CalamityEye>())
        {
            // Stop going through walls if the target can be reached.
            if (Collision.CanHit(NPC.position, NPC.width, NPC.height, target.position, target.width, target.height))
            {
                if (fadeThroughWallsFlag != 0f && !Collision.SolidCollision(NPC.position, NPC.width, NPC.height))
                {
                    fadeThroughWallsFlag = 0f;
                    fadeThroughWallsTimer = 0f;
                    NPC.netUpdate = true;
                }
            }
            else if (fadeThroughWallsFlag == 0f)
                fadeThroughWallsTimer++;

            if (fadeThroughWallsTimer >= (CalamityWorld.death ? FadeThroughWallsDelay / 2 : FadeThroughWallsDelay))
            {
                fadeThroughWallsFlag = 1f;
                fadeThroughWallsTimer = 0f;
                NPC.netUpdate = true;
            }
            if (fadeThroughWallsFlag == 0f)
            {
                NPC.alpha = 0;
                NPC.noTileCollide = false;
            }
            else
            {
                NPC.wet = false;
                NPC.alpha = 200;
                NPC.noTileCollide = true;
            }

            NPC.rotation = NPC.velocity.Y * 0.1f * NPC.direction;
            NPC.TargetClosest(true);

            DemonEyeBatMovement(NPC);
        }
        else if (NPC.type == NPCID.TheHungryII)
        {
            NPC.TargetClosest(true);

            // Emit light for some reason.
            Lighting.AddLight((int)NPC.Center.X / 16, (int)NPC.Center.Y / 16, 0.3f, 0.2f, 0.1f);

            DemonEyeBatMovement(NPC, CalamityWorld.death ? 10f : 8f, CalamityWorld.death ? 5f : 3.5f, 0.12f, 0.12f, 0.25f, 0.06f, 0.07f, 0.2f);
            if (Main.rand.NextBool(40))
            {
                Vector2 dustSpawnTopLeft = new Vector2(NPC.position.X, NPC.position.Y + NPC.height * 0.25f);
                Dust blood = Dust.NewDustDirect(dustSpawnTopLeft, NPC.width, NPC.height / 2, DustID.Blood, NPC.velocity.X, 2f, 0, default, 1f);
                blood.velocity.X *= 0.5f;
                blood.velocity.Y *= 0.1f;
            }
        }
        else if (NPC.type == NPCID.WanderingEye)
        {
            // Move faster when close to dying.
            if (NPC.life < NPC.lifeMax * 0.5)
                DemonEyeBatMovement(NPC, CalamityWorld.death ? 10f : 8f, CalamityWorld.death ? 8f : 6f, 0.12f, 0.12f, 0.07f, 0.12f, 0.12f, 0.07f);
            else
                DemonEyeBatMovement(NPC, CalamityWorld.death ? 8f : 6f, CalamityWorld.death ? 4f : 2.5f, 0.12f, 0.12f, 0.07f, 0.06f, 0.07f, 0.05f);
        }
        else
        {
            float maxSpeedX = CalamityWorld.death ? 6f : 5f;
            float maxSpeedY = CalamityWorld.death ? 2.5f : 2f;
            maxSpeedX *= 1f + (1f - NPC.scale);
            maxSpeedY *= 1f + (1f - NPC.scale);
            DemonEyeBatMovement(NPC, maxSpeedX, maxSpeedY, 0.08f, 0.08f, 0.03f, 0.02f, 0.03f, 0.015f);
        }

        // Make actual eyes emit blood randomly
        if (NPC.type == NPCID.DemonEye ||
             NPC.type == NPCID.WanderingEye ||
             NPC.type == ModContent.NPCType<CalamityEye>() ||
             (NPC.type >= NPCID.CataractEye && NPC.type <= NPCID.PurpleEye))
        {
            if (Main.rand.NextBool(40))
            {
                Vector2 dustSpawnTopLeft = new Vector2(NPC.position.X, NPC.position.Y + NPC.height * 0.25f);
                Dust blood = Dust.NewDustDirect(dustSpawnTopLeft, NPC.width, NPC.height / 2, DustID.Blood, NPC.velocity.X, 2f, 0, default, 1f);
                blood.velocity.X *= 0.5f;
                blood.velocity.Y *= 0.1f;
            }
        }

        // Avoid entering water. This does not apply to pigrons.
        if (NPC.wet && !Pigrons.Contains(NPC.type))
        {
            if (NPC.velocity.Y > 0f)
                NPC.velocity.Y *= 0.95f;

            NPC.velocity.Y -= 0.6f;
            if (NPC.velocity.Y < -5f)
                NPC.velocity.Y = -5f;

            NPC.TargetClosest(true);
        }
        return false;
    }
}
