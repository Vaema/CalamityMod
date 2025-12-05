using System.Collections.Generic;
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
    // Photoviscerator left click main projectile (the flamethrower itself)
    public class ExoFire : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Ranged";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public ref float LightPower => ref Projectile.ai[2];

        public Color sparkColor;
        public int Time = 0;
        public ref int audioCooldown => ref Main.player[Projectile.owner].Calamity().PhotoAudioCooldown;
        public ref int PhotoTimer => ref Main.player[Projectile.owner].Calamity().PhotoTimer;

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 30;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.penetrate = -1;
            Projectile.MaxUpdates = 180;
            Projectile.timeLeft = 240;
            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.idStaticNPCHitCooldown = 6;
        }

        public override void AI()
        {
            Player Owner = Main.player[Projectile.owner];
            bool photosens = CalamityClientConfig.Instance.Photosensitivity;
            float targetDist = Vector2.Distance(Owner.Center, Projectile.Center);
            List<Color> eColors = new List<Color>()
            {
                Color.OrangeRed,
                photosens ? Color.DodgerBlue : Color.MediumTurquoise,
                Color.Orange,
                Color.LawnGreen
            };
            float rate = Main.GlobalTimeWrappedHourly * (photosens ? 2 : 8);
            int colorIndex = (int)(rate / 2 % eColors.Count);
            Color currentColor = eColors[colorIndex];
            Color nextColor = eColors[(colorIndex + 1) % eColors.Count];
            sparkColor = Color.Lerp(currentColor, nextColor, rate % 2f > 1f ? 1f : rate % 1f);
            if (photosens)
                sparkColor.A = 64;

            Time++;
            Lighting.AddLight(Projectile.Center, Color.White.ToVector3() * 0.2f);

            if (targetDist < 1400f)
            {
                //if (PhotoTimer == 0)
                //PhotoMetaball3.SpawnParticle(Projectile.Center + Owner.velocity, 42 - Time * 0.165f);
                //if (PhotoTimer == 1)
                //PhotoMetaball3.SpawnParticle(Projectile.Center + Owner.velocity, (37 - Time * (PhotoTimer == 0 ? 0.165f : 0.088f)) - PhotoTimer * 0.2f + (PhotoTimer == 1 ? 20 : 0)); ;

                //PhotoMetaball4.SpawnParticle(Projectile.Center + Owner.velocity, (37 - Time * (PhotoTimer == 0 ? 0.165f : 0.088f)) - PhotoTimer * 0.2f + (PhotoTimer == 1 ? 20 : 0));

                // I hate metaballs >:(
                Particle beam3 = new CustomSpark(Projectile.Center, Projectile.velocity * Main.rand.NextFloat(0.5f, 4f), "CalamityMod/Particles/SmallBloom", false, 4, ((37 - Time * (PhotoTimer == 0 ? 0.165f : 0.088f)) - PhotoTimer * 0.2f + (PhotoTimer == 1 ? 20 : 0)) * 0.005f, sparkColor, new Vector2(1f, 1 + Time * 0.1f), true, false);
                GeneralParticleHandler.SpawnParticle(beam3);
                if (!photosens)
                {
                    Particle beam32 = new CustomSpark(Projectile.Center, Projectile.velocity * Main.rand.NextFloat(0.5f, 4f), "CalamityMod/Particles/SmallBloom", false, 4, ((37 - Time * (PhotoTimer == 0 ? 0.165f : 0.088f)) - PhotoTimer * 0.2f + (PhotoTimer == 1 ? 20 : 0)) * 0.003f, Color.Lerp(Color.Blue, sparkColor, 0.5f), new Vector2(1f, 1 + Time * 0.1f), true, false);
                    GeneralParticleHandler.SpawnParticle(beam32);
                }
            }
            if (Main.rand.NextBool(35) && targetDist < 1400f && Time > 5)
            {
                Dust dust = Dust.NewDustPerfect(Projectile.Center, DustID.PortalBolt, new Vector2(0, -5).RotatedByRandom(0.05f) * Main.rand.NextFloat(0.3f, 1.6f));
                dust.noGravity = true;
                dust.scale = Main.rand.NextFloat(0.3f, 1f);
                dust.color = sparkColor;
            }
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<MiracleBlight>(), 300);
            if (audioCooldown == 0)
            {
                SoundEngine.PlaySound(Photoviscerator.HitSound, target.Center);
                audioCooldown = 16;
            }
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info) => target.AddBuff(ModContent.BuffType<MiracleBlight>(), 300);

    }
}
