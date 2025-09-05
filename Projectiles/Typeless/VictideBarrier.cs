using System;
using System.Collections.Generic;
using CalamityMod.Cooldowns;
using CalamityMod.Items.Armor.Victide;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
using static CalamityMod.CalamityUtils;

namespace CalamityMod.Projectiles.Typeless
{
    public class VictideBarrier : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Typeless";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public static readonly SoundStyle ExplosionSound = new("CalamityMod/Sounds/Custom/BubblyBurst");

        public static float ExplosionLifetime = 24f;
        public static float MaxScale = 6f;

        public static float DashSpeed = 24f; // 24 tiles
        public static float DashLifetime = 16f;
        public static int DashSlamIFrames = 12;
        private List<int> NPCsHitBySlam = new List<int>() { -1 };

        public ref float ExplodeTimer => ref Projectile.ai[0];
        public ref float DashTimer => ref Projectile.ai[1];
        public ref float HealTimer => ref Projectile.ai[2];
        public Player Owner => Main.player[Projectile.owner];

        public static Asset<Texture2D> Shine;
        public static Asset<Texture2D> Explosion;
        public override void Load()
        {
            Shine = ModContent.Request<Texture2D>("CalamityMod/Particles/SemiCircularSmearSwipe");
            Explosion = ModContent.Request<Texture2D>("CalamityMod/Particles/SoftRoundExplosion");
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 20;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = (int)ExplosionLifetime;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            Projectile.rotation = MathHelper.ToRadians(Main.GlobalTimeWrappedHourly * 36f);

            if (ExplodeTimer > 0f)
            {
                float scaleLevel = PiecewiseAnimation(ExplodeTimer / ExplosionLifetime, new CurveSegment[] { new CurveSegment(EasingType.PolyOut, 0f, 0f, 1f, 4) });
                Projectile.scale = MathHelper.Lerp(1f, MaxScale, scaleLevel);
                Projectile.Opacity = MathF.Sin(MathHelper.PiOver2 + MathHelper.PiOver2 * ExplodeTimer / ExplosionLifetime);

                if (ExplodeTimer == 1f)
                {
                    SoundEngine.PlaySound(ExplosionSound, Projectile.Center);

                    for (int i = 0; i < 5; i++)
                    {
                        Vector2 velocity = (MathHelper.TwoPi * i / 5f).ToRotationVector2() * Main.rand.NextFloat(10f, 12f) * (i % 2 == 0 ? 0.88f : 1f);
                        Particle bub = new VelChangingSpark(Projectile.Center, velocity.RotatedByRandom(MathHelper.Pi / 10f), Vector2.UnitY * -6f, "CalamityMod/Particles/BloomRing", Main.rand.Next(36, 42 + 1), Main.rand.NextFloat(0.1f, 0.25f), Main.rand.NextBool(3) ? Color.HotPink : Color.Turquoise, Vector2.One, shrinkSpeed: 0.08f);
                        GeneralParticleHandler.SpawnParticle(bub);
                    }
                }

                if (ExplodeTimer == 6f)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        Vector2 smokeVelocity = Vector2.UnitX.RotatedByRandom(MathHelper.Pi) * Main.rand.NextFloat(2.5f, 5.5f);
                        float smokeScale = Main.rand.NextFloat(1.6f, 2f);
                        Particle smoke = new HeavySmokeParticle(Projectile.Center, smokeVelocity, Color.PaleTurquoise, 18, smokeScale, 0.25f, Main.rand.NextFloat(-0.1f, 0.1f), true);
                        GeneralParticleHandler.SpawnParticle(smoke);
                    }
                }

