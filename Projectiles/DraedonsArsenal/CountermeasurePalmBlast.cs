using System;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.DraedonsArsenal
{
    public class CountermeasurePalmBlast : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Misc";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public ref float time => ref Projectile.ai[0];
        public float fade => (float)Math.Pow(Utils.GetLerpValue(0, 13, Projectile.timeLeft, true), 4);
        public bool onSpawn = true;
        private NPC targeted = null;
        public override void SetDefaults()
        {
            Projectile.width = 150;
            Projectile.height = 150;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 15;
            Projectile.tileCollide = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, Effects.ArsenalEffects.ArsenalLaserColor.ToVector3() * 0.5f);
            Player Owner = Main.player[Projectile.owner];
            float targetDist = Vector2.Distance(Owner.Center, Projectile.Center);
            if (Projectile.ai[1] != -5)
                targeted = Main.npc[(int)Projectile.ai[1]];
            if (targeted == null || !targeted.active)
            {
                targeted = Projectile.Center.ClosestNPCAt(150);
                Projectile.ai[1] = -5;
            }
            if (time == 0)
            {
                Vector2 launchVel = Projectile.velocity.SafeNormalize(Vector2.UnitX);
                float launchPower = 25;
                targeted.MoveNPC(launchVel, launchPower, true);

                Particle bloom = new CustomSpark(Projectile.Center, Projectile.velocity.SafeNormalize(Vector2.UnitX) * 10, "CalamityMod/Particles/BloomCircle", false, 12, 0.7f, Effects.ArsenalEffects.ArsenalLaserColor, new Vector2(1f, 3f), true, true, shrinkSpeed: -0.5f, extraRotation: MathHelper.PiOver2);
                GeneralParticleHandler.SpawnParticle(bloom);
            }
            time++;
        }

        public override bool? CanHitNPC(NPC target) => (target == targeted) ? null : false;

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.SetCrit();
            Player Owner = Main.player[Projectile.owner];
            float critDamage = Math.Min(Owner.GetTotalCritChance(Projectile.DamageType) * 0.01f, 1f);
            float minMult = 0.25f;
            int hitsToMinMult = 7;
            float damageMult = Utils.Remap(Projectile.numHits, 0, hitsToMinMult, 1, minMult, true);
            modifiers.SourceDamage *= damageMult + critDamage;

            Vector2 launchVel = Projectile.velocity.SafeNormalize(Vector2.UnitX);
            float launchPower = 40;
            target.MoveNPC(launchVel, launchPower, true);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            Texture2D beam = ModContent.Request<Texture2D>("CalamityMod/Particles/BloomLineFade").Value;
            Texture2D bloom = ModContent.Request<Texture2D>("CalamityMod/Particles/BloomCircle").Value;
            float completion = Utils.GetLerpValue(15, 0, time);
            for (int i = 0; i < 6; i++)
            {
                Color orbColor = Color.Lerp(Effects.ArsenalEffects.ArsenalLaserColor, Color.White, i * 0.04f) with { A = 0 } * 0.8f;
                float scale = Projectile.scale * (0.05f + i * 0.01f) * 3;
                
                Main.EntitySpriteDraw(beam, Projectile.Center - Main.screenPosition, null, orbColor, Projectile.rotation, new Vector2(beam.Width / 2, beam.Height), new Vector2(0.3f * fade, 2 * completion) * scale, SpriteEffects.None);
                Main.EntitySpriteDraw(bloom, Projectile.Center - Main.screenPosition, null, orbColor, Projectile.rotation + MathHelper.PiOver2, bloom.Size() * 0.5f, new Vector2(0.5f, 1 + 2 * completion) * scale * 3 * fade, SpriteEffects.None);
            }

            return false;
        }
    }
}
