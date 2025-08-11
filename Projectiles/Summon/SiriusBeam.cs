using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Summon
{
    public class SiriusBeam : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Summon";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.MinionShot[Type] = true;
            ProjectileID.Sets.CultistIsResistantTo[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 4;

            Projectile.penetrate = 1;
            Projectile.extraUpdates = 3;
            Projectile.localNPCHitCooldown = -1;
            Projectile.timeLeft = 1000;

            Projectile.friendly = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.tileCollide = false;
            Projectile.DamageType = DamageClass.Summon;
        }

        public ref float Time => ref Projectile.ai[1];

        public override void AI()
        {
            
            if (Projectile.ai[0] < 1) //Normal homing bolt AI
            {
                for (int d = 0; d < 1; d++)
                {
                    Vector2 projPos = Projectile.position;
                    projPos -= Projectile.velocity * (d * 0.25f);
                    Projectile.alpha = 255;
                    int trailDust = Dust.NewDust(projPos, 1, 1, DustID.PurificationPowder, 0f, 0f, 0, default, 1f);
                    Main.dust[trailDust].position = projPos;
                    Main.dust[trailDust].scale = Main.rand.Next(70, 110) * 0.013f;
                    Main.dust[trailDust].noGravity = true;
                }

                Color outerSparkColor = Color.SkyBlue;

                GeneralParticleHandler.SpawnParticle(new CustomSpark(Projectile.Center, Projectile.velocity, "CalamityMod/Particles/BloomCircle", false, 10, 0.15f, outerSparkColor * 0.75f, Vector2.One));
                NPC target = Projectile.Center.MinionHoming(5000f, Main.player[Projectile.owner]); // Detects a target.
                                                                                                   // Move towards the target.
                if (target != null)
                {
                    Projectile.velocity = Vector2.Lerp(Projectile.velocity, (target.Center - Projectile.Center).SafeNormalize(Vector2.Zero) * 10f, 0.05f);
                    Projectile.netUpdate = true;
                }
            } else //Sirius Quazar AI
            {
                Projectile.rotation = Projectile.velocity.ToRotation();
                Time++;
                bool isDrawingUpdate = Projectile.numUpdates % 6 == 0;
                if (Time > 6f && isDrawingUpdate)
                {
                    Color outerSparkColor = new Color(8, 35, 156);
                    float scaleBoost = MathHelper.Clamp(Time * 0.005f, 0f, 2f);
                    float outerSparkScale = 3.2f + scaleBoost;
                    SparkParticle spark = new SparkParticle(Projectile.Center, Projectile.velocity, false, 7, outerSparkScale, outerSparkColor);
                    GeneralParticleHandler.SpawnParticle(spark);

                    Color innerSparkColor = new Color(184, 215, 245);
                    float innerSparkScale = 1.6f + scaleBoost;
                    SparkParticle spark2 = new SparkParticle(Projectile.Center, Projectile.velocity, false, 7, innerSparkScale, innerSparkColor);
                    GeneralParticleHandler.SpawnParticle(spark2);
                }
                for (int d = 0; d < 1; d++)
                {
                    Vector2 projPos = Projectile.position;
                    projPos -= Projectile.velocity * (d * 0.25f);
                    Projectile.alpha = 255;
                    int trailDust = Dust.NewDust(projPos, 1, 1, DustID.PurificationPowder, 0f, 0f, 0, default, 1f);
                    Main.dust[trailDust].position = projPos;
                    Main.dust[trailDust].scale = Main.rand.Next(70, 110) * 0.013f;
                    Main.dust[trailDust].velocity *= 0.2f;
                    Main.dust[trailDust].noGravity = true;
                }
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Projectile.ai[0] == 1) //Only does all the on-hit for the quazar
            {
                target.AddBuff(ModContent.BuffType<Voidfrost>(), 600);
                float x4 = Main.rgbToHsl(new Color(103, 203, Main.DiscoB)).X;
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<SiriusExplosion>(), Projectile.damage, Projectile.knockBack, Projectile.owner, x4, Projectile.whoAmI);
            }
        }
    }
}
