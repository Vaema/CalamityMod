using CalamityMod.World;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.NPCs.VanillaNPCAIOverrides.RegularEnemies;

public static partial class RevengeanceAndDeathAI
{
    public class BlazingWheelAI : VanillaAIOverride
    {
        public override bool AI(Mod mod)
        {
            if (NPC.ai[0] == 0f)
            {
                NPC.TargetClosest();
                NPC.directionY = 1;
                NPC.ai[0] = 1f;
            }

            int wheelVelocity = CalamityWorld.death ? 9 : 6;
            if (NPC.ai[1] == 0f)
            {
                NPC.rotation += (float)(NPC.direction * NPC.directionY) * 0.13f;

                if (NPC.collideY)
                    NPC.ai[0] = 2f;

                if (!NPC.collideY && NPC.ai[0] == 2f)
                {
                    NPC.direction = -NPC.direction;
                    NPC.ai[1] = 1f;
                    NPC.ai[0] = 1f;
                }

                if (NPC.collideX)
                {
                    NPC.directionY = -NPC.directionY;
                    NPC.ai[1] = 1f;
                }
            }
            else
            {
                NPC.rotation -= (float)(NPC.direction * NPC.directionY) * 0.13f;

                if (NPC.collideX)
                    NPC.ai[0] = 2f;

                if (!NPC.collideX && NPC.ai[0] == 2f)
                {
                    NPC.directionY = -NPC.directionY;
                    NPC.ai[1] = 0f;
                    NPC.ai[0] = 1f;
                }

                if (NPC.collideY)
                {
                    NPC.direction = -NPC.direction;
                    NPC.ai[1] = 0f;
                }
            }

            NPC.velocity.X = (float)(wheelVelocity * NPC.direction);
            NPC.velocity.Y = (float)(wheelVelocity * NPC.directionY);

            float lighting = (float)(270 - (int)Main.mouseTextColor) / 400f;
            Lighting.AddLight((int)(NPC.position.X + (float)(NPC.width / 2)) / 16, (int)(NPC.position.Y + (float)(NPC.height / 2)) / 16, 0.9f, 0.3f + lighting, 0.2f);

            // Emit fire dust from center when about to fire
            if (NPC.localAI[0] > (CalamityWorld.death ? BlazingWheelFlameGateValue_Death : BlazingWheelFlameGateValue) - BlazingWheelTelegraphTime)
            {
                Dust dust = Dust.NewDustDirect(NPC.Center + Main.rand.NextVector2CircularEdge(5f, 5f), 1, 1, DustID.Torch, 0f, 0f, 0, default, 3f);
                dust.noGravity = true;
                dust.velocity *= 0f;
            }

            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                NPC.localAI[0] += 1f;
                if (NPC.localAI[0] >= (CalamityWorld.death ? BlazingWheelFlameGateValue_Death : BlazingWheelFlameGateValue))
                {
                    NPC.localAI[0] = 0f;
                    for (int i = 0; i < 4; i++)
                    {
                        Vector2 vector255 = new Vector2(0f, -5f).RotatedBy((double)(MathHelper.PiOver2 * (float)i));
                        int proj = Projectile.NewProjectile(NPC.GetSource_FromAI(), NPC.Center, vector255, ProjectileID.FlamesTrap, 20, 0f, Main.myPlayer);
                        Main.projectile[proj].tileCollide = false;
                        Main.projectile[proj].friendly = false;
                        Main.projectile[proj].trap = false;
                    }
                }
            }

            return false;
        }
    }
}
