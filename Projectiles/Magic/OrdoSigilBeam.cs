using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Ranged
{
    public class OrdoSigilBeam : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Magic";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public ref float Time => ref Projectile.ai[0];

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = 5;
            Projectile.MaxUpdates = 15;
            Projectile.timeLeft = 20 * Projectile.MaxUpdates;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation();
            Time++;

            // Render the needle beam as a pair of severely stretched spark particles.
            // This has to be done only on specific extra updates or it'll instantly overload the particle limit.
            bool isDrawingUpdate = Projectile.numUpdates % 3 == 0;
            if (Time > 6f && isDrawingUpdate)
            {
                Color outerSparkColor = new Color(255, 255, 255);
                float scaleBoost = MathHelper.Clamp(Time * 0.03f, 0f, 2f);
                float outerSparkScale = 3.2f + scaleBoost;
                SparkParticle spark = new SparkParticle(Projectile.Center, Projectile.velocity, false, 7, outerSparkScale, outerSparkColor);
                GeneralParticleHandler.SpawnParticle(spark);

                Color innerSparkColor = new Color(181, 181, 181);
                float innerSparkScale = 1.6f + scaleBoost;
                SparkParticle spark2 = new SparkParticle(Projectile.Center, Projectile.velocity, false, 7, innerSparkScale, innerSparkColor);
                GeneralParticleHandler.SpawnParticle(spark2);
            }

            if (Projectile.FinalExtraUpdate())
                Lighting.AddLight(Projectile.Center, Color.MediumBlue.ToVector3() * 0.4f);
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.SetCrit();
        }

    }
}
