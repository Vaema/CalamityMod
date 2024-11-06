using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Typeless
{
    public class DirectStrike : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Typeless";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public bool sticky => Projectile.ai[1] > 0;

        public override void SetDefaults()
        {
            Projectile.width = 2;
            Projectile.height = 2;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.extraUpdates = 0;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.alpha = 255;
            Projectile.timeLeft = 2;
        }
        public override void AI()
        {
            if (sticky)
                Projectile.Center = Main.npc[(int)Projectile.ai[0]].Center;
        }

        // If the AI parameter isn't a valid NPC slot, it can hit anything. Otherwise it can only hit one NPC.
        public override bool? CanHitNPC(NPC target)
        {
            if (Projectile.ai[0] < 0f || Projectile.ai[0] > 199f || Projectile.ai[0] == target.whoAmI)
                return null;
            return (bool?)false;
        }
    }
}
