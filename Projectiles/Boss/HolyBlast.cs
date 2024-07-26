using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CalamityMod.Graphics.Primitives;
using CalamityMod.NPCs;
using CalamityMod.NPCs.Providence;
using CalamityMod.Particles;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Boss
{
    public class HolyBlast : ModProjectile, ILocalizedModType
    {
        public bool started = false;

        public new string LocalizationCategory => "Projectiles.Boss";
        public static readonly SoundStyle ShootSound = new("CalamityMod/Sounds/Custom/Providence/ProvidenceHolyBlastShoot");
        public static readonly SoundStyle ImpactSound = new("CalamityMod/Sounds/Custom/Providence/ProvidenceHolyBlastImpact");

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 4;
        }

        public override void OnSpawn(IEntitySource source)
        {
        }

        public override void SetDefaults()
        {
            Projectile.Calamity().DealsDefenseDamage = true;
            Projectile.width = 180;
            Projectile.height = 180;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 120;
            CooldownSlot = ImmunityCooldownID.Bosses;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(Projectile.localAI[0]);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            Projectile.localAI[0] = reader.ReadSingle();
        }

        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, 0.9f, 0.7f, 0f);

            if (Projectile.Hitbox.Intersects(new Rectangle((int)Projectile.ai[0], (int)Projectile.ai[1], Player.defaultWidth, Player.defaultHeight)))
                Projectile.tileCollide = true;

            Projectile.maxPenetrate = (int)Providence.BossMode.Day;
            // Day mode by default but syncs with the boss
            if (CalamityGlobalNPC.holyBoss != -1)
            {
                if (Main.npc[CalamityGlobalNPC.holyBoss].active)
                    Projectile.maxPenetrate = (int)Main.npc[CalamityGlobalNPC.holyBoss].localAI[1];
            }
            else if (CalamityGlobalNPC.doughnutBoss != -1)
            {
                if (Main.npc[CalamityGlobalNPC.doughnutBoss].active)
                {
                    if (Main.npc[CalamityGlobalNPC.doughnutBoss].Calamity().CurrentlyEnraged)
                        Projectile.maxPenetrate = (int)Providence.BossMode.Night;
                    else
                        Projectile.maxPenetrate = (int)Providence.BossMode.Day;
                }
            }
            else
                Projectile.maxPenetrate = (int)Providence.BossMode.Day;

            Projectile.frameCounter++;
            if (Projectile.frameCounter > 6)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
            }
            if (Projectile.frame > 3)
                Projectile.frame = 0;

            if (!started)
            {
                for (int i = 0; i < 6; i++)
                {
                    GeneralParticleHandler.SpawnParticle(new PointParticle(Projectile.Center, Projectile.velocity.RotatedBy(MathHelper.ToRadians(Main.rand.NextFloat(-30f, 30f))) * Main.rand.NextFloat(2.4f, 4.2f), false, 30, Main.rand.NextFloat(3f, 6f), ProvUtils.GetProjectileColor(Projectile.maxPenetrate, 255, false)));
                    GeneralParticleHandler.SpawnParticle(new SparkParticle(Projectile.Center, Projectile.velocity.RotatedBy(MathHelper.ToRadians(Main.rand.NextFloat(-50f, 50f))) * Main.rand.NextFloat(2.4f, 4.2f), false, 30, Main.rand.NextFloat(2f, 5f), ProvUtils.GetProjectileColor(Projectile.maxPenetrate, 255, false)));
                }
                started = true;
            }

            if (Projectile.localAI[0] == 0f)
            {
                int dustType = ProvUtils.GetDustID(Projectile.maxPenetrate);
                for (int i = 0; i < 10; i++)
                {
                    int holyDust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, dustType, 0f, 0f, 100, default, 2f);
                    Main.dust[holyDust].velocity *= 3f;
                    Main.dust[holyDust].noGravity = true;
                    if (Main.rand.NextBool())
                    {
                        Main.dust[holyDust].scale = 0.5f;
                        Main.dust[holyDust].fadeIn = 1f + Main.rand.Next(10) * 0.1f;
                    }
                }
                Projectile.localAI[0] = 1f;
                SoundEngine.PlaySound(ShootSound, Projectile.Center);
            }
           
            if (Main.rand.NextBool(3))
                GeneralParticleHandler.SpawnParticle(new SparkParticle(Projectile.Center + Projectile.velocity + new Vector2(Main.rand.NextFloat(-40, 40), 0).RotatedBy(Vector2.Zero.AngleTo(Projectile.velocity.RotatedBy(MathHelper.PiOver2))), Projectile.velocity * Main.rand.NextFloat(-3f, -1f), false, 10, Main.rand.NextFloat(1f, 1.5f), ProvUtils.GetProjectileColor(Projectile.maxPenetrate, 255, false)));

            if (Math.Abs(Projectile.velocity.X) > 0.2)
                Projectile.spriteDirection = -Projectile.direction;

            if (Projectile.velocity.X < 0f)
                Projectile.rotation = (float)Math.Atan2(Projectile.velocity.Y, Projectile.velocity.X);
            else
                Projectile.rotation = (float)Math.Atan2(Projectile.velocity.Y, Projectile.velocity.X);
        }

        public override Color? GetAlpha(Color lightColor)
        {
            return ProvUtils.GetProjectileColor(Projectile.maxPenetrate, lightColor);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = (Projectile.maxPenetrate == (int)Providence.BossMode.Day) ? Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value : ModContent.Request<Texture2D>("CalamityMod/Projectiles/Boss/HolyBlastNight").Value;
            int framing = texture.Height / Main.projFrames[Projectile.type];
            int y6 = framing * Projectile.frame;
            Vector2 sc = Vector2.One;
            Projectile.DrawBackglow(ProvUtils.GetProjectileColor(Projectile.maxPenetrate, lightColor, true), 4f, sc, texture);

            Main.spriteBatch.Draw(texture, Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY), new Microsoft.Xna.Framework.Rectangle?(new Rectangle(0, y6, texture.Width, framing)), Projectile.GetAlpha(lightColor), Projectile.rotation, new Vector2(texture.Width / 2f, framing / 2f), sc, SpriteEffects.None, 0);
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            Color hiColor = ProvUtils.GetProjectileColor(Projectile.maxPenetrate, 255, false);
            Color loColor = ProvUtils.GetProjectileColor(Projectile.maxPenetrate, 0, true);

            for (int i = 0; i < 30; i++)
            {
                Particle p = new FlameParticle(Projectile.Center + new Vector2(Main.rand.NextFloat(150), 0).RotatedByRandom(MathHelper.TwoPi), 40, Main.rand.NextFloat(0.5f, 0.75f), Main.rand.NextFloat(1f, 2.5f), hiColor, loColor);
                p.Velocity = new Vector2(Main.rand.NextFloat(3f, 19f), 0).RotatedByRandom(MathHelper.TwoPi);
                GeneralParticleHandler.SpawnParticle(p);
                GeneralParticleHandler.SpawnParticle(new HeavySmokeParticle(Projectile.Center, new Vector2(Main.rand.NextFloat(12f, 40f), 0).RotatedByRandom(MathHelper.TwoPi), loColor, 60, Main.rand.NextFloat(0.75f, 1.75f), 1f, Main.rand.NextFloat(-0.05f, 0.05f), true));
            }

            GeneralParticleHandler.SpawnParticle(new CustomPulse(Projectile.Center, Vector2.Zero, hiColor, "CalamityMod/Particles/BloomCircle", Vector2.One, Main.rand.NextFloat(MathHelper.TwoPi), 1f, 0.1f, 15));

            GeneralParticleHandler.SpawnParticle(new CustomPulse(Projectile.Center, Vector2.Zero, hiColor, "CalamityMod/Particles/SoftRoundExplosion", Vector2.One, Main.rand.NextFloat(MathHelper.TwoPi), 0.05f, 0.25f, 35));
            GeneralParticleHandler.SpawnParticle(new CustomPulse(Projectile.Center, Vector2.Zero, hiColor, "CalamityMod/Particles/ShatteredExplosion", Vector2.One, Main.rand.NextFloat(MathHelper.TwoPi), 0.05f, 0.175f, 25));

            if (Projectile.owner == Main.myPlayer)
            {
                int totalProjectiles = (Projectile.maxPenetrate != (int)Providence.BossMode.Day) ? 8 : 6;
                if (CalamityWorld.LegendaryMode && CalamityWorld.revenge)
                    totalProjectiles *= 2;

                float radians = MathHelper.TwoPi / totalProjectiles;
                int type = ModContent.ProjectileType<HolyFire2>();
                float velocity = 5f;
                Vector2 spinningPoint = new Vector2(0f, -velocity);
                for (int k = 0; k < totalProjectiles; k++)
                {
                    Vector2 velocity2 = spinningPoint.RotatedBy(radians * k);
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, velocity2 + Projectile.velocity * 0.25f, type, (int)Math.Round(Projectile.damage * 0.75), 0f, Projectile.owner);
                }
            }

            SoundEngine.PlaySound(ImpactSound, Projectile.Center);


            int dustType = ProvUtils.GetDustID(Projectile.maxPenetrate);
            for (int j = 0; j < 4; j++)
            {
                int dust = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, dustType, 0f, 0f, 50, default, 2f);
                Main.dust[dust].noGravity = true;
            }
            for (int k = 0; k < 40; k++)
            {
                int profaned = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, dustType, 0f, 0f, 0, default, 4f);
                Main.dust[profaned].noGravity = true;
                Main.dust[profaned].velocity *= 3f;
                profaned = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, dustType, 0f, 0f, 50, default, 2f);
                Main.dust[profaned].velocity *= 2f;
                Main.dust[profaned].noGravity = true;
            }
        }

        public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
        {
            //In GFB, "real damage" is replaced with negative healing
            if (Projectile.maxPenetrate >= (int)Providence.BossMode.Red)
                modifiers.SourceDamage *= 0f;
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            // If the player is dodging, don't apply debuffs
            if ((info.Damage <= 0 && Projectile.maxPenetrate < (int)Providence.BossMode.Red) || target.creativeGodMode)
                return;

            ProvUtils.ApplyHitEffects(target, Projectile.maxPenetrate, 320, 20);
        }
    }
}
