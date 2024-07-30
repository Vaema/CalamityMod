using System;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Events;
using CalamityMod.NPCs;
using CalamityMod.NPCs.Providence;
using CalamityMod.Particles;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Boss
{
    public class HolyBurnOrb : ModProjectile, ILocalizedModType
    {
        bool started = false;
        public new string LocalizationCategory => "Projectiles.Boss";
        public override string Texture => "CalamityMod/Projectiles/StarProj";

        public override void SetDefaults()
        {
            Projectile.localAI[1] = Main.rand.NextFloat(30f);
            Projectile.width = Projectile.height = 30;
            Projectile.hostile = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.alpha = 255;
            CooldownSlot = ImmunityCooldownID.Bosses;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 200;
            Projectile.Calamity().DealsDefenseDamage = true;
        }

        public override void OnSpawn(IEntitySource source)
        {
            
        }

        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, 0.45f, 0.35f, 0f);

            // Day mode by default but syncs with the boss
            if (CalamityGlobalNPC.holyBoss != -1)
            {
                if (Main.npc[CalamityGlobalNPC.holyBoss].active)
                    Projectile.maxPenetrate = (int)Main.npc[CalamityGlobalNPC.holyBoss].localAI[1];
            }
            else if (CalamityGlobalNPC.doughnutBoss != -1)
            {
                if (Main.npc[CalamityGlobalNPC.doughnutBoss].active)
                {
                    if (Main.npc[CalamityGlobalNPC.doughnutBoss].Calamity().CurrentlyEnraged)
                        Projectile.maxPenetrate = (int)Providence.BossMode.Night;
                    else
                        Projectile.maxPenetrate = (int)Providence.BossMode.Day;
                }
            }
            else
                Projectile.maxPenetrate = (int)Providence.BossMode.Day;

            if (Projectile.ai[0] == 0f && BossRushEvent.BossRushActive)
                Projectile.velocity *= 1.25f;

            if (!started)
            {
                Color cl = ProvUtils.GetProjectileColor(Projectile.maxPenetrate, 255);
                GeneralParticleHandler.SpawnParticle(new CustomPulse(Projectile.Center, Vector2.Zero, cl, "CalamityMod/Particles/BlastCone", new Vector2(Main.rand.NextFloat(4f, 7f), 1.5f), Vector2.Zero.AngleTo(Projectile.velocity), 1f, 0f, 30));
                started = true;
            }

            if (Projectile.ai[0] < 240f)
            {
                Projectile.ai[0] += 1f;

                if (Projectile.timeLeft < 160)
                    Projectile.timeLeft = 160;
            }

            if (CalamityWorld.LegendaryMode && CalamityWorld.revenge)
            {
                if (Projectile.velocity.Length() < 12f && Projectile.ai[1] == 0f)
                {
                    Projectile.velocity *= 1.02f;
                }
                else
                {
                    Projectile.ai[1] += 0.05f;
                    Projectile.velocity *= MathHelper.Lerp(0.95f, 1.05f, (float)Math.Abs(Math.Sin(Projectile.ai[1])));
                }
            }
            else if (Projectile.velocity.Length() < 16f)
                Projectile.velocity *= 1.01f;

            Projectile.localAI[1] += (Projectile.velocity.Length() / 20);

            Color col = ProvUtils.GetProjectileColor(Projectile.maxPenetrate, 255);

            float vel = MathHelper.Clamp(Projectile.velocity.Length() / 5, 0, 1.5f);

            GlowOrbParticle p = new GlowOrbParticle(Projectile.Center, Projectile.velocity + new Vector2(Main.rand.NextFloat(vel * 2), 0).RotatedByRandom(MathHelper.TwoPi), false, 4, 1f, col);

            GeneralParticleHandler.SpawnParticle(p);
        }

        public override bool PreDraw(ref Color lightColor)
        {
            float lerpMult = MathHelper.Lerp(0.5f, 1.5f, Math.Abs((float)Math.Sin(Projectile.localAI[1] / 10f)));
            
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Vector2 drawPos = Projectile.Center - Main.screenPosition + new Vector2(0f, Projectile.gfxOffY);
            Color baseColor = ProvUtils.GetProjectileColor(Projectile.maxPenetrate, 255, true) * 4;
            Color baseColor2 = ProvUtils.GetProjectileColor(Projectile.maxPenetrate, 255);
            baseColor.A = 0;
            baseColor *= lerpMult;
            baseColor2 *= lerpMult;
            Vector2 origin = texture.Size() / 2f;
            Vector2 scale = new Vector2(0.5f, 1.5f) * lerpMult;

            SpriteEffects spriteEffects = SpriteEffects.None;
            if (Projectile.spriteDirection == -1)
                spriteEffects = SpriteEffects.FlipHorizontally;

            Projectile.rotation += MathHelper.ToRadians(lerpMult * 2f);

            float upRight = MathHelper.PiOver4;
            float up = MathHelper.PiOver2;
            float upLeft = 3f * MathHelper.PiOver4;
            float left = MathHelper.Pi;
            Main.EntitySpriteDraw(texture, drawPos, null, baseColor, upLeft + Projectile.rotation, origin, scale, spriteEffects, 0);
            Main.EntitySpriteDraw(texture, drawPos, null, baseColor, upRight - Projectile.rotation, origin, scale, spriteEffects, 0);
            Main.EntitySpriteDraw(texture, drawPos, null, baseColor2, upLeft + Projectile.rotation, origin, scale * 0.6f, spriteEffects, 0);
            Main.EntitySpriteDraw(texture, drawPos, null, baseColor2, upRight - Projectile.rotation, origin, scale * 0.6f, spriteEffects, 0);
            Main.EntitySpriteDraw(texture, drawPos, null, baseColor, up + Projectile.rotation, origin, scale * 0.6f, spriteEffects, 0);
            Main.EntitySpriteDraw(texture, drawPos, null, baseColor, left - Projectile.rotation, origin, scale * 0.6f, spriteEffects, 0);
            Main.EntitySpriteDraw(texture, drawPos, null, baseColor2, up + Projectile.rotation, origin, scale * 0.36f, spriteEffects, 0);
            Main.EntitySpriteDraw(texture, drawPos, null, baseColor2, left - Projectile.rotation, origin, scale * 0.36f, spriteEffects, 0);

            return false;
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item14, Projectile.Center);
            Projectile.ExpandHitboxBy(50);
            int dustType = ProvUtils.GetDustID(Projectile.maxPenetrate);
            for (int d = 0; d < 5; d++)
            {
                int holy = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, dustType, 0f, 0f, 100, default, 2f);
                Main.dust[holy].velocity *= 3f;
                Main.dust[holy].noGravity = true;
                if (Main.rand.NextBool())
                {
                    Main.dust[holy].scale = 0.5f;
                    Main.dust[holy].fadeIn = 1f + Main.rand.Next(10) * 0.1f;
                }
            }
            for (int d = 0; d < 8; d++)
            {
                int fire = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, dustType, 0f, 0f, 100, default, 3f);
                Main.dust[fire].noGravity = true;
                Main.dust[fire].velocity *= 5f;
                fire = Dust.NewDust(Projectile.position, Projectile.width, Projectile.height, dustType, 0f, 0f, 100, default, 2f);
                Main.dust[fire].velocity *= 2f;
                Main.dust[fire].noGravity = true;
            }
        }

        public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
        {
            //In GFB, "real damage" is replaced with negative healing
            if (Projectile.maxPenetrate >= (int)Providence.BossMode.Red)
                modifiers.SourceDamage *= 0f;

            Projectile.Kill();
        }

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            // If the player is dodging, don't apply debuffs
            if ((info.Damage <= 0 && Projectile.maxPenetrate < (int)Providence.BossMode.Red) || target.creativeGodMode)
                return;

            ProvUtils.ApplyHitEffects(target, Projectile.maxPenetrate, 120, 50);
        }
    }
}
