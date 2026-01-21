using CalamityMod.World;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.NPCs.VanillaNPCAIOverrides.RegularEnemies;

public static partial class RevengeanceAndDeathAI
{
    public class AngryNimbusAI : VanillaAIOverride
    {
        public override bool AI(Mod mod)
        {
            NPC.noGravity = true;
            NPC.TargetClosest(true);
            float speed = CalamityWorld.death ? 9f : 6f;
            float acceleration = CalamityWorld.death ? 0.3f : 0.25f;

            Vector2 idealVelocity = NPC.SafeDirectionTo(Main.player[NPC.target].Center - 300f * Vector2.UnitY) * speed;
            float playerDistance = NPC.Distance(Main.player[NPC.target].Center);
            if (playerDistance < 20f)
                idealVelocity = NPC.velocity;

            // Yes, I understand that npc.SimpleFlyMovement exists. However, the "acceleration * 2f" is not a part of that method.
            // It is not identical to what is being achieved here.
            if (NPC.velocity.X < idealVelocity.X)
            {
                NPC.velocity.X += acceleration;
                if (NPC.velocity.X < 0f && idealVelocity.X > 0f)
                {
                    NPC.velocity.X = NPC.velocity.X + acceleration * 2f;
                }
            }
            else if (NPC.velocity.X > idealVelocity.X)
            {
                NPC.velocity.X -= acceleration;
                if (NPC.velocity.X > 0f && idealVelocity.X < 0f)
                {
                    NPC.velocity.X -= acceleration * 2f;
                }
            }
            if (NPC.velocity.Y < idealVelocity.Y)
            {
                NPC.velocity.Y += acceleration;
                if (NPC.velocity.Y < 0f && idealVelocity.Y > 0f)
                {
                    NPC.velocity.Y += acceleration * 2f;
                }
            }
            else if (NPC.velocity.Y > idealVelocity.Y)
            {
                NPC.velocity.Y -= acceleration;
                if (NPC.velocity.Y > 0f && idealVelocity.Y < 0f)
                {
                    NPC.velocity.Y -= acceleration * 2f;
                }
            }

            // Make it rain
            float minXRainDistance = CalamityWorld.death ? 200f : 150f;
            if (NPC.Center.X > Main.player[NPC.target].position.X - minXRainDistance &&
                NPC.position.X < Main.player[NPC.target].Center.X + minXRainDistance &&
                NPC.Center.Y < Main.player[NPC.target].position.Y &&
                Collision.CanHit(NPC.position, NPC.width, NPC.height, Main.player[NPC.target].position, Main.player[NPC.target].width, Main.player[NPC.target].height) &&
                Main.netMode != NetmodeID.MultiplayerClient)
            {
                if (NPC.justHit)
                    NPC.ai[0] = 0f;

                NPC.ai[0] += 1f;
                if (NPC.ai[0] % 8f == 0f)
                {
                    Vector2 rainSpawnPosition = NPC.position + new Vector2(10f + Main.rand.Next(NPC.width - 20), NPC.height + 4f);

                    Projectile.NewProjectile(NPC.GetSource_FromAI(), rainSpawnPosition, Vector2.UnitY * 5f, ProjectileID.RainNimbus, 20, 0f, Main.myPlayer, 0f, 0f);
                    if (NPC.ai[0] % 16f == 0f)
                    {
                        float speedX = (float)Main.rand.NextFloat(CalamityWorld.death ? -6f : -3f, CalamityWorld.death ? 6f : 3f) * (Main.rand.NextFloat() - 0.5f);
                        Projectile.NewProjectile(NPC.GetSource_FromAI(), rainSpawnPosition, new Vector2(speedX, 5f), ProjectileID.FrostShard, 20, 0f, Main.myPlayer, 0f, 0f);
                    }
                }
                if (NPC.ai[0] >= 607f)
                    NPC.ai[0] = 0f;
            }
            return false;
        }
    }
}
