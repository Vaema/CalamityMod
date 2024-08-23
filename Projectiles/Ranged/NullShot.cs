using System;
using System.Collections.Generic;
using CalamityMod.CalPlayer;
using CalamityMod.Dusts;
using CalamityMod.NPCs;
using CalamityMod.NPCs.NormalNPCs;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Ranged
{
    public class NullShot : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Ranged";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public ref float time => ref Projectile.ai[0];
        public Color baseColor = new Color(Main.DiscoR, Main.DiscoG, Main.DiscoB, Main.DiscoR);
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 25;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 23;
            Projectile.height = 23;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 250;
            Projectile.DamageType = DamageClass.Ranged;
            Projectile.extraUpdates = 4;
            Projectile.tileCollide = false;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        public override void AI()
        {
            Projectile.scale = Projectile.ai[1] == 5 ? 2.2f : 1.5f;
            Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver2;

            float rate = (Main.GlobalTimeWrappedHourly * 5);
            List<Color> eColors = new List<Color>()
                {
                    Color.Turquoise,
                    Color.Orchid
                };
            int colorIndex = (int)(rate / 2 % eColors.Count);
            Color currentColor = eColors[colorIndex];
            Color nextColor = eColors[(colorIndex + 1) % eColors.Count];
            if (!Main.zenithWorld)
                baseColor = Color.Lerp(currentColor, nextColor, rate % 2f > 1f ? 1f : rate % 1f);

            if (Projectile.ai[1] == 5 && !Main.zenithWorld)
                baseColor = Color.White;

            if (time == 5)
            {
                for (int i = 0; i < 4; i++)
                {
                    Dust dust = Dust.NewDustPerfect(Projectile.Center, Projectile.ai[1] == 5 ? ModContent.DustType<VoidDust>() : ModContent.DustType<LightDust>(), (Projectile.velocity * 4).RotatedByRandom(0.6f) * Main.rand.NextFloat(0.2f, 1f));
                    dust.noGravity = true;
                    dust.scale = Main.rand.NextFloat(1.15f, 1.35f);
                    dust.color = baseColor;
                }
            }
            if (time > 20)
            {
                if (Projectile.ai[1] == 5)
                {
                    // Spawn in a helix-style pattern
                    float sine = (float)Math.Sin(Projectile.timeLeft * 0.575f / MathHelper.Pi);

                    Vector2 offset = Projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(MathHelper.PiOver2) * sine * 16f;
                    float scale = Main.rand.NextFloat(0.8f, 1.1f);
                    if (Main.rand.NextBool(2))
                    {
                        Dust dust2 = Dust.NewDustPerfect(Projectile.Center + offset, Projectile.ai[1] == 5 ? ModContent.DustType<VoidDust>() : ModContent.DustType<LightDust>(), -Projectile.velocity * Main.rand.NextFloat(0.3f, 0.8f));
                        dust2.noGravity = true;
                        dust2.scale = scale;
                        dust2.color = baseColor;
                    }
                    if (Main.rand.NextBool(2))
                    {
                        Dust dust3 = Dust.NewDustPerfect(Projectile.Center - offset, Projectile.ai[1] == 5 ? ModContent.DustType<VoidDust>() : ModContent.DustType<LightDust>(), -Projectile.velocity * Main.rand.NextFloat(0.3f, 0.8f));
                        dust3.noGravity = true;
                        dust3.scale = scale;
                        dust3.color = baseColor;
                    }
                }
                else if (Main.rand.NextBool(13))
                {
                    Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(10, 10), Projectile.ai[1] == 5 ? ModContent.DustType<VoidDust>() : ModContent.DustType<LightDust>(), -Projectile.velocity * Main.rand.NextFloat(0.3f, 0.8f));
                    dust.noGravity = true;
                    dust.scale = Main.rand.NextFloat(1.05f, 1.65f);
                    dust.color = baseColor;
                }
            }
            if (time > 13 && time < 34 && Projectile.ai[2] > 0)
            {
                Projectile.Center += Projectile.velocity.RotatedBy((Projectile.ai[2] == 1 ? MathHelper.PiOver2 : -MathHelper.PiOver2)) * 0.2f;
            }

            if (Projectile.ai[1] == 5)
            {
                NPC targetedNPC = Projectile.Center.ClosestNPCAt(700);
                if (targetedNPC != null && time > 30 && Projectile.numHits < 1 && Vector2.Distance(targetedNPC.Center, Projectile.Center) < 700)
                {
                    Vector2 position = targetedNPC.Center;
                    Vector2 moveToMouse = (position - Projectile.Center).SafeNormalize(Vector2.UnitX);
                    if (Projectile.velocity.Length() < 8)
                        Projectile.velocity += moveToMouse * 0.42f;
                    else
                        Projectile.velocity *= 0.9f;
                }
            }

            time++;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            if (Projectile.ai[1] == 5)
            {
                for (int i = 0; i < 8; i++)
                {
                    Dust dust = Dust.NewDustPerfect(Projectile.Center, Projectile.ai[1] == 5 ? ModContent.DustType<VoidDust>() : ModContent.DustType<LightDust>(), (Projectile.velocity * 3).RotatedByRandom(0.7f) * Main.rand.NextFloat(0.2f, 1f));
                    dust.noGravity = true;
                    dust.scale = Main.rand.NextFloat(1.15f, 1.45f);
                    dust.color = baseColor;
                }
                Particle orb = new CustomPulse(Projectile.Center, Vector2.Zero, Color.White, "CalamityMod/Particles/BloomRing", new Vector2(1, 1), Main.rand.NextFloat(-10, 10), 0.3f, 0.65f, 13);
                GeneralParticleHandler.SpawnParticle(orb);
                Particle orb2 = new CustomPulse(Projectile.Center, Vector2.Zero, Color.Black, "CalamityMod/Particles/SmallBloomRingLayered", new Vector2(1, 1), Main.rand.NextFloat(-10, 10), 0.4f, 0.75f, 13, false);
                GeneralParticleHandler.SpawnParticle(orb2);
                Particle orb3 = new CustomPulse(Projectile.Center, Vector2.Zero, Color.Black, "CalamityMod/Particles/SmallBloom", new Vector2(1, 1), Main.rand.NextFloat(-10, 10), 0.4f, 0.25f, 16, false);
                GeneralParticleHandler.SpawnParticle(orb3);
            }
            else
            {
                for (int i = 0; i < 3; i++)
                {
                    Particle orb2 = new LineParticle(Projectile.Center, (Projectile.velocity * 2).RotatedByRandom(0.1f) * Main.rand.NextFloat(0.1f, 1f), false, Main.rand.Next(20, 28 + 1), Main.rand.NextFloat(0.6f, 1.3f), baseColor);
                    GeneralParticleHandler.SpawnParticle(orb2);
                }
            }
            SoundStyle fire = new("CalamityMod/Sounds/Item/NullHit");
            SoundEngine.PlaySound(fire with { Volume = (Projectile.ai[1] == 5 ? 1 : 0.7f), Pitch = Main.rand.NextFloat(0, 0.1f) * (Projectile.ai[1] == 5 ? 3 : 1) }, Projectile.Center);
            if (Main.zenithWorld)
            {
                #region NPC Nullification
                int nullBuff = Main.rand.Next(8);
                switch (nullBuff)
                {
                    case 0:
                        if (target.type != ModContent.NPCType<SuperDummyNPC>())
                            target.damage += 10;
                        break;
                    case 1:
                        target.damage -= 10;
                        break;
                    case 2:
                        target.knockBackResist = 0f;
                        break;
                    case 3:
                        target.knockBackResist = 1f;
                        break;
                    case 4:
                        target.defense += 5;
                        break;
                    case 5:
                        target.defense -= 5;
                        break;
                    case 6:
                        target.scale *= 2f;
                        break;
                    case 7:
                        target.scale *= 0.5f;
                        break;
                    default:
                        break;
                }
                #endregion
            }
        }
        public override bool PreDraw(ref Color lightColor)
        {
            if (time < 18)
                return false;

            Asset<Texture2D> tex = ModContent.Request<Texture2D>("CalamityMod/Particles/DrainLineBloom");
            Asset<Texture2D> tex2 = ModContent.Request<Texture2D>("CalamityMod/Particles/DrainLine");
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], baseColor with { A = 0 } * 0.35f, 1, tex.Value);
            CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Projectile.type], Projectile.ai[1] == 5 ? Color.Black : Color.Lerp(baseColor, Color.White, 0.5f), 1, tex2.Value, true, true);
            return false;
        }
    }
}
