using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.NPCs.VanillaNPCAIOverrides.MiniBosses
{
    public class MothronAI : VanillaAIOverride
    {
        internal enum MothronAIState
        {
            DespawnYeet = -1,
            NewAISelection = 0,
            FlyTowardsPlayer = 1,
            AccelerateTowardsPlayer = 2,
            ChargeRedirect = 3,
            ChargePreparation = 4,
            DoTheFuckingCharge = 5,
            PickSpotToLayEgg = 6,
            FlyToEggSpot = 7,
            LayEgg = 8
        }

        public override bool AI(Mod mod)
        {
            NPC.noTileCollide = false;
            NPC.noGravity = true;
            NPC.knockBackResist = 0f;

            ref float aiState = ref NPC.ai[0];

            // Percent life remaining
            float lifeRatio = NPC.life / (float)NPC.lifeMax;

            // Phases
            bool phase2 = lifeRatio < 0.4f;
            bool phase3 = lifeRatio < 0.1f;

            Player target = Main.player[NPC.target];

            // Despawn if no valid target exists or there's no ongoing eclipse.
            if (!Main.eclipse)
                aiState = (int)MothronAIState.DespawnYeet;
            else if (NPC.target < 0 || target.dead || !target.active)
            {
                NPC.TargetClosest(true);
                if (target.dead)
                {
                    aiState = (int)MothronAIState.DespawnYeet;
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                        NPC.netUpdate = true;
                }
            }

            float flyInertia;
            float chargeSpeed = 32f;
            Vector2 idealFlyVelocity;
            switch ((MothronAIState)(int)aiState)
            {
                case MothronAIState.DespawnYeet:
                    NPC.damage = 0;
                    Vector2 idealVelocity = Vector2.UnitY * -34f;
                    NPC.velocity = (NPC.velocity * 4f + idealVelocity) / 5f;
                    NPC.noTileCollide = true;
                    NPC.dontTakeDamage = true;
                    return false;

                case MothronAIState.NewAISelection:
                    NPC.damage = 0;

                    ref float aiTimer = ref NPC.ai[1];

                    NPC.TargetClosest(true);

                    if (NPC.Center.X < target.Center.X - 2f)
                        NPC.direction = 1;
                    if (NPC.Center.X > target.Center.X + 2f)
                        NPC.direction = -1;

                    NPC.spriteDirection = NPC.direction;
                    NPC.rotation = (NPC.rotation * 9f + NPC.velocity.X * 0.025f) / 10f;

                    // Rebounding on tile collision.
                    if (NPC.collideX)
                    {
                        NPC.velocity.X *= -NPC.oldVelocity.X * 0.5f;
                        NPC.velocity.X = MathHelper.Clamp(NPC.velocity.X, -4f, 4f);
                    }
                    if (NPC.collideY)
                    {
                        NPC.velocity.Y *= -NPC.oldVelocity.Y * 0.5f;
                        NPC.velocity.Y = MathHelper.Clamp(NPC.velocity.Y, -4f, 4f);
                    }

                    Vector2 destinationAboveTarget = target.Center - Vector2.UnitY * 200f;
                    float distanceFromAboveTarget = NPC.Distance(destinationAboveTarget);
                    if (distanceFromAboveTarget > 3000f)
                    {
                        aiState = (int)MothronAIState.FlyTowardsPlayer;
                        aiTimer = 0f;
                        NPC.ai[2] = 0f;
                        NPC.ai[3] = 0f;
                        NPC.netUpdate = true;
                    }

                    // Otherwise fly towards the destination if relatively far from it.
                    else if (distanceFromAboveTarget > 600f)
                    {
                        flyInertia = 30f;
                        idealFlyVelocity = NPC.SafeDirectionTo(destinationAboveTarget, -Vector2.UnitY) * 15f;
                        NPC.velocity = (NPC.velocity * (flyInertia - 1f) + idealFlyVelocity) / flyInertia;
                    }

                    // And otherwise, if near the destination, slow down a bit.
                    else if (NPC.velocity.Length() > 2f)
                        NPC.velocity *= 0.95f;
                    else if (NPC.velocity.Length() < 1f)
                        NPC.velocity *= 1.05f;

                    aiTimer++;

                    // Select a new AI state after 10 frames.
                    if (aiTimer >= 10f && Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        aiTimer = 0f;
                        NPC.ai[2] = 0f;
                        NPC.ai[3] = 0f;

                        while ((MothronAIState)(int)aiState == MothronAIState.NewAISelection)
                        {
                            int selection = Main.rand.Next(3);
                            if (phase3)
                                selection = 1;
                            else if (phase2)
                                selection = Main.rand.Next(2);

                            if (selection == 0 && Collision.CanHit(NPC.Center, 1, 1, target.Center, 1, 1))
                                aiState = (int)MothronAIState.AccelerateTowardsPlayer;
                            else if (selection == 1)
                                aiState = (int)MothronAIState.ChargeRedirect;
                            else if (selection == 2 && NPC.CountNPCS(NPCID.MothronEgg) + NPC.CountNPCS(NPCID.MothronSpawn) < 2)
                                aiState = (int)MothronAIState.PickSpotToLayEgg;
                        }
                        NPC.ForceNetUpdate();
                    }
                    break;

                case MothronAIState.FlyTowardsPlayer:
                    NPC.damage = 0;

                    NPC.collideX = false;
                    NPC.collideY = false;
                    NPC.noTileCollide = true;

                    if (NPC.target < 0 || !target.active || target.dead)
                        NPC.TargetClosest(true);

                    NPC.spriteDirection = NPC.direction = (NPC.velocity.X > 0).ToDirectionInt();
                    NPC.rotation = (NPC.rotation * 9f + NPC.velocity.X * 0.02f) / 10f;

                    // Don't bother flying anymore if we're stuck and the target is somewhat close.
                    if (NPC.WithinRange(target.Center, 500f) && !Collision.SolidCollision(NPC.position, NPC.width, NPC.height))
                    {
                        aiState = (int)MothronAIState.NewAISelection;
                        NPC.ai[1] = 0f;
                        NPC.ai[2] = 0f;
                        NPC.ai[3] = 0f;
                        NPC.netUpdate = true;
                    }

                    float flySpeed = 18f + NPC.Distance(target.Center) / 100f;
                    flyInertia = 25f;
                    idealFlyVelocity = NPC.SafeDirectionTo(target.Center, -Vector2.UnitY) * flySpeed;
                    NPC.velocity = (NPC.velocity * (flyInertia - 1f) + idealFlyVelocity) / flyInertia;
                    break;

                case MothronAIState.AccelerateTowardsPlayer:
                    NPC.damage = (int)Math.Round(NPC.defDamage * 0.5);

                    aiTimer = ref NPC.ai[1];
                    ref float flySpeedAdditive = ref NPC.ai[2];

                    // If no valid target exists, try to find a new one and select a new attack.
                    if (NPC.target < 0 || !target.active || target.dead)
                    {
                        NPC.TargetClosest(true);
                        aiState = (int)MothronAIState.NewAISelection;
                        aiTimer = 0f;
                        flySpeedAdditive = 0f;
                        NPC.ai[3] = 0f;
                        NPC.netUpdate = true;
                    }

                    NPC.spriteDirection = NPC.direction = (NPC.velocity.X > 0).ToDirectionInt();
                    NPC.rotation = (NPC.rotation * 4f + NPC.velocity.X * 0.025f) / 5f;

                    // Rebounding on tile collision.
                    if (NPC.collideX)
                    {
                        NPC.velocity.X *= -NPC.oldVelocity.X * 0.5f;
                        NPC.velocity.X = MathHelper.Clamp(NPC.velocity.X, -4f, 4f);
                    }
                    if (NPC.collideY)
                    {
                        NPC.velocity.Y *= -NPC.oldVelocity.Y * 0.5f;
                        NPC.velocity.Y = MathHelper.Clamp(NPC.velocity.Y, -4f, 4f);
                    }

                    Vector2 destination = target.Center - Vector2.UnitY * 20f;

                    flySpeedAdditive += 0.0222222228f;
                    if (Main.expertMode)
                        flySpeedAdditive += 0.0166666675f;

                    flySpeed = 12f + flySpeedAdditive + NPC.Distance(destination) / 120f;
                    flyInertia = 20f;
                    idealFlyVelocity = NPC.SafeDirectionTo(destination, -Vector2.UnitY) * flySpeed;
                    NPC.velocity = (NPC.velocity * (flyInertia - 1f) + idealFlyVelocity) / flyInertia;

                    aiTimer++;
                    // Stop flying if there's an obstacle between the npc and target.
                    if (aiTimer >= 120f || !Collision.CanHit(NPC.Center, 1, 1, target.Center, 1, 1))
                    {
                        aiState = (int)MothronAIState.NewAISelection;
                        aiTimer = 0f;
                        flySpeedAdditive = 0f;
                        NPC.ai[3] = 0f;
                        NPC.netUpdate = true;
                    }
                    break;

                case MothronAIState.ChargeRedirect:
                    NPC.damage = 0;

                    flySpeedAdditive = ref NPC.ai[2];
                    NPC.noTileCollide = true;

                    NPC.spriteDirection = NPC.direction = (NPC.velocity.X > 0).ToDirectionInt();
                    NPC.rotation = (NPC.rotation * 4f + NPC.velocity.X * 0.0175f) / 5f;

                    destination = target.Center;
                    destination -= Vector2.UnitY * 12f;

                    float xOffset = 600f;
                    if (NPC.Center.X > target.Center.X)
                        destination.X += xOffset;
                    else
                        destination.X -= xOffset;

                    // If close to the destination beside the player, enter the charge phase.
                    if (Main.netMode != NetmodeID.MultiplayerClient &&
                        Math.Abs(NPC.Center.X - target.Center.X) > xOffset - 50f && Math.Abs(NPC.Center.Y - target.Center.Y) < 20f)
                    {
                        aiState = (int)MothronAIState.ChargePreparation;
                        flySpeedAdditive = 0f;
                        NPC.ForceNetUpdate();
                    }

                    flySpeedAdditive += 0.0333333351f;
                    flySpeed = 24f + flySpeedAdditive;
                    flyInertia = 4f;
                    idealVelocity = NPC.SafeDirectionTo(destination, -Vector2.UnitY) * flySpeed;
                    NPC.velocity = (NPC.velocity * (flyInertia - 1f) + idealVelocity) / flyInertia;
                    break;

                case MothronAIState.ChargePreparation:
                    NPC.damage = 0;

                    aiTimer = ref NPC.ai[1];
                    ref float chargeDirection = ref NPC.ai[2];

                    NPC.noTileCollide = true;
                    NPC.rotation = (NPC.rotation * 4f + NPC.velocity.X * 0.0175f) / 5f;

                    destination = target.Center - Vector2.UnitY * 12f;

                    float chargePreperationInertia = 8f;
                    Vector2 chargeVelocity = NPC.SafeDirectionTo(destination, -Vector2.UnitY) * chargeSpeed;
                    NPC.velocity = (NPC.velocity * (chargePreperationInertia - 1f) + chargeVelocity) / chargePreperationInertia;
                    NPC.spriteDirection = NPC.direction = (NPC.velocity.X > 0).ToDirectionInt();

                    // Redirect for 10 frames. After that time has been spent, immediately charge as usual.
                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        aiTimer++;
                        if (aiTimer > 10f)
                        {
                            NPC.velocity = chargeVelocity;

                            if (NPC.velocity.X < 0f)
                                NPC.direction = -1;
                            else
                                NPC.direction = 1;

                            aiState = (int)MothronAIState.DoTheFuckingCharge;
                            chargeDirection = NPC.direction;
                            NPC.ForceNetUpdate();
                        }
                    }
                    break;

                case MothronAIState.DoTheFuckingCharge:
                    chargeDirection = ref NPC.ai[2];
                    flySpeedAdditive = ref NPC.ai[3];

                    NPC.damage = (int)Math.Round(NPC.defDamage * 1.2);
                    NPC.collideX = false;
                    NPC.collideY = false;
                    NPC.noTileCollide = true;
                    flySpeedAdditive += 0.0333333351f;
                    NPC.velocity.X = (chargeSpeed + flySpeedAdditive) * chargeDirection;

                    float chargeDistance = 460f;
                    if (Main.netMode != NetmodeID.MultiplayerClient &&
                        (chargeDirection > 0f && NPC.Center.X > target.Center.X + chargeDistance) ||
                        (chargeDirection < 0f && NPC.Center.X < target.Center.X - chargeDistance))
                    {
                        // If not stuck, pick a new attack.
                        if (!Collision.SolidCollision(NPC.position, NPC.width, NPC.height))
                        {
                            aiState = (int)MothronAIState.NewAISelection;
                            chargeDirection = 0f;
                            flySpeedAdditive = 0f;
                            NPC.ForceNetUpdate();
                        }

                        // Otherwise, if somewhat horizontally far from the target, go to typical flying by default.
                        else if (Math.Abs(NPC.Center.X - target.Center.X) > chargeDistance * 2f - 120f)
                        {
                            aiState = (int)MothronAIState.FlyTowardsPlayer;
                            chargeDirection = 0f;
                            flySpeedAdditive = 0f;
                            NPC.ForceNetUpdate();
                        }
                    }
                    NPC.rotation = (NPC.rotation * 4f + NPC.velocity.X * 0.0175f) / 5f;
                    break;

                case MothronAIState.PickSpotToLayEgg:
                    NPC.damage = 0;

                    ref float laySpotPositionX = ref NPC.ai[2];
                    ref float laySpotPositionY = ref NPC.ai[3];
                    // Fallback if the spot selection fails.
                    NPC.TargetClosest(true);

                    if (Main.netMode != NetmodeID.MultiplayerClient)
                    {
                        aiState = (int)MothronAIState.NewAISelection;
                        laySpotPositionX = laySpotPositionY = -1f;

                        for (int i = 0; i < 1000; i++)
                        {
                            int potentialSpotX = (int)target.Center.X / 16;
                            int potentialSpotY = (int)target.Center.Y / 16;

                            // Become more open to positions to search the more failed tries that have been accumulated.
                            int checkAreaX = 30 + i / 50;
                            int checkAreaY = 20 + i / 75;

                            potentialSpotX += Main.rand.Next(-checkAreaX, checkAreaX + 1);
                            potentialSpotY += Main.rand.Next(-checkAreaY, checkAreaY + 1);

                            if (!WorldGen.SolidTile(potentialSpotX, potentialSpotY))
                            {
                                // Search downward until a solid tile is reached.
                                // Stop checking if the spot is below the world surface, to prevent potential infinite loops.
                                while (!WorldGen.SolidTile(potentialSpotX, potentialSpotY) && potentialSpotY < Main.worldSurface)
                                    potentialSpotY++;

                                // And ensure that the spot isn't too far away.
                                if (NPC.WithinRange(new Vector2(potentialSpotX, potentialSpotY).ToWorldCoordinates(), 1600f))
                                {
                                    aiState = (int)MothronAIState.FlyToEggSpot;
                                    laySpotPositionX = potentialSpotX;
                                    laySpotPositionY = potentialSpotY;
                                    break;
                                }
                            }
                        }
                        NPC.ForceNetUpdate();
                    }
                    break;

                case MothronAIState.FlyToEggSpot:
                    NPC.damage = 0;

                    NPC.spriteDirection = NPC.direction = (NPC.velocity.X > 0).ToDirectionInt();
                    NPC.rotation = (NPC.rotation * 9f + NPC.velocity.X * 0.025f) / 10f;
                    NPC.noTileCollide = true;

                    Vector2 spotToLayEgg = new Vector2(NPC.ai[2], NPC.ai[3]).ToWorldCoordinates(8f, -20f);
                    float distanceFromSpot = NPC.Distance(spotToLayEgg);
                    flySpeed = 12f + distanceFromSpot / 150f;

                    if (flySpeed > 20f)
                        flySpeed = 20f;

                    if (Main.netMode != NetmodeID.MultiplayerClient && distanceFromSpot < 10f)
                    {
                        aiState = (int)MothronAIState.LayEgg;
                        NPC.netUpdate = true;
                    }

                    flyInertia = 10f;
                    NPC.velocity = (NPC.velocity * (flyInertia - 1f) + NPC.SafeDirectionTo(spotToLayEgg, -Vector2.UnitY) * flySpeed) / flyInertia;
                    break;

                case MothronAIState.LayEgg:
                    NPC.damage = 0;

                    NPC.rotation = (NPC.rotation * 9f + NPC.velocity.X * 0.025f) / 10f;
                    NPC.noTileCollide = false;

                    spotToLayEgg = new Vector2(NPC.ai[2], NPC.ai[3]).ToWorldCoordinates(8f, -28f);
                    distanceFromSpot = NPC.Distance(spotToLayEgg);
                    float hoverSpeed = 4f;
                    float hoverInertia = 2f;

                    if (Main.netMode != NetmodeID.MultiplayerClient && distanceFromSpot < 44f)
                    {
                        ref float attackTimer = ref NPC.ai[1];
                        int eggLayTime = 20;
                        if (Main.expertMode)
                            eggLayTime = (int)(eggLayTime * 0.75);
                        int waitTime = eggLayTime;

                        attackTimer++;
                        if (attackTimer == eggLayTime)
                            NPC.NewNPC(NPC.GetSource_FromAI(), (int)spotToLayEgg.X, (int)spotToLayEgg.Y + 20, NPCID.MothronEgg, NPC.whoAmI, 0f, 0f, 0f, 0f, 255);
                        else if (attackTimer == eggLayTime + waitTime)
                        {
                            aiState = (int)MothronAIState.NewAISelection;
                            attackTimer = 0f;
                            NPC.ai[2] = NPC.ai[3] = 0f;

                            // Try to lay another egg at a 66% chance if the amount of eggs + spawns is not at the limit.
                            if (NPC.CountNPCS(NPCID.MothronEgg) + NPC.CountNPCS(NPCID.MothronSpawn) < 3 && !Main.rand.NextBool(3))
                                aiState = (int)MothronAIState.PickSpotToLayEgg;
                            else if (Collision.SolidCollision(NPC.position, NPC.width, NPC.height))
                                aiState = (int)MothronAIState.FlyTowardsPlayer;
                            NPC.netUpdate = true;
                        }
                    }

                    if (distanceFromSpot < hoverSpeed)
                        hoverSpeed = distanceFromSpot;

                    Vector2 hoverVelocity = NPC.SafeDirectionTo(spotToLayEgg) * hoverSpeed;
                    NPC.velocity = (NPC.velocity * (hoverInertia - 1f) + hoverVelocity) / hoverInertia;
                    if (NPC.velocity.HasNaNs())
                        NPC.velocity = Vector2.Zero;
                    break;
            }
            return false;
        }
    }
}
