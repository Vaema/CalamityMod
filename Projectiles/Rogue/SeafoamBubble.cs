using System;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Particles;
using CalamityMod.Projectiles.Typeless;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Rogue
{
    public class SeafoamBubble : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";
        public override string Texture => "CalamityMod/Projectiles/Typeless/CoralBubble";
        public ref float time => ref Projectile.ai[0];
        public ref float bubbleLevel => ref Projectile.ai[1]; // -1 = small bubble, 0 = fusing bubble, 1 = big bubble
        public bool startEffects = true; // Spawn effects
        public float radius => 40 * Projectile.scale;
        public bool canHit = false; // Only the small bubbles can directly hit enemies
        public Projectile fuseGoal; // The bubble to fuse with
        public NPC targeted; // The enemy to home in on as a small bubble
        public override void SetDefaults()
        {
            Projectile.width = 28;
            Projectile.height = 28;
            Projectile.friendly = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 300;
            Projectile.extraUpdates = 1;
            Projectile.tileCollide = false;
            Projectile.DamageType = RogueDamageClass.Instance;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
            Projectile.ArmorPenetration = 15;
        }
        public override void AI()
        {
            // All the on spawn effects, also happens when a mid bubble becomes a big bubble
            if (startEffects)
            {
                if (bubbleLevel == 0)
                {
                    canHit = false;
                    for (int i = 0; i < 5; i++)
                    {
                        Vector2 vel = (MathHelper.TwoPi * i / 5f).ToRotationVector2() * 6f * (i % 2 == 0 ? 0.88f : 1f);
                        Particle sparks = new VelChangingSpark(Projectile.Center, (vel * Main.rand.NextFloat(1.6f, 2.4f)).RotatedByRandom(0.35f), Vector2.UnitY * -6, "CalamityMod/Particles/BloomRing", Main.rand.Next(28, 32 + 1), Main.rand.NextFloat(0.15f, 0.25f), Color.Turquoise, new Vector2(1f, 1), true, false, 0, false, 0.065f, 0.13f);
                        GeneralParticleHandler.SpawnParticle(sparks);
                    }
                }
                if (bubbleLevel == 1)
                {
                    Projectile.timeLeft = 180;
                    Projectile.velocity = Vector2.UnitY * -3;
                    canHit = false;
                    for (int i = 0; i < 10; i++)
                    {
                        Vector2 vel = (MathHelper.TwoPi * i / 10f).ToRotationVector2() * 8f * (i % 2 == 0 ? 0.88f : 1f);
                        Particle sparks = new VelChangingSpark(Projectile.Center, (vel * Main.rand.NextFloat(1.6f, 2.4f)).RotatedByRandom(0.35f), Vector2.UnitY * -6, "CalamityMod/Particles/BloomRing", Main.rand.Next(28, 32 + 1), Main.rand.NextFloat(0.15f, 0.25f), Color.Turquoise, new Vector2(1f, 1), true, false, 0, false, 0.065f, 0.13f);
                        GeneralParticleHandler.SpawnParticle(sparks);
                    }
                }
                if (bubbleLevel == -1)
                {
                    Projectile.scale = 0.5f;
                    canHit = true;
                }
                startEffects = false;
            }

            // Mid
            if (bubbleLevel == 0)
            {
                float distance = 500;
                for (int x = 0; x < Main.maxProjectiles; x++)
                {
                    Projectile projectile = Main.projectile[x];
                    if (projectile != Projectile && Vector2.Distance(Projectile.Center, projectile.Center) < distance && projectile.active && projectile.type == ModContent.ProjectileType<SeafoamBubble>() && projectile.ai[1] == 0 && projectile.owner == Projectile.owner)
                    {
                        distance = Vector2.Distance(Projectile.Center, projectile.Center);
                        fuseGoal = projectile;
                    }
                }
                if (fuseGoal != null)
                {
                    if (Projectile.velocity.Length() < 5)
                        Projectile.velocity = Projectile.velocity * 0.99f + Utils.DirectionTo(Projectile.Center, fuseGoal.Center) * 0.03f;
                    else
                        Projectile.velocity *= 0.995f;

                    if (Utils.Distance(Projectile.Center, fuseGoal.Center) < radius)
                    {
                        if (fuseGoal.localAI[0] > Projectile.localAI[0])
                        {
                            fuseGoal.localAI[0] = -5;
                            fuseGoal.Kill();
                            Projectile.ai[1]++;
                            startEffects = true;
                        }
                    }

                    if (!fuseGoal.active || fuseGoal.ai[1] != 0)
                        fuseGoal = null;
                }
                else if (Projectile.velocity.Length() > 2)
                    Projectile.velocity *= 0.99f;
                    
                if (Projectile.velocity.Y > -2)
                    Projectile.velocity.Y -= 0.012f;
            }
            // Large
            if (bubbleLevel == 1)
            {
                Projectile.scale = 1.5f;
                Projectile.velocity *= 0.99f;
            }
            // Small
            if (bubbleLevel == -1)
            {
                if (targeted == null || !targeted.active || targeted.life <= 0)
                    targeted = Projectile.Center.ClosestNPCAt(500);
                if (time > 90)
                {
                    Projectile.extraUpdates = 2;
                    if (targeted == null)
                    {
                        if (Projectile.velocity.Y > -3)
                            Projectile.velocity.Y -= 0.02f;
                    }
                    else
                        CalamityUtils.HomeInOnSelectedNPC(Projectile, Projectile.Center.ClosestNPCAt(700), false, 0.18f, 7, 0.98f, accelerate: true);
                }
                else
                {
                    Projectile.velocity *= 0.99f;
                    Projectile.velocity.Y -= 0.027f;
                }
            }
            
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;
            Lighting.AddLight(Projectile.Center, Color.Turquoise.ToVector3() * 0.3f * Projectile.scale);
            time++;
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            target.AddBuff(ModContent.BuffType<RiptideDebuff>(), 60);
        }
        public override void OnKill(int timeLeft)
        {
            float miniBubbles = 0;
            if (bubbleLevel == 0)
                miniBubbles = 2;
            if (bubbleLevel == 1)
                miniBubbles = 5;
            if (bubbleLevel == -1)
            {
                SoundEngine.PlaySound(SoundID.Item54 with { MaxInstances = 6 }, Projectile.Center);
                for (int i = 0; i < 2; i++)
                {
                    Vector2 vel = (Vector2.One * 2).RotatedByRandom(Math.PI);
                    Particle sparks = new VelChangingSpark(Projectile.Center, (vel * Main.rand.NextFloat(1f, 2.4f)).RotatedByRandom(0.35f), Vector2.UnitY * -6, "CalamityMod/Particles/BloomRing", Main.rand.Next(28, 32 + 1), Main.rand.NextFloat(0.15f, 0.25f), Color.Turquoise, new Vector2(1f, 1), true, false, 0, false, 0.065f, 0.13f);
                    GeneralParticleHandler.SpawnParticle(sparks);
                }
            }
            else
            {
                SoundStyle sound = new("CalamityMod/Sounds/Custom/BubblyPop");
                SoundEngine.PlaySound(sound with { Volume = 0.5f, Pitch = Main.rand.NextFloat(0.7f, 0.9f) - Projectile.scale * 0.6f, MaxInstances = 10 }, Projectile.Center);

                float blastSize = 70 + 70 * bubbleLevel;
                float minMultiplier = 0.25f;
                int hitsToMinMult = 5;
                int debuff1 = ModContent.BuffType<RiptideDebuff>();
                int debuffTime = 180;
                if (Projectile.localAI[0] >= 0) // On kill but exclude small bubbles and bubbled that died in a fuse
                {
                    Projectile blast = Projectile.NewProjectileDirect(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<BasicBurst>(), Projectile.damage / 2, Projectile.knockBack, Projectile.owner, blastSize, minMultiplier, hitsToMinMult);
                    blast.localAI[0] = debuff1;
                    blast.localAI[1] = debuffTime;
                    blast.DamageType = RogueDamageClass.Instance;

                    for (int i = 0; i < miniBubbles; i++)
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, (Vector2.One * 1.5f).RotatedByRandom(MathHelper.Pi) * Main.rand.NextFloat(0.9f, 1.6f) + Vector2.UnitY * 0.5f, ModContent.ProjectileType<SeafoamBubble>(), (int)(Projectile.damage * 0.35f), Projectile.knockBack / 5, Projectile.owner, 0, -1);

                }
                Particle blastRing2 = new CustomPulse(Projectile.Center, Vector2.Zero, Color.Turquoise, "CalamityMod/Particles/BloomRing", Vector2.One, Main.rand.NextFloat(-10, 10), 0.3f * Projectile.scale, 0.65f * Projectile.scale, 20);
                GeneralParticleHandler.SpawnParticle(blastRing2);
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            float sine = (float)Math.Sin(time * Projectile.scale * 0.275f / MathHelper.Pi);
            float sizeMult = ((sine + 2) * 0.05f);
            Texture2D tex = ModContent.Request<Texture2D>("CalamityMod/Particles/HighResFoggyCircleHardEdge").Value;
            Texture2D tex2 = ModContent.Request<Texture2D>("CalamityMod/Particles/BloomCircle").Value;
            Texture2D tex3 = ModContent.Request<Texture2D>("CalamityMod/Particles/BloomRingThinLarge").Value;

            Vector2 squash = new Vector2(Utils.Remap(Projectile.velocity.Length(), 2, 6, 1, 0.7f), Utils.Remap(Projectile.velocity.Length(), 2, 6, 1, 2f));

            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, null, Color.DodgerBlue with { A = 0 }, Projectile.rotation, tex.Size() * 0.5f, squash * (Projectile.scale + sizeMult) * 0.03f, SpriteEffects.None, 0);

            Main.EntitySpriteDraw(tex3, Projectile.Center - Main.screenPosition, null, Color.Turquoise with { A = 0 }, Projectile.rotation, tex3.Size() * 0.5f, squash * (Projectile.scale + sizeMult) * 0.032f, SpriteEffects.None, 0);

            Main.EntitySpriteDraw(tex2, Projectile.Center - Main.screenPosition + (MathHelper.PiOver4.ToRotationVector2() * -11.5f * Projectile.scale), null, Color.White with { A = 0 } * 0.5f, Projectile.rotation, tex2.Size() * 0.5f, squash * (Projectile.scale) * 0.3f, SpriteEffects.None, 0);

            return false;
        }
        public override bool? CanCutTiles() => false;
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => canHit ? CalamityUtils.CircularHitboxCollision(Projectile.Center, radius, targetHitbox) : false;
    }
}
