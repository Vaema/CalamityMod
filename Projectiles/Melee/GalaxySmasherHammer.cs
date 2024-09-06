using System;
using System.Collections.Generic;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Dusts;
using CalamityMod.Graphics.Metaballs;
using CalamityMod.Items.Weapons.Melee;
using CalamityMod.Particles;
using CalamityMod.Projectiles.Ranged;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Melee
{
    public class GalaxySmasherHammer : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Melee";
        public override string Texture => "CalamityMod/Items/Weapons/Melee/GalaxySmasher";
        public static readonly SoundStyle UseSound = new("CalamityMod/Sounds/Item/PwnagehammerSound") { Volume = 0.35f };
        public static readonly SoundStyle RedHamSound = new("CalamityMod/Sounds/Item/GalaxySmasherClone") { Volume = 0.6f };
        public static readonly SoundStyle UseSoundFunny = new("CalamityMod/Sounds/Item/CalamityBell") { Volume = 1.5f };
        public ref int EmpoweredHammer => ref Main.player[Projectile.owner].Calamity().GalaxyHammer;
        public int returnhammer = 0;
        public float rotatehammer = 10f;
        public int PulseCooldown = 0;
        public float EchoHammerPrep = 0f;
        public float WaitTimer = 0f;
        public int InPulse = 0;
        public int time = 0;
        public Color usedColor = Color.Aqua;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 20;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }

        public override void SetDefaults()
        {
            Projectile.width = 86;
            Projectile.height = 72;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.extraUpdates = 1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 120;
        }


        public override void AI()
        {
            //returnhammer determines if the hammer is slowing down after hitting an enemy, or homing in on the player.
            Player player = Main.player[Projectile.owner];
            float targetDist = Vector2.Distance(player.Center, Projectile.Center);
            Projectile.rotation += MathHelper.ToRadians(rotatehammer * 2) * Projectile.direction;

            List<Color> eColors = new List<Color>()
            {
                Color.Aqua,
                Color.Magenta,
            };
            float rate = (Main.GlobalTimeWrappedHourly * 12);
            int colorIndex = (int)(rate / 2 % eColors.Count);
            Color currentColor = eColors[colorIndex];
            Color nextColor = eColors[(colorIndex + 1) % eColors.Count];
            usedColor = Color.Lerp(currentColor, nextColor, rate % 2f > 1f ? 1f : rate % 1f);

            if (EmpoweredHammer >= 8)
                EmpoweredHammer = 0;

            if (returnhammer == 0)
            {
                int falloffTime = 10;
                if (time > falloffTime)
                    Projectile.velocity.X *= 0.967f;
                if (Projectile.velocity.Y < 25 && time > falloffTime)
                    Projectile.velocity.Y += 0.426f;
                if (Projectile.velocity.Y < 5)
                    Projectile.velocity.Y *= 0.98f;
            }

            if (returnhammer == 1) //Hammer slows after the inital hit
            {
                Projectile.velocity.Y *= 0.926f;
                Projectile.velocity.X *= 0.811f;
                if (Projectile.velocity.X > -1.05f && Projectile.velocity.X < 1.05f & Projectile.velocity.Y > -1.05f && Projectile.velocity.Y < 1.05f)
                    returnhammer = 2;
            }

            if (returnhammer == 2) // Projectile returns to player
            {
                Projectile.extraUpdates = 2;
                float returnSpeed = StellarContempt.Speed * 0.7f;
                float acceleration = 1.1f;
                Player owner = Main.player[Projectile.owner];
                Vector2 playerCenter = owner.Center;
                float xDist = playerCenter.X - Projectile.Center.X;
                float yDist = playerCenter.Y - Projectile.Center.Y;
                float dist = (float)Math.Sqrt(xDist * xDist + yDist * yDist);
                dist = returnSpeed / dist;
                xDist *= dist;
                yDist *= dist;

                if (Projectile.velocity.X < xDist)
                {
                    Projectile.velocity.X = Projectile.velocity.X + acceleration;
                    if (Projectile.velocity.X < 0f && xDist > 0f)
                        Projectile.velocity.X += acceleration;
                }
                else if (Projectile.velocity.X > xDist)
                {
                    Projectile.velocity.X = Projectile.velocity.X - acceleration;
                    if (Projectile.velocity.X > 0f && xDist < 0f)
                        Projectile.velocity.X -= acceleration;
                }
                if (Projectile.velocity.Y < yDist)
                {
                    Projectile.velocity.Y = Projectile.velocity.Y + acceleration;
                    if (Projectile.velocity.Y < 0f && yDist > 0f)
                        Projectile.velocity.Y += acceleration;
                }
                else if (Projectile.velocity.Y > yDist)
                {
                    Projectile.velocity.Y = Projectile.velocity.Y - acceleration;
                    if (Projectile.velocity.Y > 0f && yDist < 0f)
                        Projectile.velocity.Y -= acceleration;
                }
                // Delete the projectile if it touches its owner, increase counter to the big hammer, and spawn a dustsplosion on the player that scales with how close they are to getting a big hammer.
                if (Main.myPlayer == Projectile.owner)
                {
                    if (Projectile.Hitbox.Intersects(player.Hitbox))
                    {
                        Main.NewText(EmpoweredHammer);
                        if (EmpoweredHammer >= 6)
                        {
                            if (WaitTimer == 0)
                                Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitX) * 10;
                            ++WaitTimer;
                            if (WaitTimer == 20f)
                            {
                                Projectile.velocity *= 0.001f;
                                EmpoweredHammer = 0;
                                WaitTimer = 0;
                                returnhammer = 3;
                                rotatehammer = 0;
                            }
                        }
                        else
                        {
                            EmpoweredHammer++;
                            SoundEngine.PlaySound(SoundID.DD2_BetsysWrathShot with { Volume = 0.4f }, Projectile.Center);
                            for (int i = 0; i < 30; i++)
                            {
                                Dust fire = Dust.NewDustPerfect(Projectile.Center, 181);
                                fire.velocity = Projectile.velocity.SafeNormalize(Vector2.UnitY).RotatedByRandom(0.8f) * new Vector2(4f, 1.25f) * Main.rand.NextFloat(0.9f, 1f);
                                fire.velocity = fire.velocity.RotatedBy(Projectile.rotation - MathHelper.PiOver2);
                                fire.velocity += Projectile.velocity * (EmpoweredHammer * 0.04f);

                                fire.noGravity = true;
                                fire.scale = Main.rand.NextFloat(0.2f, 0.6f) * EmpoweredHammer;

                                fire = Dust.CloneDust(fire);
                                fire.velocity = Main.rand.NextVector2Circular(3f, 3f);
                                fire.velocity += Projectile.velocity * (EmpoweredHammer * 0.04f);
                            }
                            Projectile.Kill();
                        }
                    }
                }
            }
            if (returnhammer == 3) //Hammer prepares to spawn echo hammer
            {
                if (InPulse == 0)
                {
                    SoundEngine.PlaySound(RedHamSound, Projectile.Center);
                    InPulse = 1;
                }

                rotatehammer += 0.12f;
                Projectile.Center += new Vector2(0, -3).RotatedBy(Projectile.rotation * 0.1f);
                if (EchoHammerPrep >= 8f && InPulse < 2)
                {
                    if (time % 12 == 0)
                    {
                        Particle pulse2 = new CustomPulse(Projectile.Center, Vector2.Zero, usedColor * 0.4f, "CalamityMod/Particles/HighResHollowCircleHardEdge", new Vector2(1f * Utils.GetLerpValue(4, 12, EchoHammerPrep), 1f * Utils.GetLerpValue(30, 8, EchoHammerPrep, true)), 0f, 2f, 0f, (int)(40 * Utils.GetLerpValue(35, 8, EchoHammerPrep, true)));
                        GeneralParticleHandler.SpawnParticle(pulse2);
                    }
                }
                if (EchoHammerPrep >= 20)
                {
                    InPulse = 2;
                }
                if (EchoHammerPrep <= 24f)
                {
                    EchoHammerPrep += 0.2f;

                    float fade = Utils.GetLerpValue(30, 0, EchoHammerPrep, true);
                    float numberOfDusts = 3f;
                    float rotFactor = 360f / numberOfDusts;
                    for (int i = 0; i < numberOfDusts; i++)
                    {
                        float rot = MathHelper.ToRadians(i * rotFactor);
                        Vector2 velOffset = CalamityUtils.RandomVelocity(100f, 70f, 250f, 0.04f);
                        velOffset *= Main.rand.NextFloat(45, 65) * fade;
                        Particle energy = new SparkParticle(Projectile.Center + velOffset * 2.5f, -velOffset * Main.rand.NextFloat(0.08f, 0.12f) * 1.5f, false, 14, Main.rand.NextFloat(1.1f, 1.25f) - 0.5f * fade, usedColor);
                        GeneralParticleHandler.SpawnParticle(energy);
                        Dust dust = Dust.NewDustPerfect(Projectile.Center + velOffset * 2.5f, 278, -velOffset * Main.rand.NextFloat(0.08f, 0.12f) * 1.5f, 0, default, Main.rand.NextFloat(0.4f, 0.6f));
                        dust.noGravity = true;
                        dust.color = usedColor;
                        dust.velocity += Projectile.velocity;
                    }
                }
                else
                {
                    int hammer = Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.velocity, ModContent.ProjectileType<GalaxySmasherEcho>(), Projectile.damage * 10, Projectile.knockBack * 2.5f, Projectile.owner, 0f, Projectile.ai[1]);
                    Main.projectile[hammer].localAI[0] = Math.Sign(Projectile.velocity.X);
                    Main.projectile[hammer].netUpdate = true;
                    Projectile.Kill();
                }
            }

            if (targetDist < 1400)
            {
                if (Main.rand.NextBool(3))
                {
                    Vector2 offset = new Vector2(12, 0).RotatedByRandom(MathHelper.ToRadians(360f));
                    Vector2 velOffset = new Vector2(4, 0).RotatedBy(offset.ToRotation());
                    Dust dust = Dust.NewDustPerfect(Projectile.Center + offset, 278, -Projectile.velocity * 0.2f + velOffset, 100, default, 0.7f);
                    dust.noGravity = true;
                    dust.color = Main.rand.NextBool() ? Color.Magenta : Color.Aqua;
                }
                Vector2 linePos = Projectile.Center + (MathHelper.Pi + Projectile.rotation + MathHelper.PiOver2).ToRotationVector2() * 25f;
                Particle spark = new CustomSpark(linePos, (MathHelper.Pi + Projectile.rotation * Math.Sign(Projectile.velocity.X)).ToRotationVector2().RotatedByRandom(0.4f) * Main.rand.NextFloat(2f, 14f), "CalamityMod/Particles/BloomRing", false, Main.rand.Next(7, 16), Main.rand.NextFloat(0.25f, 0.5f), usedColor * 0.35f, new Vector2(1f, 1f), true, false, Main.rand.NextFloat(-10, 10));
                GeneralParticleHandler.SpawnParticle(spark);
                for (int i = 0; i < (InPulse > 0 ? 3 : 2); i++)
                {
                    Vector2 offset = new Vector2(0, -30).RotatedBy(-MathHelper.PiOver4 * 0.5f).RotatedBy(Projectile.rotation);
                    Vector2 velOffset = new Vector2(0, -5).RotatedBy(-MathHelper.PiOver4).RotatedBy(Projectile.rotation) * Main.rand.NextFloat(0.4f, 1f);
                    Vector2 spawnPosition = Projectile.Center + offset + Main.rand.NextVector2Circular(6, 6);
                    StreamGougeMetaball.SpawnParticle(spawnPosition, velOffset * (InPulse > 0 ? 3 : 0.2f), 40f * Main.rand.NextFloat(0.9f, 1.3f) * (InPulse > 0 ? 3 : 1));
                }
            }
            time++;
        }
        // On hit play GONG and spawn dust.
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Player player = Main.player[Projectile.owner];
            if (returnhammer == 0)
            {
                if (Main.zenithWorld)
                    SoundEngine.PlaySound(UseSoundFunny with { Pitch = EmpoweredHammer * 0.05f - 0.05f }, Projectile.Center);

                else
                    SoundEngine.PlaySound(UseSound with { Pitch = EmpoweredHammer * 0.05f - 0.05f }, Projectile.Center);

                int FunSizeHamID = ModContent.ProjectileType<GalaxySmasherMini>();
                int FunSizeHamDamage = (int)(0.1f * Projectile.damage);
                float FunSizeHamKB = 0.2f;
                int NumOfFunSizeHams = EmpoweredHammer + 1;
                for (int i = 0; i < NumOfFunSizeHams; ++i)
                {
                    float startDist = Main.rand.NextFloat(160f, 190f);
                    Vector2 startDir = Main.rand.NextVector2Unit();
                    Vector2 startPoint = Projectile.Center + startDir * startDist;

                    float FunSizeHamSpeed = Main.rand.NextFloat(10f, 14f);
                    Vector2 velocity = startDir * -FunSizeHamSpeed;

                    if (Projectile.owner == Main.myPlayer)
                    {
                        int proj = Projectile.NewProjectile(Projectile.GetSource_FromThis(), startPoint, velocity, FunSizeHamID, FunSizeHamDamage, FunSizeHamKB, Projectile.owner);
                        if (proj.WithinBounds(Main.maxProjectiles))
                        {
                            Main.projectile[proj].DamageType = DamageClass.MeleeNoSpeed;
                            Main.projectile[proj].tileCollide = false;
                            Main.projectile[proj].timeLeft = 30;
                            Main.projectile[proj].extraUpdates = 2;
                        }
                    }
                }

                returnhammer = 1;
            }
            float numberOfDusts = MathHelper.Clamp(30 - Projectile.numHits * 3, 6, 30);
            float rotFactor = 360f / numberOfDusts;
            for (int i = 0; i < numberOfDusts; i++)
            {
                float rot = MathHelper.ToRadians(i * rotFactor);
                Vector2 offset = new Vector2(4.8f, 0).RotatedBy(rot * Main.rand.NextFloat(1.1f, 4.1f));
                Vector2 velOffset = new Vector2(7f, 0).RotatedBy(rot * Main.rand.NextFloat(1.1f, 4.1f));

                if (i % 3 == 0)
                {
                    StreamGougeMetaball.SpawnParticle(Projectile.Center + offset, velOffset * Main.rand.NextFloat(1f, 1.2f), 60f * Main.rand.NextFloat(0.9f, 1.3f));
                }
                else
                {
                    Dust dust = Dust.NewDustPerfect(Projectile.Center + offset, ModContent.DustType<LightDust>());
                    dust.noGravity = true;
                    dust.velocity = velOffset * Main.rand.NextFloat(0.45f, 1);
                    dust.scale = Main.rand.NextFloat(0.8f, 1.7f);
                    dust.color = Main.rand.NextBool(3) ? Color.Aqua : Color.Magenta;
                }
            }
            Projectile.ai[1] = target.whoAmI;
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            float minMult = 0.7f;
            int hitsToMinMult = 10;
            float damageMult = Utils.Remap(Projectile.numHits, 0, hitsToMinMult, 1, minMult, true);
            modifiers.SourceDamage *= damageMult;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = ModContent.Request<Texture2D>("CalamityMod/Items/Weapons/Melee/GalaxySmasher").Value;
            Asset<Texture2D> p = ModContent.Request<Texture2D>("CalamityMod/Particles/FlameExplosion2");
            Asset<Texture2D> p2 = ModContent.Request<Texture2D>("CalamityMod/Particles/CircularSmearSmokey");
            Asset<Texture2D> p3 = ModContent.Request<Texture2D>("CalamityMod/Particles/SmallBloom");

            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], lightColor * 0.5f, 3, texture, true, true);

            Vector2 generalDrawPos = Projectile.Center - Main.screenPosition;
            float fade = Utils.GetLerpValue(2, 15, Projectile.velocity.Length(), true);
            Main.EntitySpriteDraw(p3.Value, generalDrawPos, null, Color.Black * fade, Projectile.rotation, p3.Size() * 0.5f, 0.6f, SpriteEffects.None);
            for (int i = 0; i < 3; i++)
            Main.EntitySpriteDraw(p.Value, generalDrawPos, null, Color.Indigo * 0.95f * fade, Projectile.rotation * Main.rand.NextFloat(1.2f, 1.3f) + i * 0.2f, p.Size() * 0.5f, 0.05f + i * 0.004f, SpriteEffects.None);
            //Projectile.DrawProjectileWithBackglow(Color.Transparent with { A = 0 }, Color.White, 5f, texture);

            Main.EntitySpriteDraw(texture, generalDrawPos, null, lightColor, Projectile.rotation, texture.Size() * 0.5f, Projectile.scale, Projectile.direction < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None);

            Main.EntitySpriteDraw(p2.Value, generalDrawPos, null, usedColor with { A = 0 } * 0.65f * fade, Projectile.rotation * Main.rand.NextFloat(1.4f, 1.45f), p2.Size() * 0.5f, 0.9f * Main.rand.NextFloat(0.9f, 1.1f), SpriteEffects.None);
            return false;
        }
    }
}
