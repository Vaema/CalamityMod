using System;
using System.Linq;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Items.Weapons.Ranged;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Ranged
{
    public class HalleysStarburst : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Ranged";
        public override string Texture => "CalamityMod/Particles/Sparkle";
        Color drawColor = Color.Black;
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 6;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = 1;
            Projectile.MaxUpdates = 6;
            Projectile.timeLeft = 60 * Projectile.MaxUpdates;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.frame = Main.rand.Next(0, 6);
            Projectile.scale = 0.75f;
            Projectile.rotation += Main.rand.NextFloat(0, 3);
        }

        public override void AI()
        {
            if (drawColor == Color.Black)
            {
                
            }
            Projectile.rotation += Projectile.direction * 0.05f;
            if (Projectile.FinalExtraUpdate())
            {
                var star = new BloomParticle(Projectile.Center, Vector2.Zero, drawColor, 0.2f, 0.25f, 2, false);
                var star2 = new CustomSpark(Projectile.Center, Vector2.UnitX.RotatedBy(Projectile.rotation) * 0.1f, Texture, false, 2, 1f, Color.White, Vector2.One);
                GeneralParticleHandler.SpawnParticle(star);
                GeneralParticleHandler.SpawnParticle(star2);
            }
                Projectile.frameCounter++;
            if (Projectile.frameCounter > 5)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
            }
            if (Projectile.frame > 5)
                Projectile.frame = 0;
            if (Projectile.timeLeft == 1)
            {
                Main.player[Projectile.owner].Calamity().HalleyAccuracyCounter -= HalleysInferno.LostAccuracyPerMiss;
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            if (drawColor == Color.Black)
            {
                switch (Projectile.ai[0])
                {
                    case 1:
                        drawColor = Color.HotPink;
                        break;
                    case 2:
                        drawColor = Color.Yellow;
                        break;
                    case 3:
                        drawColor = Color.LimeGreen;
                        break;
                    case 4:
                        drawColor = Color.SkyBlue;
                        break;
                    case 5:
                        drawColor = Color.Lavender;
                        break;
                    case 6:
                        drawColor = Color.White;
                        break;
                }
            }
            lightColor = drawColor;
            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<Voidfrost>(), 30);
            SoundEngine.PlaySound(SoundID.DD2_CrystalCartImpact, Projectile.Center);

            // Dust emission on hit
            for (int i = 0; i < 14; i++)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, Main.rand.NextBool(3) ? 172 : 206, Projectile.velocity);
                dust.scale = Main.rand.NextFloat(1.1f, 1.9f);
                dust.velocity = Projectile.velocity.RotatedByRandom(0.5f) * Main.rand.NextFloat(0.2f, 2.1f);
                dust.noGravity = true;
            }
            var cplay = Main.player[Projectile.owner].Calamity();
            cplay.HalleyAccuracyCounter++;
            cplay.HalleyAccuracyCounter = MathF.Min(HalleysInferno.MaxAccuracy, cplay.HalleyAccuracyCounter);
            Main.player[Projectile.owner].Calamity().StarburstSpawnFrameCounter += cplay.HalleyAccuracyCounter / HalleysInferno.MaxAccuracy * HalleysInferno.MaxStarburstPerStar;
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            target.AddBuff(ModContent.BuffType<Nightwither>(), 450);
            SoundEngine.PlaySound(SoundID.DD2_CrystalCartImpact, Projectile.Center);

            // Dust emission on hit
            for (int i = 0; i < 14; i++)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, Main.rand.NextBool(3) ? 172 : 206, Projectile.velocity);
                dust.scale = Main.rand.NextFloat(1.1f, 1.9f);
                dust.velocity = Projectile.velocity.RotatedByRandom(0.5f) * Main.rand.NextFloat(0.2f, 2.1f);
                dust.noGravity = true;
            }
        }
    }
}
