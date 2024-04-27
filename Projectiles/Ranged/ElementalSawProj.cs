using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Cooldowns;
using CalamityMod.Items.Weapons.Ranged;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Ranged
{
    public class ElementalSawProj : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Ranged";
        public static readonly SoundStyle TileCollideGFB = new("CalamityMod/Sounds/Custom/MetalPipeFalling");

        public ref float SawLevel => ref Projectile.ai[0];
        public ref float Time => ref Projectile.ai[1];
        public ref float PierceBeforeReturn => ref Projectile.ai[2];

        // Controls if the saw is returning to the player.
        public bool Returning = false;
        public int ReturnTimer = 0;
        public const int ReturnDelay = 90;

        // Whether the saw is empowered by right click.
        public bool Empowered = false;

        public Particle SmallSlashSmear;
        public Particle LargeSlashSmear;
        public static Asset<Texture2D> SawBloom;
        public static Asset<Texture2D> SawOutline;
        public static Asset<Texture2D> SmallSlash;
        public static Asset<Texture2D> LargeSlash;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 4;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 46;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.timeLeft = 600;
            Projectile.penetrate = -1; // Saws only pierce a certain number of times before returning, and don't deal direct damage while returning
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.Calamity().pointBlankShotDuration = CalamityGlobalProjectile.DefaultPointBlankDuration;
        }

        public override void AI()
        {
            // dies from cringe (Deadshot Brooch moment)
            if (Projectile.MaxUpdates > 1)
                Projectile.MaxUpdates = 1;

            // Timer and rotation
            Time++;
            Projectile.rotation += MathHelper.ToRadians(6f + 18f * SawLevel);

            // Control the saw being empowered or not
            Player Owner = Main.player[Projectile.owner];
            Empowered = Owner.HasCooldown(ElementalSawBoost.ID);

            // While empowered, the saws while slightly home in on the cursor
            if (Empowered && !Returning && Time > 30)
            {
                float homingTurnSpeed = 0.11f;
                Projectile.velocity = Projectile.velocity.ToRotation().AngleTowards(Projectile.SafeDirectionTo(Main.MouseWorld).ToRotation(), homingTurnSpeed).ToRotationVector2() * ElementalSaw.ShootSpeed;
            }

            // Saws automatically return 2 seconds after hitting an enemy
            if (ReturnTimer > 0 && ReturnTimer < ReturnDelay)
            {
                ReturnTimer++;
                if (ReturnTimer == ReturnDelay)
                    Returning = true;
            }

            if (Returning)
            {
                Projectile.tileCollide = false;
                if (ReturnTimer < ReturnDelay)
                    ReturnTimer = ReturnDelay;

                ReturnTimer++;
                if (ReturnTimer < ReturnDelay + 30)
                    Projectile.velocity *= 0.95f;
                else
                {
                    // Spawns a burst of homing bolts when it starts returning, based on how many tiles and enemies it hit
                    if (ReturnTimer == ReturnDelay + 30)
                    {
                        int boltPairs = Projectile.numHits;
                        for (int b = 0; b < boltPairs; b++)
                        {
                            for (int p = 0; p < 2; p++)
                            {
                                Vector2 randBoltVelocity = Main.rand.NextVector2CircularEdge(1f, 1f);
                                randBoltVelocity.SafeNormalize(Vector2.Zero);
                                randBoltVelocity *= 8f;

                                if (Main.myPlayer == Projectile.owner)
                                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, randBoltVelocity, ModContent.ProjectileType<ElementalSawBullet>(), (int)(Projectile.damage * 0.5f), 0f, Main.myPlayer);
                            }
                        }
                    }

                    // Continously spawn homing bolts as it returns while empowered
                    if (ReturnTimer % 7 == 0 && Empowered)
                    {
                        Vector2 randVelocity = -Projectile.velocity.RotatedByRandom(MathHelper.Pi / 3) / 2;
                        if (Main.myPlayer == Projectile.owner)
                            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, randVelocity, ModContent.ProjectileType<ElementalSawBullet>(), (int)(Projectile.damage * 0.5f), 0f, Main.myPlayer);
                    }

                    float returnSpeed = (ElementalSaw.ShootSpeed * 0.6f) + (0.05f * (ReturnTimer - 120));
                    Vector2 ownerDist = Owner.Center - Projectile.Center;
                    if (ownerDist.Length() > 3000f)
                        Projectile.Kill();

                    ownerDist.Normalize();
                    ownerDist *= returnSpeed;

                    // Home back in on the player; accelerates over time
                    if (Projectile.velocity.X < ownerDist.X)
                        Projectile.velocity.X = ownerDist.X;
                    else if (Projectile.velocity.X > ownerDist.X)
                        Projectile.velocity.X = ownerDist.X;

                    if (Projectile.velocity.Y < ownerDist.Y)
                        Projectile.velocity.Y = ownerDist.Y;
                    else if (Projectile.velocity.Y > ownerDist.Y)
                        Projectile.velocity.Y = ownerDist.Y;

                    // Delete the saw if it touches its owner
                    if (Main.myPlayer == Projectile.owner)
                    {
                        if (Projectile.Hitbox.Intersects(Owner.Hitbox))
                            Projectile.Kill();
                    }
                }
            }

            // Rainbow smear particles which follow the path of the slashes
            if (Empowered)
            {
                if (SawLevel >= 2f)
                {
                    if (LargeSlashSmear == null)
                    {
                        LargeSlashSmear = new CircularSmearVFX(Projectile.Center, Color.Black, Time * -Projectile.rotation, 1.35f);
                        GeneralParticleHandler.SpawnParticle(LargeSlashSmear);
                    }
                    else
                    {
                        LargeSlashSmear.Rotation = -Projectile.rotation;
                        LargeSlashSmear.Time = 0;
                        LargeSlashSmear.Position = Projectile.Center;
                        LargeSlashSmear.Scale = 1.35f;
                        LargeSlashSmear.Color = Main.hslToRgb(0.5f + 0.5f * MathF.Sin(Main.GlobalTimeWrappedHourly * 5f), 1f, 0.6f) * 0.8f;
                    }
                }
                if (SawLevel >= 1f)
                {
                    if (SmallSlashSmear == null)
                    {
                        SmallSlashSmear = new CircularSmearVFX(Projectile.Center, Color.Black, Projectile.rotation, 0.8f);
                        GeneralParticleHandler.SpawnParticle(SmallSlashSmear);
                    }
                    else
                    {
                        SmallSlashSmear.Rotation = Projectile.rotation;
                        SmallSlashSmear.Time = 0;
                        SmallSlashSmear.Position = Projectile.Center;
                        SmallSlashSmear.Scale = 0.8f;
                        SmallSlashSmear.Color = Main.hslToRgb(0.5f + 0.5f * MathF.Cos(Main.GlobalTimeWrappedHourly * 5f), 1f, 0.6f) * 0.6f;
                    }
                }
            }
        }

        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            int sparkCount = 6 + 5 * (int)SawLevel;
            for (int s = 0; s < sparkCount; s++)
            {
                Vector2 sparkVelocity = new Vector2();
                if (Projectile.velocity.X != oldVelocity.X && oldVelocity.X < 0)
                    sparkVelocity = Vector2.UnitX * 6.5f;
                else if (Projectile.velocity.X != oldVelocity.X && oldVelocity.X >= 0)
                    sparkVelocity = Vector2.UnitX * -6.5f;
                else if (Projectile.velocity.Y != oldVelocity.Y && oldVelocity.Y < 0)
                    sparkVelocity = Vector2.UnitY * 6.5f;
                else if (Projectile.velocity.Y != oldVelocity.Y && oldVelocity.Y >= 0)
                    sparkVelocity = Vector2.UnitY * -6.5f;

                Vector2 sparkLocation = sparkVelocity.X > 0f ? Projectile.Left : (sparkVelocity.X < 0f ? Projectile.Right : (sparkVelocity.Y > 0f ? Projectile.Top : Projectile.Bottom));
                sparkVelocity = sparkVelocity.RotatedByRandom(MathHelper.PiOver2) * (Main.rand.NextFloat(0.8f, 1.2f) + (Main.rand.NextFloat(0.2f, 0.6f) * SawLevel));
                float scale = Main.rand.NextFloat(0.5f, 0.8f) + Main.rand.NextFloat(0.2f, 0.6f) * SawLevel;
                Particle collisionSparks = new AltLineParticle(sparkLocation, sparkVelocity, false, 30, scale, new Color(Main.DiscoR, Main.DiscoG, Main.DiscoB));
                GeneralParticleHandler.SpawnParticle(collisionSparks);
            }

            SoundEngine.PlaySound(Main.zenithWorld ? TileCollideGFB : SoundID.Item178 with { Pitch = 0.15f * Projectile.numHits }, Projectile.Center); // Placeholder sound
            if (Projectile.velocity.X != oldVelocity.X)
                Projectile.velocity.X = -oldVelocity.X;
            if (Projectile.velocity.Y != oldVelocity.Y)
                Projectile.velocity.Y = -oldVelocity.Y;

            if (PierceBeforeReturn > 0)
            {
                PierceBeforeReturn--;
                Projectile.numHits++;
                if (PierceBeforeReturn <= 0)
                    Returning = true;
            }

            return false;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            target.AddBuff(ModContent.BuffType<Laceration>(), 180);
            target.AddBuff(ModContent.BuffType<ElementalMix>(), 90);
            SoundEngine.PlaySound(new SoundStyle("CalamityMod/Sounds/Custom/SwiftSlice") with { Pitch = 0.15f * Projectile.numHits }, Projectile.Center);

            if (Projectile.numHits < 1)
                ReturnTimer = 1;

            if (PierceBeforeReturn > 0)
            {
                PierceBeforeReturn--;
                if (PierceBeforeReturn <= 0)
                    Returning = true;
            }

            // SUPER COLORFUL PARTICLE EFFECTS YEAH
            int onHitSparkAmount = 7 + 10 * (int)SawLevel;
            for (int s = 0; s < onHitSparkAmount; s++)
            {
                Vector2 sparkVel = Projectile.velocity.RotatedByRandom(MathHelper.ToRadians(30f)) * (Main.rand.NextFloat(0.4f, 0.8f) + (Main.rand.NextFloat(0.4f, 0.6f) * SawLevel));
                float sparkSize = 0.4f + Main.rand.NextFloat(0.3f, 0.6f) * SawLevel;
                Color sparkColor = Main.hslToRgb(Main.rand.NextFloat(), 1f, 0.8f);

                Particle sparked = new AltLineParticle(target.Center, sparkVel, false, 30, sparkSize, sparkColor);
                GeneralParticleHandler.SpawnParticle(sparked);
            }
            for (int sq = 0; sq < 7; sq++)
            {
                Vector2 squareVel = Main.rand.NextVector2CircularEdge(1f, 1f)* (Main.rand.NextFloat(10f, 16f) + 5f * SawLevel);
                float squareSize = 1.6f + Main.rand.NextFloat(1f, 1.6f) * SawLevel;
                Color squareColor = Main.hslToRgb(Main.rand.NextFloat(), 0.6f, 0.8f);

                Particle squared = new SquareParticle(target.Center, squareVel, true, 30, squareSize, squareColor);
                GeneralParticleHandler.SpawnParticle(squared);
            }
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            // The saw deals less damage while returning, to prevent its damage being too crazy
            if (PierceBeforeReturn <= 0)
                modifiers.SourceDamage *= 0.33f;
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(new SoundStyle("CalamityMod/Sounds/Custom/CeramicImpact", 2), Projectile.Center);
        }

        public override void ModifyDamageHitbox(ref Rectangle hitbox)
        {
            if (SawLevel >= 2f)
                hitbox.Inflate(72, 72);
            else if (SawLevel >= 1f)
                hitbox.Inflate(32, 32);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            LargeSlash ??= ModContent.Request<Texture2D>("CalamityMod/Projectiles/Ranged/ElementalSawLargeSlash");
            Texture2D largeSlashTexture = LargeSlash.Value;
            SmallSlash ??= ModContent.Request<Texture2D>("CalamityMod/Projectiles/Ranged/ElementalSawSmallSlash");
            Texture2D smallSlashTexture = SmallSlash.Value;
            Color slashColor = new Color(200, 200, 200, 100);

            if (SawLevel >= 2f)
            {
                Main.EntitySpriteDraw(largeSlashTexture, Projectile.Center - Main.screenPosition, null, slashColor, -Projectile.rotation, largeSlashTexture.Size() * 0.5f, 1f, SpriteEffects.None);

                if (Time % 4 == 0)
                {
                    Vector2 randomParticleOffset = new Vector2(Main.rand.NextFloat(-Projectile.width * 1.75f, Projectile.width * 1.75f), Main.rand.NextFloat(-Projectile.width * 1.75f, Projectile.width * 1.75f));
                    float randomParticleScale = Main.rand.NextFloat(0.65f, 0.95f);
                    Color bloomColor = Color.Lerp(new Color(29, 120, 30), new Color(56, 255, 59), MathF.Abs(MathF.Sin(Time)));
                    Particle bloomCircle = new BloomParticle(Projectile.Center + randomParticleOffset, Projectile.velocity, Main.rand.NextBool() ? Color.White : bloomColor, randomParticleScale, randomParticleScale, 4, false);
                    GeneralParticleHandler.SpawnParticle(bloomCircle);
                }
            }
            if (SawLevel >= 1f)
            {
                Main.EntitySpriteDraw(smallSlashTexture, Projectile.Center - Main.screenPosition, null, slashColor, Projectile.rotation, smallSlashTexture.Size() * 0.5f, 1f, SpriteEffects.None);

                if (Time % 4 == 0)
                {
                    Vector2 randomParticleOffset = new Vector2(Main.rand.NextFloat(-Projectile.width, Projectile.width), Main.rand.NextFloat(-Projectile.width, Projectile.width));
                    float randomParticleScale = Main.rand.NextFloat(0.35f, 0.65f);
                    Color bloomColor = Color.Lerp(new Color(29, 120, 30), new Color(56, 255, 59), MathF.Abs(MathF.Cos(Time)));
                    Particle bloomCircle = new BloomParticle(Projectile.Center + randomParticleOffset, Projectile.velocity, Main.rand.NextBool() ? Color.White : bloomColor, randomParticleScale, randomParticleScale, 4, false);
                    GeneralParticleHandler.SpawnParticle(bloomCircle);
                }
            }

            // Draw the saw itself at full brightness
            Texture2D buzzsawTexture = TextureAssets.Projectile[Projectile.type].Value;
            Main.EntitySpriteDraw(buzzsawTexture, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, buzzsawTexture.Size() * 0.5f, 1f, SpriteEffects.None);

            if (Empowered) // Rainbow outline while empowered
            {
                SawOutline ??= ModContent.Request<Texture2D>("CalamityMod/Projectiles/Ranged/ElementalSawProjOutline");
                Texture2D outline = SawOutline.Value;
                Main.EntitySpriteDraw(outline, Projectile.Center - Main.screenPosition, null, Main.DiscoColor, Projectile.rotation, outline.Size() * 0.5f, 1f, SpriteEffects.None);
            }

            // Rainbow glow if empowered, otherwise lime green
            Main.spriteBatch.SetBlendState(BlendState.Additive);
            SawBloom ??= ModContent.Request<Texture2D>("CalamityMod/Particles/BloomCircle");
            Texture2D bloom = SawBloom.Value;
            Main.EntitySpriteDraw(bloom, Projectile.Center - Main.screenPosition, null, Empowered ? Main.DiscoColor * 0.5f : new Color(150, 255, 60) * 0.2f, 0f, bloom.Size() * 0.5f, 1f, SpriteEffects.None);
            Main.spriteBatch.SetBlendState(BlendState.AlphaBlend);

            if (!CalamityConfig.Instance.Afterimages)
                return false;

            // Special afterimage drawing to include the slashes
            for (int i = 1; i < Projectile.oldPos.Length; i++)
            {
                float afterimageRot = Projectile.oldRot[i];

                Vector2 drawPos = Projectile.oldPos[i] + buzzsawTexture.Size() * 0.5f - Main.screenPosition;
                float intensity = MathHelper.Lerp(0.1f, 0.6f, 1f - i / (float)Projectile.oldPos.Length);
                
                Main.EntitySpriteDraw(buzzsawTexture, drawPos, null, lightColor * intensity, afterimageRot, buzzsawTexture.Size() * 0.5f, 1f, SpriteEffects.None);

                if (SawLevel >= 2f)
                    Main.EntitySpriteDraw(largeSlashTexture, drawPos, null, slashColor * intensity, -afterimageRot, largeSlashTexture.Size() * 0.5f, 1f, SpriteEffects.None);
                if (SawLevel >= 1f)
                    Main.EntitySpriteDraw(smallSlashTexture, drawPos, null, slashColor * intensity, afterimageRot, smallSlashTexture.Size() * 0.5f, 1f, SpriteEffects.None);
            }
            return false;
        }
    }
}
