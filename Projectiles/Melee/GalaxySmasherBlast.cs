using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Graphics.Metaballs;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Melee
{
    public class GalaxySmasherBlast : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Melee";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public Player Owner => Main.player[Projectile.owner];
        private static float ExplosionRadius = 868f;

        public override void SetDefaults()
        {
            //These shouldn't matter because its circular
            Projectile.width = 868;
            Projectile.height = 868;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 18;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }
        public override void AI()
        {
            // Pureley visual stuff
            float fade = Utils.GetLerpValue(-2, 18, Projectile.timeLeft);
            float numberOfDusts = 8f;
            for (int i = 0; i < numberOfDusts; i++)
            {
                Vector2 velOffset = new Vector2(42, 42).RotatedByRandom(100) * Main.rand.NextFloat(0.5f, 1f);

                GalaxyMetaball.SpawnParticle(Projectile.Center, velOffset * fade, 300f * Main.rand.NextFloat(0.8f, 1f) * fade);
                if (i % 2 == 0)
                {
                    Vector2 velOffset2 = new Vector2(92, 92).RotatedByRandom(100) * Main.rand.NextFloat(0.5f, 1f);
                    GalaxyMetaball.SpawnParticle(Projectile.Center, velOffset2 * fade, 120f * Main.rand.NextFloat(0.7f, 1f) * fade);
                }
            }
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            target.AddBuff(ModContent.BuffType<GodSlayerInferno>(), 180);
            float minMult = 0.15f;
            int hitsToMinMult = 12;
            float damageMult = Utils.Remap(Projectile.numHits, 0, hitsToMinMult, 1, minMult, true);
            modifiers.SourceDamage *= damageMult;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            // If you are hitting an armored target or kill a target, don't reduce damage based on enemy hits
            if ((damageDone <= 2 || (target.life <= 0 && target.realLife == -1)) && Projectile.numHits > 0)
            {
                Projectile.numHits -= 1;
            }
        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => CalamityUtils.CircularHitboxCollision(Projectile.Center, ExplosionRadius, targetHitbox);
    }
}
