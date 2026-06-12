using System;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.NPCs.VanillaNPCAIOverrides.RegularEnemies;

public static partial class RevengeanceAndDeathAI
{
    public class FlowInvaderAI : VanillaAIOverride
    {
        public override bool AI(Mod mod)
        {
            float velocityMult = CalamityWorld.death ? 8f : 6.5f;
            float moveSpeed = CalamityWorld.death ? 0.25f : 0.2f;
            NPC.TargetClosest();
            Vector2 desiredVelocity3 = Main.player[NPC.target].Center - NPC.Center + new Vector2(0f, -300f);
            float velocityCheck = desiredVelocity3.Length();
            if (velocityCheck < 20f)
            {
                desiredVelocity3 = NPC.velocity;
            }
            else if (velocityCheck < 40f)
            {
                desiredVelocity3.Normalize();
                desiredVelocity3 *= velocityMult * 0.35f;
            }
            else if (velocityCheck < 80f)
            {
                desiredVelocity3.Normalize();
                desiredVelocity3 *= velocityMult * 0.65f;
            }
            else
            {
                desiredVelocity3.Normalize();
                desiredVelocity3 *= velocityMult;
            }

            NPC.SimpleFlyMovement(desiredVelocity3, moveSpeed);
            NPC.rotation = NPC.velocity.X * 0.1f;
            if (!((NPC.ai[0] += 1f) >= (CalamityWorld.death ? 30f : 50f)))
                return false;

            NPC.ai[0] = 0f;
            if (Main.netMode != NetmodeID.MultiplayerClient)
            {
                Vector2 projDirection = Vector2.Zero;
                while (Math.Abs(projDirection.X) < 1.5f)
                    projDirection = Vector2.UnitY.RotatedByRandom(MathHelper.PiOver2) * new Vector2(5f, 3f);

                int proj = Projectile.NewProjectileDirect(NPC.GetSource_FromAI(), NPC.Center, projDirection, ProjectileID.StardustJellyfishSmall, 60, 0f, Main.myPlayer, 0f, NPC.whoAmI).identity;
                Main.projectile[proj].Calamity().extraUpdatesToSync = 1;
                if (Main.dedServ)
                    NetMessage.SendData(MessageID.SyncProjectile, -1, -1, null, proj);
            }

            return false;
        }
    }
}
