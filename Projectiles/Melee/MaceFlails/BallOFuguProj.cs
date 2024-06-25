using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Projectiles.BaseProjectiles;
using CalamityMod.Projectiles.Melee;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Melee.MaceFlails
{
    public class BallOFuguProj : BaseMaceFlailProjectile
    {
        public override int AssociatedItemID => ModContent.ItemType<BallOFugu>();

        public static float MaxSpikeTime = 180f;
        public static float SpikeRate = 10f;
        public static float SpikeDamage => 0.6f;
        public static float SpikeKnockback => 0.2f;

        public ref float CurrentFlailState => ref Projectile.ai[0];
        public ref float SpikeTimer => ref Projectile.ai[2];

        public Player Owner => Main.player[Projectile.owner];

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 38;
            Projectile.ignoreWater = true;
            base.SetDefaults();
        }

        public override void SpinAI(float launchSpeed)
        {
            SpikeTimer++;

            // Spews spikes while spinning if timer exceeded
            if (Projectile.owner == Main.myPlayer && SpikeTimer > MaxSpikeTime && SpikeTimer % SpikeRate == 0f)
            {
                Vector2 velocity = Projectile.DirectionFrom(Owner.MountedCenter).SafeNormalize(Vector2.Zero).RotatedByRandom(MathHelper.ToRadians(15f)) * Main.rand.NextFloat(4.5f, 6.5f);
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, velocity, ModContent.ProjectileType<UrchinSpikeFugu>(), (int)(Projectile.damage * SpikeDamage), Projectile.knockBack * SpikeKnockback, Projectile.owner, -10f);
            }
            base.SpinAI(launchSpeed);
        }

        public override Action<Projectile> EffectBeforePullback => (proj) =>
        {
            int SpikeCount = (int)(MathHelper.Clamp(SpikeTimer, 0f, MaxSpikeTime) / SpikeRate);
            for (int i = 0; i < SpikeCount; i++)
            {
                Vector2 velocity = Main.rand.NextVector2Unit() * Main.rand.NextFloat(3.5f, 5f);
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, velocity, ModContent.ProjectileType<UrchinSpikeFugu>(), (int)(Projectile.damage * SpikeDamage), Projectile.knockBack * SpikeKnockback, Projectile.owner, -10f);
            }
            SpikeTimer = 0f;
            Projectile.netUpdate = true;
        };

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => target.AddBuff(BuffID.Poisoned, 180);

        public override bool PreDraw(ref Color lightColor)
        {
            float glowStrength = MathHelper.Clamp(SpikeTimer, 0f, MaxSpikeTime) / SpikeRate;
            Projectile.DrawBackglow(Color.Indigo * glowStrength, 0.15f);
            return base.PreDraw(ref lightColor);
        }
    }
}
