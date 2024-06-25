using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Melee
{
    public class UrchinSpikeFugu : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Melee";
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 6;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.ignoreWater = true;
            Projectile.alpha = 255;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 90;
            Projectile.noEnchantments = true;
        }

        public override void AI()
        {
            // Lionfish: ai[1] is 1 (unused)
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            if (Projectile.ai[0] == 0f)
            {
                float maxRange = 256f;
                int npcIndex = -1;
                foreach (NPC npc in Main.ActiveNPCs)
                {
                    if (npc.CanBeChasedBy(Projectile, false) && Collision.CanHit(Projectile.position, Projectile.width, Projectile.height, npc.position, npc.width, npc.height))
                    {
                        float targetDist = (npc.Center - Projectile.Center).Length();
                        if (targetDist < maxRange)
                        {
                            npcIndex = npc.whoAmI;
                            maxRange = targetDist;
                        }
                    }
                }
                Projectile.ai[0] = (float)(npcIndex + 1);
                if (Projectile.ai[0] == 0f)
                {
                    Projectile.ai[0] = -15f;
                }
                if (Projectile.ai[0] > 0f)
                {
                    Projectile.velocity = (Projectile.velocity * 20f + Projectile.SafeDirectionTo(Main.npc[(int)Projectile.ai[0] - 1].Center) * 4f) / 21f;
                    Projectile.netUpdate = true;
                }
            }
            else if (Projectile.ai[0] > 0f)
                Projectile.velocity = (Projectile.velocity * 40f + Projectile.SafeDirectionTo(Main.npc[(int)Projectile.ai[0] - 1].Center) * 12f) / 41f;
            else
            {
                Projectile.ai[0]++;
                Projectile.alpha -= 25;
                if (Projectile.alpha < 0)
                    Projectile.alpha = 0;

                Projectile.velocity.Y += 0.015f;
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => target.AddBuff(BuffID.Poisoned, 120);

        public override bool? CanDamage() => Projectile.ai[0] < 0f ? false : base.CanDamage();
    }
}
