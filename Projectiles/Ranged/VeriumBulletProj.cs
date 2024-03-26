using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Ranged
{
    public class VeriumBulletProj : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Ranged";
        private float speed = 0f;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 18;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 4;
            Projectile.height = 4;
            Projectile.aiStyle = ProjAIStyleID.Arrow;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 2;
            Projectile.timeLeft = 600;
            Projectile.extraUpdates = 1;
            AIType = ProjectileID.Bullet;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 12;
            Projectile.Calamity().pointBlankShotDuration = CalamityGlobalProjectile.DefaultPointBlankDuration;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (Projectile.localAI[0] > 7f && Projectile.penetrate > 1)
                CalamityUtils.DrawAfterimagesFromEdge(Projectile, 0, Color.Plum);
            return true;
        }

        public override bool PreAI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.ToRadians(90f);
            Projectile.spriteDirection = Projectile.direction;

            Projectile.localAI[0] += 1f;

            if (Projectile.ai[0] > 0f)
                Projectile.ai[0]--;
            if (speed == 0f)
                speed = Projectile.velocity.Length();
            if (Projectile.penetrate == 1 && Projectile.ai[0] <= 0f)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, Main.rand.NextBool(4) ? 226 : 303, -Projectile.velocity * Main.rand.NextFloat(0.05f, 0.3f));
                dust.noGravity = true;
                if (dust.type == 226)
                    dust.scale = Main.rand.NextFloat(0.15f, 0.25f);
                else
                {
                    dust.color = Color.Cyan;
                    dust.scale = Main.rand.NextFloat(0.25f, 0.45f);
                }

                float inertia = 15f;
                Vector2 center = Projectile.Center;
                float maxDistance = 300f;
                bool homeIn = false;

                int targetIndex = (int)Projectile.ai[1];
                NPC target = Main.npc[targetIndex];
                if (target.CanBeChasedBy(Projectile, false))
                {
                    float extraDistance = (target.width / 2) + (target.height / 2);

                    bool canHit = true;
                    if (extraDistance < maxDistance)
                        canHit = Collision.CanHit(Projectile.Center, 1, 1, target.Center, 1, 1);

                    if (Vector2.Distance(target.Center, Projectile.Center) < (maxDistance + extraDistance) && canHit)
                    {
                        center = target.Center;
                        homeIn = true;
                    }
                }

                if (!homeIn)
                {
                    for (int i = 0; i < Main.maxNPCs; i++)
                    {
                        NPC npc = Main.npc[i];
                        if (npc.CanBeChasedBy(Projectile, false))
                        {
                            float extraDistance = (npc.width / 2) + (npc.height / 2);

                            bool canHit = true;
                            if (extraDistance < maxDistance)
                                canHit = Collision.CanHit(Projectile.Center, 1, 1, npc.Center, 1, 1);

                            if (Vector2.Distance(npc.Center, Projectile.Center) < (maxDistance + extraDistance) && canHit)
                            {
                                center = npc.Center;
                                homeIn = true;
                                break;
                            }
                        }
                    }
                }

                if (!Projectile.friendly)
                {
                    homeIn = false;
                }

                if (homeIn)
                {
                    Vector2 moveDirection = Projectile.SafeDirectionTo(center, Vector2.UnitY);
                    Projectile.velocity = (Projectile.velocity * inertia + moveDirection * speed) / (inertia + 1f);
                }
                return false;
            }
            else if (Projectile.localAI[0] > 4f && Main.rand.NextBool())
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, Main.rand.NextBool(5) ? 223 : 303, -Projectile.velocity * Main.rand.NextFloat(0.05f, 0.5f));
                dust.noGravity = true;
                if (dust.type == 223)
                    dust.scale = Main.rand.NextFloat(0.35f, 0.55f);
                else
                {
                    dust.color = Color.Cyan;
                    dust.scale = Main.rand.NextFloat(0.45f, 0.65f);
                }
            }
            return true;
        }
        // This projectile is always fullbright.
        public override Color? GetAlpha(Color lightColor)
        {
            return Projectile.penetrate == 1 ? Color.LightSkyBlue : Color.Plum;
        }

        public override bool? CanHitNPC(NPC target) => Projectile.ai[0] <= 0f && target.CanBeChasedBy(Projectile);

        public override bool CanHitPvp(Player target) => Projectile.ai[0] <= 0f;

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Projectile.ai[0] = 10f;
            Projectile.damage /= 2;
            if (target.life > 0)
                Projectile.ai[1] = target.whoAmI;
            Projectile.velocity = Projectile.velocity.RotatedByRandom(0.35f) * Main.rand.NextFloat(0.98f, 1.02f);
            if (Projectile.penetrate == 1)
            {
                for (int k = 0; k < 4; k++)
                {
                    Dust dust = Dust.NewDustPerfect(Projectile.Center, 223, Projectile.velocity.RotatedByRandom(0.5) * Main.rand.NextFloat(0.1f, 0.9f));
                    dust.noGravity = true;
                    dust.scale = Main.rand.NextFloat(0.5f, 0.7f);
                }
            }
            else
            {
                Projectile.tileCollide = false;
                for (int k = 0; k < 2; k++)
                {
                    Dust dust = Dust.NewDustPerfect(Projectile.Center, 226, Projectile.velocity.RotatedByRandom(0.5) * Main.rand.NextFloat(0.1f, 0.9f));
                    dust.noGravity = true;
                    dust.scale = Main.rand.NextFloat(0.5f, 0.7f);
                }
                SoundEngine.PlaySound(SoundID.DD2_WitherBeastCrystalImpact with { PitchVariance = 0.2f, Pitch = 0.8f, Volume = 0.4f }, Projectile.Center);
            }
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            Projectile.ai[0] = 10f;
            Projectile.damage /= 2;
        }
    }
}
