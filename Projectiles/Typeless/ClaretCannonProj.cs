using CalamityMod.Balancing;
using System;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Dusts;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityMod.Items.Weapons.Typeless;
using CalamityMod.Projectiles.Healing;
using CalamityMod.Particles;

namespace CalamityMod.Projectiles.Typeless
{
    public class ClaretCannonProj : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Typeless";

        public override void SetDefaults()
        {
            Projectile.width = 14;
            Projectile.height = 14;
            Projectile.friendly = true;
            Projectile.alpha = 0;
            Projectile.penetrate = 1;
            Projectile.DamageType = AverageDamageClass.Instance;
            Projectile.MaxUpdates = 2;
            Projectile.timeLeft = 120 * Projectile.MaxUpdates;
            Projectile.aiStyle = 0;
        }

        public override void AI()
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            Lighting.AddLight(Projectile.Center, (255 - Projectile.alpha) * 0.5f / 255f, (255 - Projectile.alpha) * 0f / 255f, (255 - Projectile.alpha) * 0f / 255f);
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);
            SoundEngine.PlaySound(SoundID.Dig, Projectile.position);
            return true;
        }

        public override void OnKill(int timeLeft)
        {
            if (Projectile.penetrate != -1)
                return;
            SoundEngine.PlaySound(new SoundStyle("CalamityMod/Sounds/NPCKilled/PerfLargeDeath") { Volume = 0.5f }, Projectile.Center);
            SoundEngine.PlaySound(new SoundStyle("CalamityMod/Sounds/Custom/BloodPactCrit") { Volume = 0.5f }, Projectile.Center);
        }

         public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<Laceration>(), ClaretCannon.ClaretCooldownMax / 2);
            target.AddBuff(BuffID.BetsysCurse, ClaretCannon.ClaretCooldownMax);
            if (Projectile.penetrate == -1)
                return;
            var player = Main.player[Projectile.owner];
            float orbCount = 20;
            for (var i = 0; i < orbCount; i++)
            {
                Projectile.NewProjectile(Projectile.GetSource_OnHit(target), Projectile.Center, -Projectile.velocity.SafeNormalize(Vector2.Zero).RotatedByRandom(1) * Main.rand.NextFloat(2.75f, 5.25f), ModContent.ProjectileType<BloodstoneHealOrb>(), 20, 0f, player.whoAmI);
            }
                Projectile.position = Projectile.Center;
                Projectile.Size = new Vector2(352);
                Projectile.Center = Projectile.position;
                Projectile.penetrate = -1;
                Projectile.extraUpdates = 0;
                Projectile.timeLeft = 2;
                Projectile.velocity *= 0;
                Projectile.damage /= 2;
                float particleScale = 8f;
                Particle bloodsplosion = new CustomPulse(Projectile.Center, Vector2.Zero, Color.DarkRed, "CalamityMod/Particles/DetailedExplosion", Vector2.One, Main.rand.NextFloat(-15f, 15f), 0.16f*particleScale/5f, 0.87f * particleScale / 5f, (int)(40 * 0.38f), false);
                GeneralParticleHandler.SpawnParticle(bloodsplosion);
                Particle bloodsplosion2 = new CustomPulse(Projectile.Center, Vector2.Zero, new Color(255, 32, 32), "CalamityMod/Particles/DustyCircleHardEdge", Vector2.One, Main.rand.NextFloat(-15f, 15f), 0.03f * particleScale / 5f, 0.155f * particleScale / 5f, 40);
                GeneralParticleHandler.SpawnParticle(bloodsplosion2);
        }
    }
}