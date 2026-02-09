using CalamityMod.World;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.NPCs.VanillaNPCAIOverrides.RegularEnemies;

public static partial class RevengeanceAndDeathAI
{
    public class FlyingWeaponAI : VanillaAIOverride
    {
        public override bool AI(Mod mod)
        {
            NPC.knockBackResist = 0f;
            NPC.noGravity = true;
            NPC.noTileCollide = true;
            if (NPC.type == NPCID.EnchantedSword)
            {
                Lighting.AddLight((int)(NPC.Center.X / 16f), (int)(NPC.Center.Y / 16f), 0.2f, 0.05f, 0.3f);
            }
            else if (NPC.type == NPCID.CrimsonAxe)
            {
                Lighting.AddLight((int)(NPC.Center.X / 16f), (int)(NPC.Center.Y / 16f), 0.3f, 0.15f, 0.05f);
            }
            else
            {
                Lighting.AddLight((int)(NPC.Center.X / 16f), (int)(NPC.Center.Y / 16f), 0.05f, 0.2f, 0.3f);
            }
            // Adjust target if we don't have one.
            if (NPC.target < 0 || NPC.target == Main.maxPlayers || Main.player[NPC.target].dead)
            {
                NPC.TargetClosest(true);
            }
            // Charge
            if (NPC.ai[0] == 0f)
            {
                float chargeSpeed = CalamityWorld.death ? 16f : 12f;
                NPC.velocity = NPC.SafeDirectionTo(Main.player[NPC.target].Center, -Vector2.UnitY) * chargeSpeed;
                NPC.rotation = NPC.velocity.ToRotation() + MathHelper.PiOver4;

                // Slow down
                NPC.ai[0] = 1f;
                NPC.ai[1] = 0f;
                NPC.netUpdate = true;
                return false;
            }
            // Slow down
            if (NPC.ai[0] == 1f)
            {
                NPC.velocity *= CalamityWorld.death ? 0.98f : 0.99f;
                NPC.ai[1] += 1f;
                // Get ready to spin and then charge again.
                if (NPC.ai[1] >= (CalamityWorld.death ? 50f : 100f))
                {
                    NPC.netUpdate = true;
                    NPC.ai[0] = 2f;
                    NPC.ai[1] = 0f;
                    NPC.velocity = Vector2.Zero;
                }
            }
            // Spin
            else
            {
                NPC.velocity *= CalamityWorld.death ? 0.94f : 0.96f;
                NPC.ai[1] += 1f;

                float anglularSpeed = NPC.ai[1] / (CalamityWorld.death ? 90f : 150f);
                anglularSpeed = 0.1f + anglularSpeed * 0.4f;
                NPC.rotation += anglularSpeed * NPC.direction;

                // Charge
                if (NPC.ai[1] >= (CalamityWorld.death ? 90f : 150f))
                {
                    NPC.netUpdate = true;
                    NPC.ai[0] = 0f;
                    NPC.ai[1] = 0f;
                }
            }
            return false;
        }
    }
}
