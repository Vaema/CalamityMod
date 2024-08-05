using System;
using System.Collections.Generic;
using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.Dusts;
using CalamityMod.Particles;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using ReLogic.Utilities;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Boss
{
    public class OldDukeVortex : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Boss";
        public SoundStyle SpawnSound = new("CalamityMod/Sounds/Custom/OldDukeVortex");
        public SlotId SoundId;

        public override void SetStaticDefaults()
        {
            SpawnSound.MaxInstances = 50;

            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 6;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }

        public override void SetDefaults()
        {
            SoundId = SoundEngine.PlaySound(SpawnSound with { IsLooped = true, MaxInstances = 20 }, Projectile.Center, _ => new ProjectileAudioTracker(Projectile).IsActiveAndInGame());
            Projectile.Calamity().DealsDefenseDamage = true;
            Projectile.width = 408;
            Projectile.height = 408;
            Projectile.scale = 0.004f;
            Projectile.hostile = true;
            Projectile.alpha = 0;
            Projectile.penetrate = -1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 1800;
            CooldownSlot = ImmunityCooldownID.Bosses;
        }

        public override void AI()
        {
            if (Main.zenithWorld)
            {
                if (Projectile.scale < 2f)
                {
                    if (Projectile.alpha > 0)
                        Projectile.alpha -= 1;

                    Projectile.scale += 0.004f;
                    if (Projectile.scale > 2f)
                        Projectile.scale = 2f;
                }
                else
                {
                    if (Projectile.timeLeft <= 85)
                    {
                        if (Projectile.alpha < 255)
                            Projectile.alpha += 3;

                        Projectile.scale -= 0.012f;
                    }
                }
            }
            else
            {
                if (Projectile.scale < 1f)
                {
                    if (Projectile.alpha > 0)
                        Projectile.alpha -= 1;

                    Projectile.scale += 0.004f;
                    if (Projectile.scale > 1f)
                        Projectile.scale = 1f;

                    Projectile.width = Projectile.height = (int)(408f * Projectile.scale);
                }
                else
                {
                    if (Projectile.timeLeft <= 85)
                    {
                        if (Projectile.alpha < 255)
                            Projectile.alpha += 3;

                        Projectile.scale -= 0.012f;
                        Projectile.width = Projectile.height = (int)(408f * Projectile.scale);
                    }
                    else
                        Projectile.width = Projectile.height = 408;
                }
            }

            if (Projectile.timeLeft <= 85)
            {
                Projectile.localAI[2] += 1f / 85f;
            }
            Projectile.velocity = Vector2.Normalize(new Vector2(Projectile.ai[0], Projectile.ai[1]) - Projectile.Center) * 1.5f;

            Projectile.rotation -= 0.1f * (float)(1D - (Projectile.alpha / 255D));

            float lightAmt = 2f * Projectile.scale;
            Lighting.AddLight(Projectile.Center, lightAmt, lightAmt * 2f, lightAmt);

            float maxdist = 1200;

            if (SoundEngine.TryGetActiveSound(SoundId, out var Sound) && Sound.IsPlaying)
            {
                Sound.Position = Projectile.Center;
                Sound.Volume = Projectile.scale;
                Sound.Pitch = MathHelper.Lerp(0f, -1f, (MathHelper.Clamp((Projectile.Distance(Main.LocalPlayer.Center) - 800) / maxdist, 0f, 1f) + (-Projectile.scale + 1)));
            }

            if (Projectile.timeLeft > 85)
            {
                Vector2 vec2 = Projectile.Center + new Vector2(Main.rand.NextFloat(320, 540) * Projectile.scale, 0).RotatedByRandom(MathHelper.TwoPi);

                GeneralParticleHandler.SpawnParticle(new SparkParticle(vec2, (Projectile.Center - vec2) / 20, false, 10, Main.rand.NextFloat(0.5f, 1f), Color.LimeGreen, true));
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            Asset<Texture2D> Tex = ModContent.Request<Texture2D>(Texture);

            float sc = MathHelper.Lerp(1, 0, Projectile.localAI[2]);

            float alphaLerp = MathHelper.Lerp(1f, 0f, (float)Projectile.alpha / 255f);

            Main.EntitySpriteDraw(Tex.Value, Projectile.Center - Main.screenPosition, Tex.Frame(), new Color(0f, 0f, 0f, 0.4f).MultiplyRGBA(new Color(alphaLerp, alphaLerp, alphaLerp, alphaLerp)), -Projectile.rotation / 2 * (4 + 1), Tex.Frame().Center(), 1.61f * Projectile.scale * sc, SpriteEffects.None);

            for (int i = 2; i >= 0; i--)
            {
                float lerp = (float)i / 3f;

                Main.EntitySpriteDraw(Tex.Value, Projectile.Center - Main.screenPosition, Tex.Frame(), Color.Lerp(new Color(5, 155, 95, 100), new Color(255, 255, 255, 55), lerp).MultiplyRGBA(new Color(alphaLerp, alphaLerp, alphaLerp, alphaLerp)), -Projectile.rotation / 2 * (i + 1), Tex.Frame().Center(), MathHelper.Lerp(1f, 1.7f, lerp) * Projectile.scale * sc, SpriteEffects.None);
            }
            return false;
        }

        public override bool CanHitPlayer(Player target) => Projectile.timeLeft <= 1680 && Projectile.timeLeft > 85;

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox) => CalamityUtils.CircularHitboxCollision(Projectile.Center, 210f * Projectile.scale, targetHitbox);

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            if (info.Damage <= 0)
                return;

            if (Projectile.timeLeft <= 1680 && Projectile.timeLeft > 85)
                target.AddBuff(ModContent.BuffType<Irradiated>(), 600);
        }
    }
}
