using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod.Projectiles.Enemy
{
    public class ShoreskipperTackle : ModProjectile
    {
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        private NPC OwnerNPC => Main.npc[(int)Projectile.ai[0]];

        public override void SetDefaults()
        {
            Projectile.width = 64;
            Projectile.height = 32;
            Projectile.hostile = true;
            Projectile.friendly = false;
            Projectile.ignoreWater = false;
            Projectile.tileCollide = false;
            Projectile.timeLeft = 360;
            Projectile.penetrate = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1; // Only hit once.
        }

        public override void AI()
        {
            if (!OwnerNPC.active || OwnerNPC.type != NPCType<NPCs.SunkenSea.Shoreskipper>())
            {
                Projectile.Kill();
                return;
            }

            Projectile.Center = OwnerNPC.Center;

            if (OwnerNPC.velocity.Y == 0f)
            {
                Projectile.Kill();
            }
        }

        public override bool? CanHitNPC(NPC target)
        {
            // Can only damage other Shoreskippers
            if (target.type == NPCType<NPCs.SunkenSea.Shoreskipper>())
            {
                // Never kill yourself
                if (target.whoAmI == OwnerNPC.whoAmI)
                    return false;

                return true;
            }
            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // Optional: add visual feedback
            Dust.NewDust(target.position, target.width, target.height, DustID.Water, 0f, -2f, 0, default, 1f);
        }
    }
}
