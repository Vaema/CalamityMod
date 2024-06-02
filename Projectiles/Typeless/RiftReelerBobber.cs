using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Projectiles.Typeless
{
    public class RiftReelerBobber : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Typeless";

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.aiStyle = ProjAIStyleID.Bobber;
            Projectile.bobber = true;
        }

        public override bool PreDrawExtras()
        {
            if (Projectile.ai[2] == 0f)
                Lighting.AddLight(Projectile.Center, 0.5f, 0.25f, 0f);
            else
                Lighting.AddLight(Projectile.Center, 0f, 0.45f, 0.46f);
            return true;
        }
    }
}
