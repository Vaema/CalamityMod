using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using CalamityMod.Particles;
using Terraria.ID;
using Terraria.ModLoader;
using System;

namespace CalamityMod.Projectiles.Rogue
{
    public class ContaminatedBileFlask : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";
        public override string Texture => "CalamityMod/Items/Weapons/Rogue/ContaminatedBile";

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 28;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = true;
            Projectile.aiStyle = ProjAIStyleID.ThrownProjectile;
            Projectile.alpha = 0;
            Projectile.DamageType = RogueDamageClass.Instance;
        }

        public override void AI()
        {
            float rotateratio = 0.002f;
            float rotation = (Math.Abs(Projectile.velocity.X) + Math.Abs(Projectile.velocity.Y)) * rotateratio;
            Projectile.rotation += rotation * Projectile.direction;
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item107, Projectile.Bottom);
            #region Visuals
            Particle blast = new CustomPulse(Projectile.Center, Vector2.Zero, Color.DarkOliveGreen, "CalamityMod/Particles/BloomRing", Vector2.One, Main.rand.NextFloat(-10f, 10f), 0f, 0.7f, 200, true, 0.7f);
            GeneralParticleHandler.SpawnParticle(blast);
            Particle blast1 = new CustomPulse(Projectile.Center, Vector2.Zero, Color.OliveDrab * 0.5f, "CalamityMod/Particles/FlameExplosion", Vector2.One, Main.rand.NextFloat(-10f, 10f), 0.02f, 0.05f, 340);
            GeneralParticleHandler.SpawnParticle(blast1);
            Particle blast2 = new CustomPulse(Projectile.Center, Vector2.Zero, Color.OliveDrab * 0.5f, "CalamityMod/Particles/FlameExplosion", Vector2.One, Main.rand.NextFloat(-10f, 10f), 0.03f, 0.07f, 340);
            GeneralParticleHandler.SpawnParticle(blast2);
            Particle blast3 = new CustomPulse(Projectile.Center, Vector2.Zero, Color.DarkOliveGreen * 0.55f, "CalamityMod/Particles/SoftRoundExplosion", Vector2.One, Main.rand.NextFloat(-10f, 10f), 0.04f, 0.08f, 340);
            GeneralParticleHandler.SpawnParticle(blast3);
            #endregion
            Projectile explosion = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<BileExplosion>(), (int)(Projectile.damage * 0.4f), Projectile.knockBack, Projectile.owner);
            if (explosion.whoAmI.WithinBounds(Main.maxProjectiles))
            {
                explosion.Calamity().stealthStrike = Projectile.Calamity().stealthStrike;
            }
        }
    }
}
