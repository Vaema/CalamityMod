using System;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Dusts;
using CalamityMod.Graphics.Metaballs;
using CalamityMod.NPCs;
using CalamityMod.NPCs.SupremeCalamitas;
using CalamityMod.Particles;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Steamworks;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Boss
{
    public class CloneHellblast : ModProjectile, ILocalizedModType
    {

        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public new string LocalizationCategory => "Projectiles.Boss";
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 4;
            ProjectileID.Sets.TrailCacheLength[Type] = 2;
            ProjectileID.Sets.TrailingMode[Type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.Calamity().DealsDefenseDamage = true;
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.hostile = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.penetrate = -1;
            Projectile.timeLeft = 255;
            CooldownSlot = ImmunityCooldownID.Bosses;
        }

        public override void AI()
        {
            if (Projectile.extraUpdates == 0)
            {
                Projectile.extraUpdates = 1;
                Projectile.velocity *= 0.5f;
            }
            Projectile.frameCounter++;
            if (Projectile.frameCounter >= 10)
            {
                Projectile.frame++;
                Projectile.frameCounter = 0;
            }
            if (Projectile.frame > 3)
                Projectile.frame = 0;

            int target = Player.FindClosest(Projectile.Center, 1, 1);

            float targetDist;
            if (target != -1 && !Main.player[target].dead && Main.player[target].active && Main.player[target] != null)
                targetDist = Vector2.Distance(Main.player[target].Center, Projectile.Center);
            else
                targetDist = 1000;

            Lighting.AddLight(Projectile.Center, 0.9f * Projectile.Opacity, 0f, 0f);

            if (targetDist < 1400f && Projectile.ai[1] == 2f)
            {
                // Spawn in a helix-style pattern
                float sine = (float)Math.Sin(Projectile.timeLeft * 0.575f / MathHelper.Pi);

                Vector2 offset = Projectile.velocity.SafeNormalize(Vector2.UnitX).RotatedBy(MathHelper.PiOver2) * sine * 16f;

                SparkParticle orb = new(Projectile.Center + offset, -Projectile.velocity * 0.05f, false, 8, 0.8f, Main.rand.NextBool() ? Color.Red : Color.Lerp(Color.Red, Color.Magenta, 0.5f));
                GeneralParticleHandler.SpawnParticle(orb);

                SparkParticle orb2 = new(Projectile.Center - offset, -Projectile.velocity * 0.05f, false, 8, 0.8f, Main.rand.NextBool() ? Color.Red : Color.Lerp(Color.Red, Color.Magenta, 0.5f));
                GeneralParticleHandler.SpawnParticle(orb2);
            }

            if (Projectile.timeLeft < 51)
                Projectile.Opacity -= 0.02f;

            if (Projectile.ai[2] == 0f)
            {
                Projectile.ai[2] = 1f;
                SoundEngine.PlaySound(SoundID.Item20, Projectile.Center);
            }

            if (Projectile.velocity.Length() < 9f)
                Projectile.velocity *= 1.03f;

            if (Projectile.velocity.X < 0f)
            {
                Projectile.spriteDirection = -1;
                Projectile.rotation = (float)Math.Atan2(-Projectile.velocity.Y, -Projectile.velocity.X);
            }
            else
            {
                Projectile.spriteDirection = 1;
                Projectile.rotation = (float)Math.Atan2(Projectile.velocity.Y, Projectile.velocity.X);
            }
            Projectile.rotation = Projectile.velocity.ToRotation();
            Projectile.spriteDirection = 1;
            var dir = Projectile.rotation.ToRotationVector2();
            if (Projectile.spriteDirection == -1)
                dir = dir.RotatedBy(MathHelper.Pi);
            var spot = Vector2.Lerp(Projectile.Center + dir.RotatedBy(MathHelper.PiOver2) * 26, Projectile.Center + dir.RotatedBy(-MathHelper.PiOver2) * 26, Main.rand.NextFloat());
            //Dust.NewDustPerfect(spot, DustID.Clentaminator_Red, Vector2.Zero, newColor: Color.Crimson, Scale: 0.75f).noGravity = true;
            for (var i = 0; i < 1; i++)
            {
                var p = CalamitasMetaball.SpawnParticle(Projectile.Center + Projectile.velocity * 2, Vector2.Zero, 40);// Main.rand.NextVector2Circular(2, 2), 64f);//
                p.rotation = Projectile.rotation + MathHelper.PiOver2;
                p.TextureToUse = ModContent.Request<Texture2D>("CalamityMod/Particles/PointParticle").Value;
                p.SizeScaling = 0.65f;
                p = CalamitasMetaball.SpawnParticle(Projectile.Center, Main.rand.NextVector2Circular(3, 3), 24f);
                p.SizeScaling = 0.8f;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            return false;
            //CalamityUtils.DrawAfterimagesCentered(Projectile, ProjectileID.Sets.TrailingMode[Type], lightColor, 1);
            var dir = Projectile.rotation.ToRotationVector2();
            if (Projectile.spriteDirection == -1)
                dir = dir.RotatedBy(MathHelper.Pi);

            Vector2 GetPoint(float dist, float rot = 0, float scaleMult = 1, bool useDir = true)
            {
                if (!useDir)
                {

                    return Projectile.Center + Vector2.UnitX.RotatedBy(rot) * dist * scaleMult;
                }
                return Projectile.Center + dir.RotatedBy(rot) * dist * scaleMult;
            }
            var tex = ModContent.Request<Texture2D>("CalamityMod/Particles/Blood").Value;
            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, tex.Frame(), Color.Crimson, Projectile.rotation + MathHelper.PiOver2, tex.Size() * 0.5f, 0.4f * Projectile.scale, Microsoft.Xna.Framework.Graphics.SpriteEffects.None, 0);

            Main.spriteBatch.Draw(tex, Projectile.Center - Main.screenPosition, tex.Frame(), Color.Black, Projectile.rotation + MathHelper.PiOver2, tex.Size() * 0.5f, 0.35f * Projectile.scale, Microsoft.Xna.Framework.Graphics.SpriteEffects.None, 0);
            Main.spriteBatch.Draw(tex, Projectile.Center + dir*14 - Main.screenPosition, null, Color.Crimson, Projectile.rotation + MathHelper.PiOver2, tex.Size() * 0.5f, 0.15f*Projectile.scale, Microsoft.Xna.Framework.Graphics.SpriteEffects.None,0);
            return false;
            for (var i = 0; i < 0; i++)
            {
                float scaleMult = (1 - i / 3f);
                //scaleMult *= 0.5f;
                for (var inv = -1; inv <= 1; inv += 2)
                {
                    CalamityUtils.DrawLineBetter(Main.spriteBatch, GetPoint(30, 0, scaleMult), GetPoint(30, MathHelper.PiOver2*inv, scaleMult), Color.Red * scaleMult, 4);
                    CalamityUtils.DrawLineBetter(Main.spriteBatch, GetPoint(-30, 0, scaleMult), GetPoint(30, MathHelper.PiOver2 * inv, scaleMult), Color.Red * scaleMult, 4);
                    /*CalamityUtils.DrawLineBetter(Main.spriteBatch, GetPoint(30, 0, scaleMult), GetPoint(28, 0.6f*inv, scaleMult), Color.Red * scaleMult, 4);
                    CalamityUtils.DrawLineBetter(Main.spriteBatch, GetPoint(28, 0.6f*inv, scaleMult), GetPoint(26, MathHelper.PiOver2 * inv, scaleMult), Color.Red * scaleMult, 4);
                    CalamityUtils.DrawLineBetter(Main.spriteBatch, GetPoint(-32, -0.5f * inv, scaleMult), GetPoint(26, MathHelper.PiOver2 * inv, scaleMult), Color.Red * scaleMult, 4);
                    CalamityUtils.DrawLineBetter(Main.spriteBatch, GetPoint(-32, -0.5f * inv, scaleMult), GetPoint(-48, 0, scaleMult), Color.Red * scaleMult, 4);*/
                }
            }
            
            for (var i = 0; i < 10; i++)
            {
                CalamityUtils.DrawLineBetter(Main.spriteBatch, GetPoint(30, 0, 1, false) - new Vector2(15 * MathF.Sin(Main.GlobalTimeWrappedHourly+i) + 15) * Projectile.spriteDirection, GetPoint(30, MathHelper.PiOver2, 1, false) - new Vector2(15 * MathF.Sin(Main.GlobalTimeWrappedHourly+i) + 15) * Projectile.spriteDirection, Color.Red * 0.25f, 4);
                //CalamityUtils.DrawLineBetter(Main.spriteBatch, GetPoint(30, 0) + new Vector2(15 * MathF.Sin(Main.GlobalTimeWrappedHourly + i) + 15).RotatedBy(MathHelper.PiOver2) * Projectile.spriteDirection, GetPoint(30, -MathHelper.PiOver2) + new Vector2(15 * MathF.Sin(Main.GlobalTimeWrappedHourly + i) + 15).RotatedBy(MathHelper.PiOver2) * Projectile.spriteDirection, Color.Red * 0.25f, 4);
            }
            for (var i = 0; i < 10; i+=2)
            {
                //CalamityUtils.DrawLineBetter(Main.spriteBatch, GetPoint(30, 0) - new Vector2(15 * MathF.Sin(Main.GlobalTimeWrappedHourly + i) + 15) * Projectile.spriteDirection, GetPoint(30, MathHelper.PiOver2) - new Vector2(15 * MathF.Sin(Main.GlobalTimeWrappedHourly + i) + 15) * Projectile.spriteDirection, Color.Red * 0.25f, 4);
                CalamityUtils.DrawLineBetter(Main.spriteBatch, GetPoint(30, 0,1,false) + new Vector2(15 * MathF.Sin(Main.GlobalTimeWrappedHourly + i) + 15).RotatedBy(MathHelper.PiOver2) * Projectile.spriteDirection, GetPoint(30, -MathHelper.PiOver2, 1, false) + new Vector2(15 * MathF.Sin(Main.GlobalTimeWrappedHourly + i) + 15).RotatedBy(MathHelper.PiOver2) * Projectile.spriteDirection, Color.Red * 0.25f, 4);
            }
            return false;
        }

        public override bool CanHitPlayer(Player target) => Projectile.timeLeft >= 51;

        public override void OnHitPlayer(Player target, Player.HurtInfo info)
        {
            if (info.Damage <= 0 || Projectile.timeLeft < 51)
                return;

            if (Projectile.ai[0] == 0f || Main.zenithWorld)
                target.AddBuff(ModContent.BuffType<VulnerabilityHex>(), 180);
            else
                target.AddBuff(ModContent.BuffType<BrimstoneFlames>(), 120);
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item20, Projectile.Center);
            for (int dust = 0; dust <= 5; dust++)
            {
                Dust.NewDust(Projectile.position + Projectile.velocity, Projectile.width, Projectile.height, (int)CalamityDusts.Brimstone, 0f, 0f);
            }
        }
    }
}
