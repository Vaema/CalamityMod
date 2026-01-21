using System;
using CalamityMod.NPCs.AcidRain;
using CalamityMod.NPCs.Crags;
using CalamityMod.NPCs.NormalNPCs;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.NPCs.VanillaNPCAIOverrides.RegularEnemies
{
    public class SlimeAI : VanillaAIOverride
    {
        public static void ChooseRandomItem(out int dropItem)
        {
            // Use a fallback of -1.
            dropItem = -1;

            switch (Main.rand.Next(4))
            {
                // Potions.
                case 0:
                    int rand = Main.rand.Next(7);
                    if (rand == 0)
                    {
                        dropItem = ItemID.SwiftnessPotion;
                    }
                    else if (rand == 1)
                    {
                        dropItem = ItemID.IronskinPotion;
                    }
                    else if (rand == 2)
                    {
                        dropItem = ItemID.SpelunkerPotion;
                    }
                    else if (rand == 3)
                    {
                        dropItem = ItemID.MiningPotion;
                    }
                    else if (Main.netMode != NetmodeID.SinglePlayer && Main.rand.NextBool())
                    {
                        dropItem = ItemID.WormholePotion;
                    }
                    else
                    {
                        dropItem = ItemID.RecallPotion;
                    }
                    break;

                // Misc Items.
                case 1:
                    switch (Main.rand.Next(4))
                    {
                        case 0:
                            dropItem = ItemID.Torch;
                            break;
                        case 1:
                            dropItem = ItemID.Bomb;
                            break;
                        case 2:
                            dropItem = ItemID.Rope;
                            break;
                        case 3:
                            dropItem = ItemID.Heart;
                            break;
                    }
                    break;

                // Ores.
                case 2:
                    if (Main.rand.NextBool())
                    {
                        dropItem = Main.rand.Next(ItemID.IronOre, ItemID.SilverOre + 1);
                    }
                    else
                    {
                        dropItem = Main.rand.Next(ItemID.TinOre, ItemID.PlatinumOre + 1);
                    }
                    break;

                // Coins.
                case 3:
                    dropItem = Main.rand.Next(ItemID.CopperCoin, ItemID.GoldCoin + 1);
                    break;
            }
        }

        public override bool AI(Mod mod)
        {
            bool isSpikedSlime = NPC.type == NPCID.SlimeSpiked || NPC.type == NPCID.SpikedIceSlime || NPC.type == NPCID.SpikedJungleSlime || NPC.type == ModContent.NPCType<CryoSlime>();
            bool isLavaSlime = NPC.type == NPCID.LavaSlime || NPC.type == ModContent.NPCType<InfernalCongealment>();
            bool canShootProjectile = NPC.type == NPCID.SpikedIceSlime || NPC.type == NPCID.SlimeSpiked || NPC.type == NPCID.SpikedJungleSlime;
            int projectileShootType = -1;
            float projectileShootSpeedFactor = 1f;

            if (NPC.type == NPCID.SpikedIceSlime)
                projectileShootType = ProjectileID.IceSpike;
            if (NPC.type == NPCID.SlimeSpiked)
                projectileShootType = ProjectileID.SpikedSlimeSpike;
            if (NPC.type == NPCID.SpikedJungleSlime)
            {
                projectileShootType = ProjectileID.JungleSpike;
                projectileShootSpeedFactor *= 0.6f;
            }

            ref float jumpDelay = ref NPC.ai[0];
            ref float dropItemID = ref NPC.ai[1];
            ref float targetResetCountdown = ref NPC.ai[2];
            ref float projectileShootCountdown = ref NPC.localAI[0];

            if (NPC.type == NPCID.BlueSlime && (dropItemID == 1f || dropItemID == 2f || dropItemID == 3f))
                dropItemID = -1f;

            // Determine what the slime holds, if anything. This does not apply to slimes that are have no money tp drop.
            if (NPC.type == NPCID.BlueSlime && dropItemID == 0f && Main.netMode != NetmodeID.MultiplayerClient && NPC.value > 0f)
            {
                dropItemID = -1f;

                if (Main.rand.NextBool(20))
                {
                    ChooseRandomItem(out int dropItem);
                    dropItemID = dropItem;
                    NPC.netUpdate = true;
                }
            }

            // Decide colors for rainbow slimes.
            if (NPC.type == NPCID.RainbowSlime)
            {
                Lighting.AddLight(NPC.Center / 16, Main.DiscoColor.ToVector3() * -1f);
                NPC.color.R = (byte)Main.DiscoR;
                NPC.color.G = (byte)Main.DiscoG;
                NPC.color.B = (byte)Main.DiscoB;
                NPC.color.A = 100;
                NPC.alpha = 175;
            }

            // Have corrupt slimes emit demonite dust.
            if (NPC.type == NPCID.CorruptSlime && Main.rand.NextBool(30))
            {
                Dust demonite = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Demonite, 0f, 0f, NPC.alpha, NPC.color, 1f);
                demonite.velocity *= 0.3f;
            }

            // Have ice slimes emit snow dust.
            if ((NPC.type == NPCID.IceSlime || NPC.type == NPCID.SpikedIceSlime) && Main.rand.NextBool(10))
            {
                Dust snow = Dust.NewDustDirect(NPC.position, NPC.width, NPC.height, DustID.Snow, 0f, 0f, 0, default, 1f);
                snow.noGravity = true;
                snow.velocity *= 0.1f;
            }

            if (isLavaSlime)
            {
                // Emit orange light.
                Lighting.AddLight((int)(NPC.Center.X / 16f), (int)(NPC.Center.Y / 16f), 1f, 0.3f, 0.1f);

                // And fire. dust.
                int idx = Dust.NewDust(NPC.position, NPC.width, NPC.height, DustID.Torch, NPC.velocity.X * 0.2f, NPC.velocity.Y * 0.2f, 100, default, 1.7f);
                Main.dust[idx].noGravity = true;
            }

            // Handle projectile shoot logic, if applicable.
            if (canShootProjectile)
            {
                // Decrement the projectile shoot countdown until it's ready.
                if (projectileShootCountdown > 0f)
                    projectileShootCountdown--;

                float distanceFromTarget = NPC.Distance(Main.player[NPC.target].Center);
                bool noTilesInWayOfTarget = Collision.CanHit(NPC.position, NPC.width, NPC.height, Main.player[NPC.target].position, Main.player[NPC.target].width, Main.player[NPC.target].height);

                // If not in water, the target can be hit by this NPC, and they're close and in a line of sight, release projectiles.
                if (!NPC.wet && !Main.player[NPC.target].npcTypeNoAggro[NPC.type] && noTilesInWayOfTarget && distanceFromTarget < 200f && NPC.velocity.Y == 0f)
                {
                    jumpDelay = -40f;
                    NPC.velocity.X *= 0.9f;

                    if (Main.netMode != NetmodeID.MultiplayerClient && projectileShootCountdown <= 0f)
                    {
                        var source = NPC.GetSource_FromAI();
                        if (distanceFromTarget < 120f)
                        {
                            for (int i = 0; i < 5; i++)
                            {
                                Vector2 spikeShootVelocity = new Vector2(i - 2, -4f);
                                spikeShootVelocity *= Main.rand.NextVector2Square(0.75f, 1.25f);
                                spikeShootVelocity.Normalize();
                                spikeShootVelocity *= Main.rand.NextFloat(3.5f, 4.5f) * projectileShootSpeedFactor;
                                int proj = Projectile.NewProjectile(source, NPC.Center, spikeShootVelocity, projectileShootType, 9, 0f, Main.myPlayer);
                                if (CalamityWorld.death)
                                    Main.projectile[proj].extraUpdates += 1;

                                projectileShootCountdown = 30f;
                            }
                        }
                        else
                        {
                            Vector2 velocity = NPC.SafeDirectionTo(Main.player[NPC.target].Center - Vector2.UnitY * 100f) * projectileShootSpeedFactor * (CalamityWorld.death ? 3.25f : CalamityWorld.revenge ? 5.5f : 4.5f);
                            int proj = Projectile.NewProjectile(source, NPC.Center, velocity, projectileShootType, 9, 0f, Main.myPlayer);
                            if (CalamityWorld.death)
                            {
                                Main.projectile[proj].extraUpdates += 1;
                                Main.projectile[proj].timeLeft = 1200;
                            }

                            projectileShootCountdown = 50f;
                        }
                    }
                }
                else
                    projectileShootCountdown = 50f;
            }

            // Decrement the target reset counter.
            if (targetResetCountdown > 1f)
                targetResetCountdown--;

            // Rise to the top of water.
            if (NPC.wet)
                DoWaterHoverBehavior(NPC, isLavaSlime, ref targetResetCountdown);

            NPC.aiAction = 0;

            // Initialize with short jumps.
            if (targetResetCountdown == 0f)
            {
                jumpDelay = -100f;
                targetResetCountdown = 1f;
                NPC.TargetClosest();
            }

            // Avoid cheap bullshit
            if (!isSpikedSlime)
                NPC.damage = (NPC.velocity.Y == 0f || NPC.velocity.Length() < 3f) ? 0 : NPC.defDamage;

            if (NPC.velocity.Y == 0f)
            {
                // Slide out of blocks if stuck.
                if (NPC.collideY && NPC.oldVelocity.Y != 0f && Collision.SolidCollision(NPC.position, NPC.width, NPC.height))
                    NPC.position.X -= NPC.velocity.X + NPC.direction;

                if (NPC.ai[3] == NPC.position.X)
                {
                    NPC.direction *= -1;
                    targetResetCountdown = 200f;
                }

                NPC.ai[3] = 0f;

                // Slow down horizontally until stopping.
                NPC.velocity.X *= 0.8f;
                if (Math.Abs(NPC.velocity.X) < 0.1f)
                    NPC.velocity.X = 0f;

                // Slimes jump more quickly overall when the slime rain event is ongoing.
                jumpDelay += (Main.slimeRain ? 4f : 3f) * (CalamityWorld.death ? 2f : 1f);

                if (NPC.type == NPCID.HoppinJack || NPC.type == NPCID.GoldenSlime)
                    jumpDelay += 10f;

                if (isLavaSlime)
                    jumpDelay += 2f;

                if (NPC.type == NPCID.DungeonSlime || NPC.type == ModContent.NPCType<CryoSlime>() || NPC.type == ModContent.NPCType<CrimulanBlightSlime>() ||
                    NPC.type == ModContent.NPCType<EbonianBlightSlime>())
                {
                    jumpDelay += 3f;
                }

                if (NPC.type == NPCID.RainbowSlime)
                    jumpDelay += 2f;

                if (NPC.type == NPCID.IlluminantSlime)
                    jumpDelay += 2f;

                if (NPC.type == NPCID.Crimslime)
                    jumpDelay += 1f;

                // The fuck? This is from vanilla, presumably. I'll leave it alone in the event that it's some dumb spaghetti.
                if (NPC.type == NPCID.CorruptSlime)
                    jumpDelay += NPC.scale >= 0f ? 4f : 1f;

                int jumpType = 0;
                if (jumpDelay >= 0f)
                    jumpType = 1;

                if (jumpDelay >= -1000f && jumpDelay <= -500f)
                    jumpType = 2;

                if (jumpDelay >= -2000f && jumpDelay <= -1500f)
                    jumpType = 3;

                if (jumpType > 0)
                    DoJump(NPC, jumpType, isLavaSlime, ref targetResetCountdown, out jumpDelay);

                else if (jumpDelay >= -30f)
                {
                    NPC.aiAction = 1;
                    return false;
                }
            }
            else if (NPC.target < Main.maxPlayers && ((NPC.direction == 1 && NPC.velocity.X < 3f) || (NPC.direction == -1 && NPC.velocity.X > -3f)))
            {
                if (NPC.collideX && Math.Abs(NPC.velocity.X) == 0.2f)
                {
                    NPC.position.X -= 1.4f * NPC.direction;
                }
                if (NPC.collideY && NPC.oldVelocity.Y != 0f && Collision.SolidCollision(NPC.position, NPC.width, NPC.height))
                {
                    NPC.position.X -= NPC.velocity.X + NPC.direction;
                }
                if ((NPC.direction == -1 && NPC.velocity.X < 0.01) || (NPC.direction == 1 && NPC.velocity.X > -0.01))
                {
                    NPC.velocity.X += 0.2f * (float)NPC.direction;
                    return false;
                }
                NPC.velocity.X *= 0.93f;
            }
            return false;
        }

        public static void DoJump(NPC npc, int jumpType, bool isLavaSlime, ref float targetResetCountdown, out float jumpDelay)
        {
            if (targetResetCountdown == 1f)
                npc.TargetClosest();

            float verticalJumpSpeed = 4f;
            float horizontalJumpSpeed = 4f;
            if (Main.slimeRain)
            {
                verticalJumpSpeed = 5f;
                horizontalJumpSpeed = 5f;
            }

            // Long jumps go further into the air.
            if (jumpType == 3)
            {
                verticalJumpSpeed *= 2.5f;
                horizontalJumpSpeed++;
                if (isLavaSlime)
                    verticalJumpSpeed += 2f;
            }

            // Perform the jump.
            npc.velocity.Y = -verticalJumpSpeed;
            npc.velocity.X += horizontalJumpSpeed * npc.direction;

            // Cycle between jump type 1, 2, and 3.
            if (jumpType == 3)
            {
                jumpDelay = -200f;
                npc.ai[3] = npc.position.X;
            }
            else if (jumpType == 1)
                jumpDelay = -1120f;
            else
                jumpDelay = -2120f;

            // Certain slimes have overall larger jumps.
            if (npc.type == NPCID.ToxicSludge || npc.type == ModContent.NPCType<PerennialSlime>() || npc.type == ModContent.NPCType<BloomSlime>() || npc.type == ModContent.NPCType<IrradiatedSlime>())
            {
                npc.velocity.X *= 1.2f;
                npc.velocity.Y *= 1.3f;
            }

            npc.netUpdate = true;
        }

        public static void DoWaterHoverBehavior(NPC npc, bool isLavaSlime, ref float targetResetCountdown)
        {
            // Move up if tiles are hit on the Y axis.
            if (npc.collideY)
                npc.velocity.Y = -(CalamityWorld.death ? 4f : 3f);

            if (npc.velocity.Y < 0f && npc.ai[3] == npc.position.X)
            {
                npc.direction *= -1;
                targetResetCountdown = 200f;
            }

            if (npc.velocity.Y > 0f)
                npc.ai[3] = npc.position.X;

            float riseSpeed = CalamityWorld.death ? 0.6f : 0.55f;
            float maxRiseSpeed = CalamityWorld.death ? 6f : 5f;
            if (isLavaSlime)
            {
                riseSpeed += 0.2f;
                maxRiseSpeed += 10f;
            }

            // Grind downward vertical movement to a halt if present.
            if (npc.velocity.Y > 2f)
                npc.velocity.Y *= 0.9f;

            // Move upwards more quickly if rising upward and the slime is lava in nature.
            else if (npc.directionY < 0 && isLavaSlime)
                npc.velocity.Y -= 1.2f;

            // Do typical rise movement.
            npc.velocity.Y -= riseSpeed;
            if (npc.velocity.Y < -maxRiseSpeed)
                npc.velocity.Y = -maxRiseSpeed;

            if (targetResetCountdown == 1f)
                npc.TargetClosest();
        }
    }
}
