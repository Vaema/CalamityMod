using CalamityMod.Graphics.Metaballs;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Melee
{
    public class GalaxySmasherMini : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Melee";
        public override string Texture => "CalamityMod/Projectiles/Melee/GalaxySmasherMini";
        public static readonly SoundStyle HitSound = new("CalamityMod/Sounds/Item/WulfrumKnifeTileHit2") { Volume = 0.5f, PitchVariance = 0.7f };
        public float rotatehammer = 20f;

        public override void SetDefaults()
        {
            Projectile.width = 86;
            Projectile.height = 72;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.alpha = 80;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 30;
            Projectile.scale = 0.7f;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 60;
        }

        public override void AI()
        {
            Projectile.direction = Projectile.spriteDirection = Projectile.velocity.X > 0f ? 1 : -1;
            Projectile.rotation += MathHelper.ToRadians(rotatehammer) * Projectile.direction;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            /*
            for (int i = 0; i < 8; i++)
            {
                Vector2 spawnPosition = target.Center + Main.rand.NextVector2Circular(10f, 10f);
                StreamGougeMetaball.SpawnParticle(spawnPosition, Main.rand.NextVector2Circular(3f, 3f), 60f);

                float scale = MathHelper.Lerp(24f, 64f, CalamityUtils.Convert01To010(i / 8f));
                spawnPosition = target.Center + Projectile.velocity.SafeNormalize(Vector2.UnitY) * MathHelper.Lerp(-40f, 90f, i / 8f);
                Vector2 particleVelocity = Projectile.velocity.SafeNormalize(Vector2.UnitY).RotatedByRandom(0.23f) * Main.rand.NextFloat(2.5f, 9f);
                StreamGougeMetaball.SpawnParticle(spawnPosition, particleVelocity, scale);
            }
            */
        }
    }
}
