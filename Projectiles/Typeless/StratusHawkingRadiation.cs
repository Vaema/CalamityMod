using CalamityMod.Balancing;
using System;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Dusts;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityMod.Items.Weapons.Typeless;
using CalamityMod.Projectiles.Healing;
using CalamityMod.Particles;

namespace CalamityMod.Projectiles.Typeless
{
    public class StratusHawkingRadiation : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Typeless";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetDefaults()
        {
            Projectile.width = 64;
            Projectile.height = 64;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.DamageType = AverageDamageClass.Instance;
            Projectile.MaxUpdates = 2;
            Projectile.timeLeft = 75 * Projectile.MaxUpdates;
            Projectile.localNPCHitCooldown = -1;
            Projectile.aiStyle = 0;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
        }

        public override void AI()
        {
            Projectile.velocity *= 0.975f;
            if (Projectile.timeLeft < 20)
            {
                Projectile.Opacity = Projectile.timeLeft / 20f;
            }
            GeneralParticleHandler.SpawnParticle(new CustomSpark(Projectile.Center, Projectile.velocity, "CalamityMod/Particles/VerticalSmear", false, 3, 0.4f * Projectile.Opacity, Color.Turquoise * Projectile.Opacity * 0.75f,new Vector2(1,1f)));
        }
    }
}
