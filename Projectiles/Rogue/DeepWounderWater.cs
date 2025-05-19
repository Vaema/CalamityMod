using Microsoft.Xna.Framework;
using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Rogue
{
    public class DeepWounderWater : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";

        private const int TimeBeforeBurst = 120;
        private bool foundTarget = false;

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 160;
            Projectile.DamageType = RogueDamageClass.Instance;
        }

        public override void AI()
        {
            Dust idleDust = Dust.NewDustPerfect(Projectile.Center, DustID.Water, Vector2.Zero, Scale: 1.5f);
            idleDust.noGravity = true;
            Projectile.rotation += 0.2f;

            if (Projectile.timeLeft > TimeBeforeBurst)
            {
                Projectile.velocity *= 0.97f;
            }
            else
            {
                // Zoom off towards the nearest target
                // Don't bother running code if you've already found one
                if (foundTarget)
                    return;

                // Find the closest target
                float npcDistCompare = 960f;
                int index = -1;
                foreach (NPC n in Main.ActiveNPCs)
                {
                    if (!n.CanBeChasedBy(Projectile))
                        continue;

                    float hitboxWidth = Math.Max(n.Hitbox.Width / 2f, n.Hitbox.Height / 2f);
                    float currentNPCDist = n.Distance(Projectile.Center) - hitboxWidth;
                    if ((currentNPCDist < npcDistCompare) && (Collision.CanHit(Projectile.Center, 1, 1, n.Center, 1, 1)))
                    {
                        npcDistCompare = currentNPCDist;
                        index = n.whoAmI;
                    }
                }

                if (index != -1)
                {
                    foundTarget = true;

                    Projectile.velocity = CalamityUtils.CalculatePredictiveAimToTargetMaxUpdates(Projectile.Center, Main.npc[index], 7f, 4);
                    Projectile.MaxUpdates = 4;
                    if (Projectile.timeLeft < TimeBeforeBurst)
                        Projectile.timeLeft = TimeBeforeBurst;

                    for (int i = 0; i < 12; i++)
                    {
                        Vector2 dustVel = Main.rand.NextVector2CircularEdge(7, 7);
                        Dust dustBurst = Dust.NewDustPerfect(Projectile.Center, DustID.Water, dustVel);
                        dustBurst.noGravity = true;
                    }
                }
            }
        }

        public override bool? CanDamage() => Projectile.timeLeft <= TimeBeforeBurst;
    }
}
