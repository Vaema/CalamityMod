using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.NPCs.VanillaNPCAIOverrides.MiniBosses;

public class IceQueenAI : VanillaAIOverride
{
    public override bool AI(Mod mod)
    {
        if (Main.dayTime)
        {
            if (NPC.velocity.X > 0f)
                NPC.velocity.X += 0.25f;
            else
                NPC.velocity.X -= 0.25f;

            NPC.velocity.Y -= 0.1f;
            NPC.rotation = NPC.velocity.X * 0.05f;
        }
        else if (NPC.ai[0] == 0f)
        {
            if (NPC.ai[2] == 0f)
            {
                NPC.TargetClosest(true);

                if (NPC.Center.X < Main.player[NPC.target].Center.X)
                    NPC.ai[2] = 1f;
                else
                    NPC.ai[2] = -1f;
            }

            NPC.TargetClosest(true);
            float iceQueenTargetDist = Math.Abs(NPC.Center.X - Main.player[NPC.target].Center.X);

            if (NPC.Center.X < Main.player[NPC.target].Center.X && NPC.ai[2] < 0f && iceQueenTargetDist > 800f)
                NPC.ai[2] = 0f;
            if (NPC.Center.X > Main.player[NPC.target].Center.X && NPC.ai[2] > 0f && iceQueenTargetDist > 800f)
                NPC.ai[2] = 0f;

            float iceQueenAcceleration = 0.6f;
            float iceQueenMaxVelocity = 10f;
            if ((double)NPC.life < (double)NPC.lifeMax * 0.75)
            {
                iceQueenAcceleration = 0.7f;
                iceQueenMaxVelocity = 12f;
            }
            if ((double)NPC.life < (double)NPC.lifeMax * 0.5)
            {
                iceQueenAcceleration = 0.8f;
                iceQueenMaxVelocity = 14f;
            }
            if ((double)NPC.life < (double)NPC.lifeMax * 0.25)
            {
                iceQueenAcceleration = 0.95f;
                iceQueenMaxVelocity = 16f;
            }

            NPC.velocity.X += NPC.ai[2] * iceQueenAcceleration;
            if (NPC.velocity.X > iceQueenMaxVelocity)
                NPC.velocity.X = iceQueenMaxVelocity;
            if (NPC.velocity.X < -iceQueenMaxVelocity)
                NPC.velocity.X = -iceQueenMaxVelocity;

            float iceQueenHoverHeight = Main.player[NPC.target].position.Y - (NPC.position.Y + (float)NPC.height);
            if (iceQueenHoverHeight < 150f)
                NPC.velocity.Y -= 0.2f;
            if (iceQueenHoverHeight > 200f)
                NPC.velocity.Y += 0.2f;
            if (NPC.velocity.Y > 9f)
                NPC.velocity.Y = 9f;
            if (NPC.velocity.Y < -9f)
                NPC.velocity.Y = -9f;

            NPC.rotation = NPC.velocity.X * 0.05f;

            if ((iceQueenTargetDist < 500f || NPC.ai[3] < 0f) && NPC.position.Y < Main.player[NPC.target].position.Y)
            {
                NPC.ai[3] += 1f;
                int frostWaveFireDelay = 8;
                if ((double)NPC.life < (double)NPC.lifeMax * 0.75)
                    frostWaveFireDelay = 7;
                if ((double)NPC.life < (double)NPC.lifeMax * 0.5)
                    frostWaveFireDelay = 6;
                if ((double)NPC.life < (double)NPC.lifeMax * 0.25)
                    frostWaveFireDelay = 5;

                frostWaveFireDelay++;
                if (NPC.ai[3] > (float)frostWaveFireDelay)
                    NPC.ai[3] = (float)-(float)frostWaveFireDelay;

                if (NPC.ai[3] == 0f && Main.netMode != NetmodeID.MultiplayerClient)
                {
                    Vector2 frostWavePosition = new Vector2(NPC.Center.X, NPC.Center.Y);
                    frostWavePosition.X += NPC.velocity.X * 7f;
                    float frostWaveTargetX = Main.player[NPC.target].position.X + (float)Main.player[NPC.target].width * 0.5f - frostWavePosition.X;
                    float frostWaveTargetY = Main.player[NPC.target].Center.Y - frostWavePosition.Y;
                    float frostWaveTargetDist = (float)Math.Sqrt((double)(frostWaveTargetX * frostWaveTargetX + frostWaveTargetY * frostWaveTargetY));

                    float frostWaveSpeed = 8f;
                    if ((double)NPC.life < (double)NPC.lifeMax * 0.75)
                        frostWaveSpeed = 9f;
                    if ((double)NPC.life < (double)NPC.lifeMax * 0.5)
                        frostWaveSpeed = 10f;
                    if ((double)NPC.life < (double)NPC.lifeMax * 0.25)
                        frostWaveSpeed = 11f;

                    frostWaveTargetDist = frostWaveSpeed / frostWaveTargetDist;
                    frostWaveTargetX *= frostWaveTargetDist;
                    frostWaveTargetY *= frostWaveTargetDist;

                    Projectile.NewProjectile(NPC.GetSource_FromAI(), frostWavePosition.X, frostWavePosition.Y, frostWaveTargetX, frostWaveTargetY, ProjectileID.FrostWave, 50, 0f, Main.myPlayer, 0f, 0f);
                }
            }
            else if (NPC.ai[3] < 0f)
                NPC.ai[3] += 1f;

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                NPC.ai[1] += (float)Main.rand.Next(1, 4);

                if (NPC.ai[1] > 600f && iceQueenTargetDist < 600f)
                    NPC.ai[0] = -1f;
            }
        }
        else if (NPC.ai[0] == 1f)
        {
            NPC.TargetClosest(true);

            float icicleAttackAcceleration = 0.2f;
            float icicleAttackMaxVelocity = 10f;
            if ((double)NPC.life < (double)NPC.lifeMax * 0.75)
            {
                icicleAttackAcceleration = 0.24f;
                icicleAttackMaxVelocity = 12f;
            }
            if ((double)NPC.life < (double)NPC.lifeMax * 0.5)
            {
                icicleAttackAcceleration = 0.28f;
                icicleAttackMaxVelocity = 14f;
            }
            if ((double)NPC.life < (double)NPC.lifeMax * 0.25)
            {
                icicleAttackAcceleration = 0.32f;
                icicleAttackMaxVelocity = 16f;
            }
            icicleAttackAcceleration -= 0.05f;
            icicleAttackMaxVelocity -= 1f;

            if (NPC.Center.X < Main.player[NPC.target].Center.X)
            {
                NPC.velocity.X += icicleAttackAcceleration;
                if (NPC.velocity.X < 0f)
                    NPC.velocity.X *= 0.98f;
            }
            if (NPC.Center.X > Main.player[NPC.target].Center.X)
            {
                NPC.velocity.X -= icicleAttackAcceleration;
                if (NPC.velocity.X > 0f)
                    NPC.velocity.X *= 0.98f;
            }
            if (NPC.velocity.X > icicleAttackMaxVelocity || NPC.velocity.X < -icicleAttackMaxVelocity)
                NPC.velocity.X *= 0.95f;

            float icicleAttackHoverHeight = Main.player[NPC.target].position.Y - (NPC.position.Y + (float)NPC.height);
            if (icicleAttackHoverHeight < 180f)
                NPC.velocity.Y -= 0.1f;
            if (icicleAttackHoverHeight > 200f)
                NPC.velocity.Y += 0.1f;

            if (NPC.velocity.Y > 7f)
                NPC.velocity.Y = 7f;
            if (NPC.velocity.Y < -7f)
                NPC.velocity.Y = -7f;

            NPC.rotation = NPC.velocity.X * 0.01f;

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                NPC.ai[3] += 1f;
                int icicleFireDelay = 10;
                if ((double)NPC.life < (double)NPC.lifeMax * 0.75)
                    icicleFireDelay = 8;
                if ((double)NPC.life < (double)NPC.lifeMax * 0.5)
                    icicleFireDelay = 6;
                if ((double)NPC.life < (double)NPC.lifeMax * 0.25)
                    icicleFireDelay = 4;
                if ((double)NPC.life < (double)NPC.lifeMax * 0.1)
                    icicleFireDelay = 2;

                icicleFireDelay += 3;
                if (NPC.ai[3] >= (float)icicleFireDelay)
                {
                    NPC.ai[3] = 0f;
                    Vector2 icicleSpawnPos = new Vector2(NPC.Center.X, NPC.position.Y + (float)NPC.height - 14f);
                    int i2 = (int)(icicleSpawnPos.X / 16f);
                    int j2 = (int)(icicleSpawnPos.Y / 16f);
                    if (!WorldGen.SolidTile(i2, j2))
                    {
                        float icicleFallSpeed = NPC.velocity.Y;

                        if (icicleFallSpeed < 0f)
                            icicleFallSpeed = 0f;

                        icicleFallSpeed += 3f;
                        float speedX2 = NPC.velocity.X * 0.25f;
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), icicleSpawnPos.X, icicleSpawnPos.Y, speedX2, icicleFallSpeed, ProjectileID.FrostShard, 44, 0f, Main.myPlayer, (float)Main.rand.Next(5), 0f);
                    }
                }
            }

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                NPC.ai[1] += (float)Main.rand.Next(1, 4);

                if (NPC.ai[1] > 450f)
                    NPC.ai[0] = -1f;
            }
        }
        else if (NPC.ai[0] == 2f)
        {
            NPC.TargetClosest(true);

            Vector2 iceRainPosition = new Vector2(NPC.Center.X, NPC.Center.Y - 20f);
            float iceRainXVel = (float)Main.rand.Next(-1000, 1001);
            float iceRainYVel = (float)Main.rand.Next(-1000, 1001);
            float iceRainVelocity = (float)Math.Sqrt((double)(iceRainXVel * iceRainXVel + iceRainYVel * iceRainYVel));
            float iceRainSpeed = 20f;

            NPC.velocity *= 0.95f;
            iceRainVelocity = iceRainSpeed / iceRainVelocity;
            iceRainXVel *= iceRainVelocity;
            iceRainYVel *= iceRainVelocity;
            NPC.rotation += 0.2f;
            iceRainPosition.X += iceRainXVel * 4f;
            iceRainPosition.Y += iceRainYVel * 4f;

            NPC.ai[3] += 1f;
            int iceRainFireDelay = 7;
            if ((double)NPC.life < (double)NPC.lifeMax * 0.75)
                iceRainFireDelay--;
            if ((double)NPC.life < (double)NPC.lifeMax * 0.5)
                iceRainFireDelay -= 2;
            if ((double)NPC.life < (double)NPC.lifeMax * 0.25)
                iceRainFireDelay -= 3;
            if ((double)NPC.life < (double)NPC.lifeMax * 0.1)
                iceRainFireDelay -= 4;

            if (NPC.ai[3] > (float)iceRainFireDelay)
            {
                NPC.ai[3] = 0f;
                int iceRainAttack = Projectile.NewProjectile(NPC.GetSource_FromAI(), iceRainPosition.X, iceRainPosition.Y, iceRainXVel, iceRainYVel, ProjectileID.FrostShard, 40, 0f, Main.myPlayer, 0f, 0f);
            }

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                NPC.ai[1] += (float)Main.rand.Next(1, 4);

                if (NPC.ai[1] > 300f)
                    NPC.ai[0] = -1f;
            }
        }
        if (NPC.ai[0] == -1f)
        {
            int attackPicker = Main.rand.Next(3);
            NPC.TargetClosest(true);

            if (Math.Abs(NPC.Center.X - Main.player[NPC.target].Center.X) > 1000f)
                attackPicker = 0;

            NPC.ai[0] = (float)attackPicker;
            NPC.ai[1] = 0f;
            NPC.ai[2] = 0f;
            NPC.ai[3] = 0f;
        }

        return false;
    }
}
