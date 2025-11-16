using System;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.NPCs.VanillaNPCAIOverrides.RegularEnemies;

public static partial class RevengeanceAndDeathAI
{
    public class SmallStarCellAI : VanillaAIOverride
    {
        public override bool AI(Mod mod)
        {
            float turnBigDelay = CalamityWorld.death ? 100f : 200f;
            if (NPC.velocity.Length() > 4f)
                NPC.velocity *= 0.95f;

            NPC.velocity *= 0.99f;
            NPC.ai[0]++;
            float cellScale = MathHelper.Clamp(NPC.ai[0] / turnBigDelay, 0f, 1f);
            NPC.scale = 1f + 0.3f * cellScale;
            if (NPC.ai[0] >= turnBigDelay)
            {
                if (Main.netMode != NetmodeID.MultiplayerClient)
                {
                    NPC.Transform(NPCID.StardustCellBig);
                    NPC.netUpdate = true;
                }

                return false;
            }

            NPC.rotation += NPC.velocity.X * 0.1f;
            if (!(NPC.ai[0] > 20f))
                return false;

            Vector2 cellCenter = NPC.Center;
            int dustAmt = (int)(NPC.ai[0] / (turnBigDelay / 2f));
            for (int i = 0; i < dustAmt + 1; i++)
            {
                if (Main.rand.NextBool())
                {
                    float dustScale = 0.4f;
                    if (i % 2 == 1)
                    {
                        dustScale = 0.65f;
                    }

                    Vector2 dustRotation = cellCenter + ((float)Main.rand.NextDouble() * ((float)Math.PI * 2f)).ToRotationVector2() * (12f - dustAmt * 2);
                    int cellDust = Dust.NewDust(dustRotation - Vector2.One * 12f, 24, 24, DustID.Electric, NPC.velocity.X / 2f, NPC.velocity.Y / 2f);
                    Dust dust = Main.dust[cellDust];
                    dust.position -= new Vector2(2f);
                    Main.dust[cellDust].velocity = Vector2.Normalize(cellCenter - dustRotation) * 1.5f * (10f - dustAmt * 2f) / 10f;
                    Main.dust[cellDust].noGravity = true;
                    Main.dust[cellDust].scale = dustScale;
                    Main.dust[cellDust].customData = NPC;
                }
            }

            return false;
        }
    }
}
