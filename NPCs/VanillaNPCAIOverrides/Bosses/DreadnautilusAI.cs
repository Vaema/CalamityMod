using System;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.NPCs.VanillaNPCAIOverrides.Bosses
{
    public class DreadnautilusAI : VanillaAIOverride
    {
        public override bool AI(Mod mod)
        {
            // Death Mode bool
            bool death = CalamityWorld.death;

            // Attack variables
            float goToAttackPositionAcceleration = death ? 0.2f : 0.15f;
            float goToAttackPositionVelocity = death ? 10f : 7.5f;
            float phaseSwitchPhaseTime = death ? 30f : 60f;
            float dashChargeUpPhaseTime = 120f;
            float dashPhaseTime = death ? 150f : 180f;
            float bloodSpitChargeUpPhaseTime = 90f;
            float bloodSpitPhaseTime = death ? 120f : 90f;
            int numBloodSpitVolleys = death ? 3 : 2;
            float bloodSquidPhaseTime = 180f;
            int maxBloodSquids = death ? 3 : 2;

            // Spawn effect
            if (NPC.localAI[0] == 0f)
            {
                NPC.localAI[0] = 1f;
                NPC.alpha = 255;
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    NPC.ai[0] = -1f;
                    NPC.netUpdate = true;
                }
            }

            // Create dust
            if (NPC.ai[0] != -1f && Main.rand.NextBool(4))
            {
                NPC.position += NPC.netOffset;
                Dust dust = Dust.NewDustDirect(NPC.position + new Vector2(5f), NPC.width - 10, NPC.height - 10, DustID.Blood);
                dust.velocity *= 0.5f;
                if (dust.velocity.Y < 0f)
                    dust.velocity.Y *= -1f;

                dust.alpha = 120;
                dust.scale = 1f + Main.rand.NextFloat() * 0.4f;
                dust.velocity += NPC.velocity * 0.3f;
                NPC.position -= NPC.netOffset;
            }

            // Get a target
            if (NPC.target == Main.maxPlayers)
            {
                NPC.TargetClosest();
                NPC.ai[2] = NPC.direction;
            }

            // Get a new target if the current target is dead or too far away
            if (Main.player[NPC.target].dead || Vector2.Distance(Main.player[NPC.target].Center, NPC.Center) > 2000f)
                NPC.TargetClosest();

            // Set to despawn
            NPCAimedTarget nPCAimedTarget = NPC.GetTargetData();
            if (Main.dayTime || !Main.bloodMoon)
                nPCAimedTarget = default(NPCAimedTarget);

            // Attacks and shit
            int attackType = -1;
            switch ((int)NPC.ai[0])
            {
                // Spawn effects
                case -1:
                    {
                        NPC.velocity *= 0.98f;
                        int spawnFaceDirection = Math.Sign(nPCAimedTarget.Center.X - NPC.Center.X);
                        if (spawnFaceDirection != 0)
                        {
                            NPC.direction = spawnFaceDirection;
                            NPC.spriteDirection = -NPC.direction;
                        }

                        if (NPC.localAI[1] == 0f && NPC.alpha < 100)
                        {
                            NPC.localAI[1] = 1f;
                            int dustAmt = 36;
                            for (int l = 0; l < dustAmt; l++)
                            {
                                NPC.position += NPC.netOffset;
                                Vector2 dustRotation = (Vector2.Normalize(NPC.velocity) * new Vector2(NPC.width / 2f, NPC.height) * 0.75f * 0.5f).RotatedBy((l - (dustAmt / 2 - 1)) * ((float)Math.PI * 2f) / dustAmt) + NPC.Center;
                                Vector2 dustVelocity = dustRotation - NPC.Center;
                                int spawnDustBlood = Dust.NewDust(dustRotation + dustVelocity, 0, 0, DustID.Blood, dustVelocity.X * 2f, dustVelocity.Y * 2f, 100, default, 1.4f);
                                Main.dust[spawnDustBlood].noGravity = true;
                                Main.dust[spawnDustBlood].velocity = Vector2.Normalize(dustVelocity) * 3f;
                                NPC.position -= NPC.netOffset;
                            }
                        }

                        if (NPC.ai[2] > 5f)
                        {
                            NPC.velocity.Y = -2.5f;
                            NPC.alpha -= 10;
                            if (Collision.SolidCollision(NPC.position, NPC.width, NPC.height))
                            {
                                NPC.alpha += 15;
                                if (NPC.alpha > 150)
                                    NPC.alpha = 150;
                            }

                            if (NPC.alpha < 0)
                                NPC.alpha = 0;
                        }

                        NPC.ai[2] += 1f;
                        if (NPC.ai[2] >= 50f)
                        {
                            NPC.ai[0] = 0f;
                            NPC.ai[1] = 0f;
                            NPC.ai[2] = 0f;
                            NPC.ai[3] = 0f;
                            NPC.netUpdate = true;
                        }

                        break;
                    }

                // Get in position for an attack and then choose an attack type
                case 0:
                    {
                        Vector2 destination = nPCAimedTarget.Center + new Vector2((0f - NPC.ai[2]) * 500f, -300f);
                        if (NPC.Center.Distance(destination) > 50f)
                        {
                            Vector2 desiredVelocity = NPC.DirectionTo(destination) * goToAttackPositionVelocity;
                            NPC.SimpleFlyMovement(desiredVelocity, goToAttackPositionAcceleration);
                        }

                        NPC.direction = (NPC.Center.X < nPCAimedTarget.Center.X) ? 1 : (-1);
                        float faceTargetDirection = NPC.Center.DirectionTo(nPCAimedTarget.Center).ToRotation() - 213f / 452f * NPC.spriteDirection;
                        if (NPC.spriteDirection == -1)
                            faceTargetDirection += (float)Math.PI;

                        if (NPC.spriteDirection != NPC.direction)
                        {
                            NPC.spriteDirection = NPC.direction;
                            NPC.rotation = 0f - NPC.rotation;
                            faceTargetDirection = 0f - faceTargetDirection;
                        }

                        NPC.rotation = NPC.rotation.AngleTowards(faceTargetDirection, 0.02f);
                        NPC.ai[1] += 1f;
                        if (NPC.ai[1] > phaseSwitchPhaseTime)
                        {
                            int attackPicker = (int)NPC.ai[3];
                            if (attackPicker % 7 == 3 && NPC.CountNPCS(NPCID.BloodSquid) < maxBloodSquids)
                            {
                                attackType = 3;
                            }
                            else if (attackPicker % 2 == 0)
                            {
                                SoundEngine.PlaySound(SoundID.Item170, NPC.Center);
                                attackType = 2;
                            }
                            else
                            {
                                SoundEngine.PlaySound(SoundID.Item170, NPC.Center);
                                attackType = 1;
                            }
                        }

                        break;
                    }

                // Dash
                case 1:
                    {
                        NPC.direction = (!(NPC.Center.X < nPCAimedTarget.Center.X)) ? 1 : (-1);
                        float chargeFaceDirection = NPC.Center.DirectionFrom(nPCAimedTarget.Center).ToRotation() - 213f / 452f * NPC.spriteDirection;
                        if (NPC.spriteDirection == -1)
                            chargeFaceDirection += (float)Math.PI;

                        bool shouldStartCharge = NPC.ai[1] < dashChargeUpPhaseTime;
                        if (NPC.spriteDirection != NPC.direction && shouldStartCharge)
                        {
                            NPC.spriteDirection = NPC.direction;
                            NPC.rotation = 0f - NPC.rotation;
                            chargeFaceDirection = 0f - chargeFaceDirection;
                        }

                        if (NPC.ai[1] < dashChargeUpPhaseTime)
                        {
                            if (NPC.ai[1] == dashChargeUpPhaseTime - 1f)
                                SoundEngine.PlaySound(SoundID.Item172, NPC.Center);

                            NPC.velocity *= 0.95f;
                            NPC.rotation = NPC.rotation.AngleLerp(chargeFaceDirection, 0.02f);
                            NPC.position += NPC.netOffset;
                            NPC.BloodNautilus_GetMouthPositionAndRotation(out Vector2 mouthPosition4, out Vector2 mouthDirection4);
                            Dust chargeUpDust = Dust.NewDustDirect(mouthPosition4 + mouthDirection4 * 60f - new Vector2(40f), 80, 80, DustID.Cloud, 0f, 0f, 150, Color.Transparent, 0.6f);
                            chargeUpDust.fadeIn = 1f;
                            chargeUpDust.velocity = chargeUpDust.position.DirectionTo(mouthPosition4 + Main.rand.NextVector2Circular(15f, 15f)) * chargeUpDust.velocity.Length();
                            chargeUpDust.noGravity = true;
                            chargeUpDust = Dust.NewDustDirect(mouthPosition4 + mouthDirection4 * 100f - new Vector2(30f), 60, 60, DustID.Cloud, 0f, 0f, 100, Color.Transparent, 0.9f);
                            chargeUpDust.fadeIn = 1.5f;
                            chargeUpDust.velocity = chargeUpDust.position.DirectionTo(mouthPosition4 + Main.rand.NextVector2Circular(15f, 15f)) * (chargeUpDust.velocity.Length() + 5f);
                            chargeUpDust.noGravity = true;
                            NPC.position -= NPC.netOffset;
                        }
                        else if (NPC.ai[1] < dashChargeUpPhaseTime + dashPhaseTime)
                        {
                            NPC.position += NPC.netOffset;
                            NPC.rotation = NPC.rotation.AngleLerp(chargeFaceDirection, 0.07f);
                            NPC.BloodNautilus_GetMouthPositionAndRotation(out Vector2 mouthPosition5, out Vector2 mouthDirection5);

                            // Dash directly towards the target until within 15 tiles of the target, and then continue in the same direction for 18 frames (15 frames in Death Mode)
                            if (NPC.ai[1] < dashChargeUpPhaseTime + dashPhaseTime * 0.9f)
                            {
                                if (NPC.Center.Distance(nPCAimedTarget.Center) > 240f || NPC.ai[1] == dashChargeUpPhaseTime)
                                    NPC.velocity = mouthDirection5 * -(death ? 20f : 16f) + NPC.Center.DirectionTo(nPCAimedTarget.Center) * 2f;
                                else
                                    NPC.ai[1] = dashChargeUpPhaseTime + dashPhaseTime * 0.9f;
                            }

                            for (int m = 0; m < 4; m++)
                            {
                                Dust chargeBloodDust = Dust.NewDustDirect(mouthPosition5 + mouthDirection5 * 60f - new Vector2(15f), 30, 30, DustID.Blood, 0f, 0f, 0, Color.Transparent, 1.5f);
                                chargeBloodDust.velocity = chargeBloodDust.position.DirectionFrom(mouthPosition5 + Main.rand.NextVector2Circular(5f, 5f)) * chargeBloodDust.velocity.Length();
                                chargeBloodDust.position -= mouthDirection5 * 60f;
                                chargeBloodDust = Dust.NewDustDirect(mouthPosition5 + mouthDirection5 * 100f - new Vector2(20f), 40, 40, DustID.Blood, 0f, 0f, 100, Color.Transparent, 1.5f);
                                chargeBloodDust.velocity = chargeBloodDust.position.DirectionFrom(mouthPosition5 + Main.rand.NextVector2Circular(10f, 10f)) * (chargeBloodDust.velocity.Length() + 5f);
                                chargeBloodDust.position -= mouthDirection5 * 100f;
                            }

                            NPC.position -= NPC.netOffset;
                        }

                        NPC.ai[1] += 1f;
                        if (NPC.ai[1] >= dashChargeUpPhaseTime + dashPhaseTime)
                            attackType = 0;

                        break;
                    }

                // Spit 3 spreads of blood projectiles
                case 2:
                    {
                        NPC.direction = (NPC.Center.X < nPCAimedTarget.Center.X) ? 1 : (-1);
                        float bloodProjFaceDirection = NPC.Center.DirectionTo(nPCAimedTarget.Center).ToRotation() - 213f / 452f * NPC.spriteDirection;
                        if (NPC.spriteDirection == -1)
                            bloodProjFaceDirection += (float)Math.PI;

                        if (NPC.spriteDirection != NPC.direction)
                        {
                            NPC.spriteDirection = NPC.direction;
                            NPC.rotation = 0f - NPC.rotation;
                            bloodProjFaceDirection = 0f - bloodProjFaceDirection;
                        }

                        NPC.rotation = NPC.rotation.AngleLerp(bloodProjFaceDirection, 0.2f);
                        if (NPC.ai[1] < bloodSpitChargeUpPhaseTime)
                        {
                            NPC.position += NPC.netOffset;
                            NPC.velocity *= 0.95f;
                            NPC.BloodNautilus_GetMouthPositionAndRotation(out Vector2 mouthPosition2, out Vector2 mouthDirection2);
                            if (!Main.rand.NextBool(4))
                            {
                                Dust bloodProjChargeUpDust = Dust.NewDustDirect(mouthPosition2 + mouthDirection2 * 60f - new Vector2(60f), 120, 120, DustID.Cloud, 0f, 0f, 150, Color.Transparent, 0.6f);
                                bloodProjChargeUpDust.fadeIn = 1f;
                                bloodProjChargeUpDust.velocity = bloodProjChargeUpDust.position.DirectionTo(mouthPosition2 + Main.rand.NextVector2Circular(15f, 15f)) * (bloodProjChargeUpDust.velocity.Length() + 3f);
                                bloodProjChargeUpDust.noGravity = true;
                                bloodProjChargeUpDust = Dust.NewDustDirect(mouthPosition2 + mouthDirection2 * 100f - new Vector2(80f), 160, 160, DustID.Cloud, 0f, 0f, 100, Color.Transparent, 0.9f);
                                bloodProjChargeUpDust.fadeIn = 1.5f;
                                bloodProjChargeUpDust.velocity = bloodProjChargeUpDust.position.DirectionTo(mouthPosition2 + Main.rand.NextVector2Circular(15f, 15f)) * (bloodProjChargeUpDust.velocity.Length() + 5f);
                                bloodProjChargeUpDust.noGravity = true;
                            }

                            NPC.position -= NPC.netOffset;
                        }
                        else if (NPC.ai[1] < bloodSpitChargeUpPhaseTime + bloodSpitPhaseTime)
                        {
                            NPC.position += NPC.netOffset;
                            NPC.velocity *= 0.9f;
                            float bloodProjShootTimer = (NPC.ai[1] - bloodSpitChargeUpPhaseTime) % (bloodSpitPhaseTime / numBloodSpitVolleys);
                            NPC.BloodNautilus_GetMouthPositionAndRotation(out Vector2 mouthPosition3, out Vector2 mouthDirection3);
                            if (bloodProjShootTimer < bloodSpitPhaseTime / numBloodSpitVolleys * 0.8f)
                            {
                                for (int i = 0; i < 5; i++)
                                {
                                    Dust bloodProjShootDust = Dust.NewDustDirect(mouthPosition3 + mouthDirection3 * 50f - new Vector2(15f), 30, 30, DustID.Blood, 0f, 0f, 0, Color.Transparent, 1.5f);
                                    bloodProjShootDust.velocity = bloodProjShootDust.position.DirectionFrom(mouthPosition3 + Main.rand.NextVector2Circular(5f, 5f)) * bloodProjShootDust.velocity.Length();
                                    bloodProjShootDust.position -= mouthDirection3 * 60f;
                                    bloodProjShootDust = Dust.NewDustDirect(mouthPosition3 + mouthDirection3 * 90f - new Vector2(20f), 40, 40, DustID.Blood, 0f, 0f, 100, Color.Transparent, 1.5f);
                                    bloodProjShootDust.velocity = bloodProjShootDust.position.DirectionFrom(mouthPosition3 + Main.rand.NextVector2Circular(10f, 10f)) * (bloodProjShootDust.velocity.Length() + 5f);
                                    bloodProjShootDust.position -= mouthDirection3 * 100f;
                                }
                            }

                            // Spit blood spread
                            if ((int)bloodProjShootTimer == 0)
                            {
                                // Recoil away with each spit
                                NPC.velocity += mouthDirection3 * -8f;

                                // Spawn dust with each spit
                                for (int j = 0; j < 20; j++)
                                {
                                    Dust bloodProjShootDust2 = Dust.NewDustDirect(mouthPosition3 + mouthDirection3 * 60f - new Vector2(15f), 30, 30, DustID.Blood, 0f, 0f, 0, Color.Transparent, 1.5f);
                                    bloodProjShootDust2.velocity = bloodProjShootDust2.position.DirectionFrom(mouthPosition3 + Main.rand.NextVector2Circular(5f, 5f)) * bloodProjShootDust2.velocity.Length();
                                    bloodProjShootDust2.position -= mouthDirection3 * 60f;
                                    bloodProjShootDust2 = Dust.NewDustDirect(mouthPosition3 + mouthDirection3 * 100f - new Vector2(20f), 40, 40, DustID.Blood, 0f, 0f, 100, Color.Transparent, 1.5f);
                                    bloodProjShootDust2.velocity = bloodProjShootDust2.position.DirectionFrom(mouthPosition3 + Main.rand.NextVector2Circular(10f, 10f)) * (bloodProjShootDust2.velocity.Length() + 5f);
                                    bloodProjShootDust2.position -= mouthDirection3 * 100f;
                                }

                                // Spawn projectiles
                                if (Main.netMode != NetmodeID.MultiplayerClient)
                                {
                                    int projectileAmt = death ? 6 : 5;
                                    int spread = death ? 35 : 30;
                                    float rotation = MathHelper.ToRadians(spread);
                                    Vector2 initialProjectileVelocity = mouthDirection3 * 10f;
                                    int damage = NPC.GetAttackDamage_ForProjectiles(30f, 25f);
                                    for (int k = 0; k < projectileAmt + 1; k++)
                                    {
                                        Vector2 perturbedSpeed = initialProjectileVelocity.RotatedBy(MathHelper.Lerp(-rotation, rotation, k / (float)(projectileAmt - 1)));
                                        Projectile.NewProjectile(NPC.GetSource_FromAI(), mouthPosition3 - mouthDirection3 * 5f, initialProjectileVelocity + perturbedSpeed, ProjectileID.BloodNautilusShot, damage, 0f, Main.myPlayer);
                                    }
                                }
                            }

                            NPC.position -= NPC.netOffset;
                        }

                        NPC.ai[1] += 1f;
                        if (NPC.ai[1] >= bloodSpitChargeUpPhaseTime + bloodSpitPhaseTime)
                            attackType = 0;

                        break;
                    }

                // Spawn Blood Squids
                case 3:
                    {
                        NPC.direction = (NPC.Center.X < nPCAimedTarget.Center.X) ? 1 : (-1);
                        float targetAngle = 0f;
                        NPC.spriteDirection = NPC.direction;
                        if (NPC.ai[1] < bloodSquidPhaseTime)
                        {
                            NPC.position += NPC.netOffset;
                            float bloodSquidVelClamp = MathHelper.Clamp(1f - NPC.ai[1] / bloodSquidPhaseTime * 1.5f, 0f, 1f);
                            NPC.velocity = Vector2.Lerp(value2: new Vector2(0f, bloodSquidVelClamp * -1.5f), value1: NPC.velocity, amount: 0.03f);
                            NPC.velocity = Vector2.Zero;
                            NPC.rotation = NPC.rotation.AngleLerp(targetAngle, 0.02f);
                            NPC.BloodNautilus_GetMouthPositionAndRotation(out Vector2 _, out Vector2 _);
                            float t = NPC.ai[1] / bloodSquidPhaseTime;
                            float scaleFactor2 = Utils.GetLerpValue(0f, 0.5f, t) * Utils.GetLerpValue(1f, 0.5f, t);
                            Lighting.AddLight(NPC.Center, new Vector3(1f, 0.5f, 0.5f) * scaleFactor2);
                            if (!Main.rand.NextBool(3))
                            {
                                Dust bloodSquidSpawnDust = Dust.NewDustDirect(NPC.Center - new Vector2(6f), 12, 12, DustID.Blood, 0f, 0f, 60, Color.Transparent, 1.4f);
                                bloodSquidSpawnDust.position += new Vector2(NPC.spriteDirection * 12, 12f);
                                bloodSquidSpawnDust.velocity *= 0.1f;
                            }

                            NPC.position -= NPC.netOffset;
                        }

                        if (NPC.ai[1] == 10f || (death && NPC.ai[1] == 20f) || NPC.ai[1] == 30f)
                            BloodNautilus_CallForHelp(NPC);

                        NPC.ai[1] += 1f;
                        if (NPC.ai[1] >= bloodSquidPhaseTime)
                            attackType = 0;

                        break;
                    }
            }

            // Set AI arrays for the next attack
            if (attackType != -1)
            {
                NPC.ai[0] = attackType;
                NPC.ai[1] = 0f;
                NPC.ai[2] = 0f;
                NPC.netUpdate = true;
                NPC.TargetClosest();
                if (attackType == 0)
                    NPC.ai[2] = NPC.direction;
                else
                    NPC.ai[3] += 1f;
            }

            // Always set this to false because it's fucking stupid
            NPC.reflectsProjectiles = false;

            return false;
        }

        private static void BloodNautilus_CallForHelp(NPC npc)
        {
            if (Main.netMode == NetmodeID.MultiplayerClient || !Main.player[npc.target].active || Main.player[npc.target].dead || npc.Distance(Main.player[npc.target].Center) > 2000f)
                return;

            Point npcCenterTileCoords = npc.Center.ToTileCoordinates();
            Point npcCenterTileCoordsCopy = npcCenterTileCoords;
            int bloodTearRandSpawnOffset = 20;
            int npcCenterTileRadius = 3;
            int npcCenterCopyTileRadius = 8;
            int bloodTearSpawnTileRadius = 2;
            int attempts = 0;
            int bloodTearTileX;
            int bloodTearTileY;
            while (true)
            {
                if (attempts >= 100)
                    return;

                attempts++;
                bloodTearTileX = Main.rand.Next(npcCenterTileCoordsCopy.X - bloodTearRandSpawnOffset, npcCenterTileCoordsCopy.X + bloodTearRandSpawnOffset + 1);
                bloodTearTileY = Main.rand.Next(npcCenterTileCoordsCopy.Y - bloodTearRandSpawnOffset, npcCenterTileCoordsCopy.Y + bloodTearRandSpawnOffset + 1);
                if ((bloodTearTileY < npcCenterTileCoordsCopy.Y - npcCenterCopyTileRadius || bloodTearTileY > npcCenterTileCoordsCopy.Y + npcCenterCopyTileRadius || bloodTearTileX < npcCenterTileCoordsCopy.X - npcCenterCopyTileRadius || bloodTearTileX > npcCenterTileCoordsCopy.X + npcCenterCopyTileRadius) && (bloodTearTileY < npcCenterTileCoords.Y - npcCenterTileRadius || bloodTearTileY > npcCenterTileCoords.Y + npcCenterTileRadius || bloodTearTileX < npcCenterTileCoords.X - npcCenterTileRadius || bloodTearTileX > npcCenterTileCoords.X + npcCenterTileRadius) && !Main.tile[bloodTearTileX, bloodTearTileY].HasUnactuatedTile)
                {
                    bool spawnBloodTear = true;
                    if (spawnBloodTear && Main.tile[bloodTearTileX, bloodTearTileY].LiquidType == LiquidID.Lava)
                        spawnBloodTear = false;

                    if (spawnBloodTear && Collision.SolidTiles(bloodTearTileX - bloodTearSpawnTileRadius, bloodTearTileX + bloodTearSpawnTileRadius, bloodTearTileY - bloodTearSpawnTileRadius, bloodTearTileY + bloodTearSpawnTileRadius))
                        spawnBloodTear = false;

                    if (spawnBloodTear && !Collision.CanHitLine(npc.Center, 0, 0, Main.player[npc.target].Center, 0, 0))
                        spawnBloodTear = false;

                    if (spawnBloodTear)
                        break;
                }
            }

            Projectile.NewProjectile(npc.GetSource_FromAI(), bloodTearTileX * 16 + 8, bloodTearTileY * 16 + 8, 0f, 0f, ProjectileID.BloodNautilusTears, 0, 0f, Main.myPlayer);
        }
    }
}
