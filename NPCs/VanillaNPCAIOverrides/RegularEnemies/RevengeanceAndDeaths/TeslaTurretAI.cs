using System;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.NPCs.VanillaNPCAIOverrides.RegularEnemies;

public static partial class RevengeanceAndDeathAI
{
    public class TeslaTurretAI : VanillaAIOverride
    {
        public override bool AI(Mod mod)
        {
            NPC.TargetClosest(false);

            NPC.spriteDirection = NPC.direction;

            NPC.velocity.X *= 0.93f;
            if (Math.Abs(NPC.velocity.X) < 0.1f)
            {
                NPC.velocity.X = 0f;
            }

            float appearTime = 120f;
            float alphaFadeinTime = 60f;

            // Spend the first "appearTime" frames sitting around and spawning.
            if (NPC.ai[1] < appearTime)
            {
                NPC.ai[1] += 1f;
                if (NPC.ai[1] > appearTime - alphaFadeinTime)
                {
                    float alphaRatio = (NPC.ai[1] - alphaFadeinTime) / (appearTime - alphaFadeinTime);
                    NPC.alpha = (int)((1f - alphaRatio) * 255f);
                }
                else
                {
                    NPC.alpha = 255;
                }

                NPC.dontTakeDamage = true;

                NPC.frameCounter = 0.0;
                NPC.frame.Y = 0;

                // Circular dust
                float angularRatio = NPC.ai[1] / alphaFadeinTime;
                Vector2 spinningpoint = new Vector2(0f, -30f).RotatedBy(angularRatio * 1.5f * MathHelper.TwoPi) * new Vector2(1f, 0.4f);

                for (int i = 0; i < 4; i++)
                {
                    Vector2 dustSpawnDelta = Vector2.Zero;
                    float scaleFactor2 = 1f;
                    switch (i)
                    {
                        case 0:
                            dustSpawnDelta = Vector2.UnitY * -15f;
                            scaleFactor2 = 0.15f;
                            break;
                        case 1:
                            dustSpawnDelta = Vector2.UnitY * -5f;
                            scaleFactor2 = 0.3f;
                            break;
                        case 2:
                            dustSpawnDelta = Vector2.UnitY * 5f;
                            scaleFactor2 = 0.6f;
                            break;
                        case 3:
                            dustSpawnDelta = Vector2.UnitY * 20f;
                            scaleFactor2 = 0.45f;
                            break;
                    }

                    int idx = Dust.NewDust(NPC.Center, 0, 0, DustID.Electric, 0f, 0f, 100, default, 0.5f);
                    Main.dust[idx].noGravity = true;
                    Main.dust[idx].position = NPC.Center + spinningpoint * scaleFactor2 + dustSpawnDelta;
                    Main.dust[idx].velocity = Vector2.Zero;
                    spinningpoint *= -1f;

                    idx = Dust.NewDust(NPC.Center, 0, 0, DustID.Electric, 0f, 0f, 100, default, 0.5f);
                    Main.dust[idx].noGravity = true;
                    Main.dust[idx].position = NPC.Center + spinningpoint * scaleFactor2 + dustSpawnDelta;
                    Main.dust[idx].velocity = Vector2.Zero;
                }

                Lighting.AddLight((int)NPC.Center.X / 16, (int)(NPC.Center.Y - 10f) / 16, 0.1f * angularRatio, 0.5f * angularRatio, 0.7f * angularRatio);

                return false;
            }

            Lighting.AddLight((int)NPC.Center.X / 16, (int)(NPC.Center.Y - 10f) / 16, 0.1f, 0.5f, 0.7f);
            NPC.dontTakeDamage = false;

            if (NPC.ai[0] < 60f)
            {
                NPC.ai[0] += 1f;
            }

            // Reset laser shoot counter
            if (NPC.justHit)
            {
                NPC.ai[0] = 0f;
            }

            // Shoot laser
            if (NPC.ai[0] == 60f)
            {
                NPC.ai[0] = CalamityWorld.death ? -60f : -120f;

                // The "+ Main.player[npc.target].velocity * 20f" part ensures the turret will aim ahead of the player
                Vector2 distanceVector = Main.player[NPC.target].Center + Main.player[NPC.target].velocity * 20f - (NPC.Center - Vector2.UnitY * 10f);

                if (distanceVector.HasNaNs())
                {
                    distanceVector = -Vector2.UnitY;
                }
                Vector2 velocity = Vector2.Normalize(distanceVector) * 14f;

                Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center - Vector2.UnitY * 10f, velocity, ProjectileID.MartianTurretBolt, 28, 0f, Main.myPlayer, 0f, 0f);
            }

            return false;
        }
    }
}