                ExplodeTimer++;
            }
            else if (DashTimer > 0f)
            {
                Owner.mount?.Dismount(Owner);
                Owner.RemoveAllGrapplingHooks();

                Owner.velocity = Projectile.velocity;
                Owner.ChangeDir(Math.Sign(Projectile.velocity.X) <= 0 ? -1 : 1);

                // Move the player to the projectile, allowing them to bypass platforms (but not tiles)
                Owner.Center = Projectile.Center;

                // Custom ram effect, similar to the one used in CalamityPlayerDashEffects
                Rectangle hitArea = new Rectangle((int)(Owner.position.X + Owner.velocity.X * 0.5 - 4f), (int)(Owner.position.Y + Owner.velocity.Y * 0.5 - 4), Owner.width + 8, Owner.height + 8);
                foreach (NPC n in Main.ActiveNPCs)
                {
                    // Ignore critters with the Guide to Critter Companionship
                    if (Owner.dontHurtCritters && NPCID.Sets.CountsAsCritter[n.type])
                        continue;

                    if (!n.dontTakeDamage && !n.friendly && !NPCsHitBySlam.Contains(n.whoAmI))
                    {
                        if (hitArea.Intersects(n.getRect()) && (n.noTileCollide || Owner.CanHit(n)))
                        {
                            Projectile.NewProjectile(Owner.GetSource_FromThis(), n.Center, Vector2.Zero, ModContent.ProjectileType<DirectStrike>(), Projectile.damage, 0f, Owner.whoAmI, n.whoAmI);
                            Owner.GiveImmuneTimeForCollisionAttack(DashSlamIFrames);
                            NPCsHitBySlam.Add(n.whoAmI);
                        }
                    }
                }

                // Dash trail effects
                Color trailColor = Color.Lerp(Color.Cyan, Color.RoyalBlue, DashTimer / DashLifetime);
                if (DashTimer % 2f == 1f)
                {
                    Particle trail = new CustomSpark(Projectile.Center - Projectile.velocity * 2f, Projectile.velocity * 0.5f, "CalamityMod/Particles/ForwardSmear", false, 12, 0.2f, trailColor, new Vector2(1f, Main.rand.NextFloat(1.2f, 1.35f)), fadeIn: true, extraRotation: MathHelper.ToRadians(180f));
                    GeneralParticleHandler.SpawnParticle(trail);
                }
                for (int direction = -1; direction <= 1; direction += 2)
                {
                    Vector2 sidePos = Projectile.Center - Projectile.velocity * 2f + Projectile.velocity.SafeNormalize(Vector2.Zero).RotatedBy(MathHelper.PiOver2 * direction) * Utils.Remap(DashTimer, 1f, DashLifetime, 48f, 24f);
                    Particle sideTrail = new CustomSpark(sidePos, Projectile.velocity * 0.5f, "CalamityMod/Particles/BloomCircle", false, 18, 0.3f, trailColor * 0.6f, new Vector2(1f, 2.5f), shrinkSpeed: 0.2f);
                    GeneralParticleHandler.SpawnParticle(sideTrail);
                }

                DashTimer++;
            }
            else
            {
                // If the barrier set isn't on, unceremoniously disappear
                if (!Owner.Calamity().victideBarrierSet || Owner.dead || !Owner.active)
                {
                    Projectile.Kill();
                    Owner.Calamity().victideBarrierHeal = 0;
                    return;
                }
                // Set to dash if cooldown is detected and also create an explosion in its place
                if (Owner.HasCooldown(WardingWave.ID))
                {
                    DashTimer++;
                    Projectile.damage = (int)Owner.GetBestClassDamage().ApplyTo(VictideHeadBarrier.BarrierDamage);
                    Owner.Calamity().victideBarrierHeal = 0;
                    if (Projectile.owner == Main.myPlayer)
                    {
                        // Set dash velocity
                        Projectile.velocity = Owner.SafeDirectionTo(Main.MouseWorld) * DashSpeed;
                        Projectile.tileCollide = true;

                        // Spawn explosion
                        Projectile.NewProjectile(Projectile.GetSource_FromThis(), Owner.MountedCenter, Vector2.Zero, Type, Projectile.damage, VictideHeadBarrier.BarrierExplosionKB, Projectile.owner, 1f);
                    }
                }

                if (Owner.Calamity().victideBarrierHeal > 0)
                {
                    HealTimer++;
                    if (HealTimer % VictideHeadBarrier.BarrierFramesPerHeal == VictideHeadBarrier.BarrierFramesPerHeal - 1)
                    {
                        Owner.Calamity().victideBarrierHeal--;
                        Owner.HealPlayer(1);
                    }
                }
                else
                    HealTimer = 0f;

                Lighting.AddLight(Owner.MountedCenter, Color.White.ToVector3() * 0.5f);
                Projectile.Center = Owner.MountedCenter;
                Projectile.timeLeft = (int)DashLifetime;
            }
        }

        public override void OnKill(int timeLeft)
        {
            if (DashTimer > 0f)
                Owner.velocity *= 0.5f;
        }

        // Explosion collision only (dash has a custom ram logic check effect)
        public override bool? CanDamage() => ExplodeTimer > 0f ? null : false;
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => CalamityUtils.CircularHitboxCollision(Projectile.Center, 20f * Projectile.scale, targetHitbox);

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            modifiers.HitDirectionOverride = (Owner.Center.X < target.Center.X).ToDirectionInt();
            modifiers.SourceDamage *= Utils.Remap(Projectile.numHits, 0, 10, 1f, 0.1f, true);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (DashTimer > 0f)
                return false;

            Texture2D explosionTex = Explosion.Value;
            Color barrierColor = Main.hslToRgb(0.6f + 0.1f * MathF.Sin(Main.GlobalTimeWrappedHourly * 2f), 1f, 0.5f);

            if (ExplodeTimer > 0f)
            {
                Main.spriteBatch.EnterShaderRegion(BlendState.Additive);
                Vector2 explosionPos = Projectile.Center - Main.screenPosition;
                Main.EntitySpriteDraw(explosionTex, explosionPos, null, barrierColor * Projectile.Opacity, Projectile.rotation, explosionTex.Size() * 0.5f, 0.02f * Projectile.scale, SpriteEffects.None);
                Main.spriteBatch.ExitShaderRegion();
                return false;
            }

            Vector2 drawPos = Owner.Center + Vector2.UnitY * Owner.gfxOffY - Main.screenPosition;
            Texture2D circleBase = TextureAssets.Projectile[Type].Value;
            Color contrastingColor = Main.hslToRgb(0.6f + 0.1f * MathF.Cos(Main.GlobalTimeWrappedHourly * 2f), 1f, 0.5f);

            // Silly solution to making a clean circle over the player
            Main.spriteBatch.EnterShaderRegion();
            GameShaders.Misc["CalamityMod:CircularAoETelegraph"].UseOpacity(0.5f);
            GameShaders.Misc["CalamityMod:CircularAoETelegraph"].UseColor(barrierColor);
            GameShaders.Misc["CalamityMod:CircularAoETelegraph"].UseSecondaryColor(Color.Transparent);
            GameShaders.Misc["CalamityMod:CircularAoETelegraph"].UseSaturation(0f);
            GameShaders.Misc["CalamityMod:CircularAoETelegraph"].Apply();

            Main.EntitySpriteDraw(circleBase, drawPos, null, Color.White, 0f, circleBase.Size() * 0.5f, 80f, SpriteEffects.None);
            Main.spriteBatch.ExitShaderRegion();

            // Give it additional texture
            Main.spriteBatch.EnterShaderRegion(BlendState.Additive);
            Main.EntitySpriteDraw(explosionTex, drawPos, null, contrastingColor * 0.3f, Projectile.rotation, explosionTex.Size() * 0.5f, 0.04f, SpriteEffects.None);
            Texture2D shineTex = Shine.Value;
            Main.EntitySpriteDraw(shineTex, drawPos, null, Color.HotPink * 0.5f, MathHelper.ToRadians(36f), shineTex.Size() * 0.5f, 0.45f, SpriteEffects.None);
            Main.EntitySpriteDraw(shineTex, drawPos, null, Color.Orange * 0.3f, MathHelper.ToRadians(216f), shineTex.Size() * 0.5f, 0.55f, SpriteEffects.None);
            Main.spriteBatch.ExitShaderRegion();
            return false;
        }
    }
}
