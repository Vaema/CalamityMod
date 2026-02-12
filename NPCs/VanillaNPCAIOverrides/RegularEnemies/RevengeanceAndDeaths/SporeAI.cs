using CalamityMod.World;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.NPCs.VanillaNPCAIOverrides.RegularEnemies;

public static partial class RevengeanceAndDeathAI
{
    public class SporeAI : VanillaAIOverride
    {
        public override bool AI(Mod mod)
        {
            if (NPC.type == NPCID.Spore)
                Lighting.AddLight(NPC.Center, 0.5f, 0.2f, 0.5f);

            if (NPC.timeLeft > 5)
                NPC.timeLeft = 5;

            NPC.noTileCollide = true;
            NPC.velocity.Y += 0.02f;

            // Ensure slow fall
            if (NPC.velocity.Y > 1f)
                NPC.velocity.Y = 1f;

            // Use regular AI if not spawned by a Giant Plantera Bulb
            if (NPC.ai[0] != -1f)
            {
                NPC.TargetClosest(true);
                float acceleration = CalamityWorld.death ? 0.25f : Main.expertMode ? 0.2f : 0.1f;
                float velocity = CalamityWorld.death ? 6.25f : Main.expertMode ? 5f : 3f;

                // Simple movement AI. You shouldn't need any help from comments to parse this.
                if (NPC.Center.X < Main.player[NPC.target].position.X)
                {
                    if (NPC.velocity.X < 0f)
                    {
                        NPC.velocity.X *= 0.96f;
                    }
                    NPC.velocity.X += acceleration;
                }
                else if (NPC.position.X > Main.player[NPC.target].Center.X)
                {
                    if (NPC.velocity.X > 0f)
                    {
                        NPC.velocity.X *= 0.96f;
                    }
                    NPC.velocity.X -= acceleration;
                }
                if (NPC.velocity.X > velocity || NPC.velocity.X < -velocity)
                {
                    NPC.velocity.X *= 0.97f;
                }
            }
            else
            {
                NPC.velocity.X *= 0.98f;
                NPC.damage = NPC.defDamage = 0;
            }

            NPC.rotation = NPC.velocity.X * 0.2f;

            return false;
        }
    }
}
