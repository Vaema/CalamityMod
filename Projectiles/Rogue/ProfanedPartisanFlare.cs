using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Items.Weapons.Rogue;
using CalamityMod.NPCs.Providence;
using CalamityMod.Particles;
using CalamityMod.Projectiles.Boss;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Graphics.PackedVector;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Rogue
{
    public class ProfanedPartisanFlare : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";
        public int timer = 0;
        public override string Texture => "CalamityMod/Projectiles/Boss/HolyFire";
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 4;
            ProjectileID.Sets.CultistIsResistantTo[Type] = true;
        }
        public override void SetDefaults()
        {
            Projectile.width = 20;
            Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.DamageType = RogueDamageClass.Instance;
            Projectile.penetrate = 3;
            Projectile.extraUpdates = 0;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
            Projectile.timeLeft = 600;
            Projectile.tileCollide = false;
        }

        public override bool? CanHitNPC(NPC target)
        {
            return true;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<HolyFlames>(), 120);
        }

        NPC target = null;
        public override void AI()
        {

            if (Projectile.originalDamage == 0)
            {
                Projectile.originalDamage = ProfanedPartisan.ShardBaseDamage;
                Projectile.ContinuouslyUpdateDamageStats = true;
            }
            if (target != null && Projectile.localNPCImmunity[target.whoAmI] <= 0 && target.active && !target.dontTakeDamage)
            {
                Projectile.Calamity().HomingTarget = target.whoAmI;
                var dis = Projectile.Distance(target.Center);
                Projectile.velocity = Projectile.velocity.ToRotation().AngleLerp(Projectile.DirectionTo(target.Center).ToRotation(), (Projectile.Distance(target.Center) < 160 ? 0.1f + (1 - dis/160)*0.4f : 0.1f)).ToRotationVector2() * Projectile.velocity.Length();
            }
            else
            {
                target = GetTargetInRange(2000);
            }
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
        }
        public override void OnKill(int timeLeft)
        {
            Color hiColor = new Color(255, 155, 25, 255);
            Color loColor = new Color(255, 0, 0, 0);

            for (int i = 0; i < 25; i++)
            {
                GeneralParticleHandler.SpawnParticle(new GlowOrbParticle(Projectile.Center, new Vector2(Main.rand.NextFloat(10), 0).RotatedByRandom(MathHelper.TwoPi), false, 10, Main.rand.NextFloat(0.8f, 1.2f), hiColor));
            }

            GeneralParticleHandler.SpawnParticle(new CustomPulse(Projectile.Center, Vector2.Zero, Color.White, "CalamityMod/Particles/BloomCircle", Vector2.One, 0f, 0.5f, 0.1f, 4));

            for (float i = 0; i < 1; i += 0.25f)
            {
                GeneralParticleHandler.SpawnParticle(new CustomPulse(Projectile.Center, Vector2.Zero, hiColor, "CalamityMod/Particles/SoftRoundExplosion", Vector2.One, Main.rand.NextFloat(MathHelper.TwoPi), 0.02f * i, 0.075f * i, 24));
            }
            GeneralParticleHandler.SpawnParticle(new CustomPulse(Projectile.Center, Vector2.Zero, hiColor, "CalamityMod/Particles/ShatteredExplosion", Vector2.One, Main.rand.NextFloat(MathHelper.TwoPi), 0.02f, 0.045f, 16));

            SoundEngine.PlaySound(SoundID.DD2_BetsyFireballImpact.WithPitchOffset(0.6f), Projectile.Center);
            SoundEngine.PlaySound(SoundID.Item100.WithPitchOffset(0.4f), Projectile.Center);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = Terraria.GameContent.TextureAssets.Projectile[Type].Value;
            var frame = tex.Frame(1, Main.projFrames[Type], 0, Projectile.frame);
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, frame, Color.White, Projectile.rotation, frame.Size() / 2f, Projectile.scale, SpriteEffects.None, 0);
            return false;
        }

        NPC GetTargetInRange(float range)
        {
            var player = Main.player[Projectile.owner];
            {
                NPC gotTarget = null;
                float currentDistance = range;
                foreach (var npc in Main.ActiveNPCs)
                {
                    if (Projectile.localNPCImmunity[npc.whoAmI] > 0)
                        continue;
                    var myDistance = npc.Distance(Projectile.Center);

                    if (npc.CanBeChasedBy() && myDistance < currentDistance)
                    {
                        currentDistance = myDistance;
                        gotTarget = npc;
                    }
                }
                return gotTarget;
            }
        }
    }
}
