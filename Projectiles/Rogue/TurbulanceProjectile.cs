using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using CalamityMod.Particles;
using Terraria.Audio;
using CalamityMod.Items.Weapons.Rogue;
using Terraria.DataStructures;
using System;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;

namespace CalamityMod.Projectiles.Rogue
{
    public class TurbulanceProjectile : ModProjectile, ILocalizedModType
    {
        int StealthProjectileFrequency => 50;

        NPC npcToStickTo = null;
        Vector2 stickOffset;
        float Shake = 0f;

        public new string LocalizationCategory => "Projectiles.Rogue";
        public override string Texture => "CalamityMod/Items/Weapons/Rogue/Turbulance";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 6;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.timeLeft = 600;
            Projectile.extraUpdates = 2;
            Projectile.penetrate = 3;
            Projectile.ignoreWater = true;
            Projectile.DamageType = RogueDamageClass.Instance;
            Projectile.tileCollide = true;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.stopsDealingDamageAfterPenetrateHits = true;
        }

        public override void OnSpawn(IEntitySource source)
        {
            stickOffset = Vector2.Zero;
        }

        public override bool? CanHitNPC(NPC target)
        {
            if (Projectile.velocity == Vector2.Zero) return false;
            return base.CanHitNPC(target);
        }

        public override void AI()
        {
            Shake = MathHelper.Lerp(Shake, 0f, 0.1f);

            if (npcToStickTo != null)
            {
                if (!npcToStickTo.active || npcToStickTo.life <= 0) Projectile.Kill();
                Projectile.position = npcToStickTo.Center + (stickOffset * npcToStickTo.scale) + (Projectile.Size / 2);
            }

            if (!Projectile.Calamity().stealthStrike)
            {
                Projectile.velocity.Y += 0.05f;
                Projectile.velocity.X *= 0.995f;
            }
            else
            {
                Projectile.velocity *= 1.05f;

                if (Projectile.velocity == Vector2.Zero)
                {
                    Projectile.ai[1]++;

                    if (Projectile.ai[1] >= 50)
                    {
                        if (Projectile.ai[1] < 100)
                        {
                            Projectile.ai[2] = MathHelper.Lerp(Projectile.ai[2], 1f, 0.1f);
                        }
                        else if (Projectile.timeLeft < 50)
                        {
                            Projectile.ai[2] = MathHelper.Lerp(Projectile.ai[2], 1f, -0.1f);
                        }

                        if (Projectile.ai[1] % StealthProjectileFrequency == 0)
                        {
                            OnHitEffects();
                        }
                    }
                }
            }

            if (Projectile.velocity != Vector2.Zero)
            {
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.ToRadians(45f);
                if (Main.rand.NextBool(5))
                {
                    Dust dust = Dust.NewDustDirect(Projectile.position + Projectile.velocity, Projectile.width, Projectile.height, 187, Projectile.velocity.X * 0.5f, Projectile.velocity.Y * 0.5f, 100, new Color(53, Main.DiscoG, 255));
                    dust.noGravity = true;
                }
                if (Main.rand.NextBool(5))
                {
                    Dust dust = Dust.NewDustDirect(Projectile.position + Projectile.velocity, Projectile.width, Projectile.height, 16, Projectile.velocity.X * 0.5f, Projectile.velocity.Y * 0.5f);
                    dust.noGravity = true;
                }
            }

            Projectile.ai[0]++;
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.DD2_CrystalCartImpact, Projectile.Center);

            GeneralParticleHandler.SpawnParticle(new ImpactParticle(
                Projectile.Center, MathHelper.ToRadians(Main.rand.NextFloat(-2, 2)), 20, 2f, Color.SkyBlue));
            for (int i = 0; i < 3; i++)
            {
                GeneralParticleHandler.SpawnParticle(new ImpactParticle(
                    Projectile.Center + new Vector2(Main.rand.NextFloat(20, 50), 0).RotatedByRandom(MathHelper.TwoPi), MathHelper.ToRadians(Main.rand.NextFloat(-2, 2)), 20, 0.5f, Color.SkyBlue));
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Projectile.Calamity().stealthStrike)
            {
                SoundEngine.PlaySound(SoundID.DD2_CrystalCartImpact.WithPitchOffset(-0.9f), Projectile.Center);

                npcToStickTo = target;
                stickOffset = target.DirectionTo(Projectile.Center) * ((target.width + target.height) / 4);
                Projectile.velocity = Vector2.Zero;
            }
            OnHitEffects();
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            OnHitEffects();
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            OnHitEffects();
            if (Projectile.Calamity().stealthStrike)
            {
                SoundEngine.PlaySound(SoundID.DD2_CrystalCartImpact.WithPitchOffset(-0.7f), Projectile.Center);;
                Projectile.velocity = Vector2.Zero;
                stickOffset = oldVelocity * 1.7f;
            }
            else
            {
                return true;
            }
            return false;
        }

        private void OnHitEffects()
        {
            if (Projectile.owner == Main.myPlayer)
            {
                SoundEngine.PlaySound(Main.rand.NextBool(2) ? SoundID.Item48 : SoundID.Item49, Projectile.Center);
                Shake = 1f;
                for (int w = 0; w < (Projectile.Calamity().stealthStrike ? 3 : 2); w++)
                {
                    Vector2 velocity = CalamityUtils.RandomVelocity(100f, 70f, 100f);
                    int proj = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, velocity, ModContent.ProjectileType<TurbulanceWindSlash>(), Projectile.damage / 3, Projectile.knockBack / 3, Main.myPlayer);
                    Main.projectile[proj].Calamity().stealthStrike = Main.projectile[proj].tileCollide = Projectile.Calamity().stealthStrike;
                }
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Asset<Texture2D> tex = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Melee/WindBolt");
            Asset<Texture2D> texSmall = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Melee/WindBolt_Small");

            Asset<Texture2D> texSpear = ModContent.Request<Texture2D>(Texture);

            for (int i = 5; i >= 0; i--)
            {
                float c = Math.Max(i, 1);

                Asset<Texture2D> t = tex;
                if (i <= 2) t = texSmall;

                Main.EntitySpriteDraw(t.Value, Projectile.Center - Main.screenPosition, t.Frame(), lightColor.MultiplyRGB(Color.Lerp(Color.White, Color.CadetBlue, (float)i / 5)).MultiplyRGBA(new Color(1f / c, 1f / c, 1f / c, 1f / c)), -MathHelper.ToRadians(Projectile.ai[1]) * c, t.Size() / 2, (float)MathHelper.Lerp(0f, i * 0.7f, Projectile.ai[2]) / (t == texSmall ? 1 : 2), SpriteEffects.None);
            }

            Main.EntitySpriteDraw(texSpear.Value, Projectile.Center - Main.screenPosition + new Vector2(Main.rand.NextFloat(-5f * Shake, Shake * 5f), Main.rand.NextFloat(-5f * Shake, Shake * 5f)), texSpear.Frame(), lightColor, Projectile.rotation, texSpear.Size() / 2, 1f, SpriteEffects.None);
            return false;
        }
    }
}
