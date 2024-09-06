using System;
using System.IO;
using CalamityMod.Graphics.Metaballs;
using CalamityMod.Particles;
using CalamityMod.Projectiles.DraedonsArsenal;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using static Humanizer.In;

namespace CalamityMod.Projectiles.Melee
{
    public class GalaxySmasherEcho : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Melee";
        public static readonly SoundStyle SlamHamSound = new("CalamityMod/Sounds/Item/GalaxySmasherSmash") { Volume = 0.6f };
        public static readonly SoundStyle Kunk = new("CalamityMod/Sounds/Item/TF2PanHit") { Volume = 1.1f };
        public float rotatehammer = 35f;
        public float speed = 0f;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.CultistIsResistantTo[Type] = true;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 15;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }
        //This is all copied straight from PwnagehammerEcho with some minor edits.
        public override void SetDefaults()
        {
            Projectile.width = 78;
            Projectile.height = 78;
            Projectile.aiStyle = 0;
            Projectile.friendly = true;
            Projectile.DamageType = DamageClass.MeleeNoSpeed;
            Projectile.penetrate = 1;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.usesLocalNPCImmunity = true;
        }
        public override void AI()
        {
            Projectile.scale = 2.78f;
            rotatehammer *= 1.2f;
            Projectile.rotation += MathHelper.ToRadians(rotatehammer) * Projectile.direction;
            Projectile.ai[0] += 1f;
            if (Projectile.ai[0] < 42f)
            {
                Projectile.velocity *= 0.95f;
                Projectile.rotation += MathHelper.ToRadians(Projectile.ai[0] * 0.5f) * Projectile.localAI[0];
            }
            else if (Projectile.ai[0] > 42f)
            {
                Projectile.extraUpdates = 1;
                if (Projectile.ai[1] < 0f)
                {
                    CalamityUtils.HomeInOnNPC(Projectile, Projectile.tileCollide, 2000f, speed, 12f);
                    if (Projectile.ai[0] > 80f)
                        Projectile.Kill();
                }

                NPC target = Main.npc[(int)Projectile.ai[1]];
                if (!target.CanBeChasedBy(Projectile, false) || !target.active)
                    Projectile.Kill();
                else
                {
                    float velConst = 5f;
                    velConst--;
                    Projectile.velocity = new Vector2((target.Center.X - Projectile.Center.X) / velConst, (target.Center.Y - Projectile.Center.Y) / velConst);
                    Projectile.rotation += MathHelper.ToRadians(48f) * Projectile.localAI[0];
                }
            }

            if (Main.rand.NextBool())
            {
                Vector2 offset = new Vector2(12, 0).RotatedByRandom(MathHelper.ToRadians(360f));
                Vector2 velOffset = new Vector2(4, 0).RotatedBy(offset.ToRotation());
                Dust dust = Dust.NewDustPerfect(new Vector2(Projectile.Center.X, Projectile.Center.Y) + offset, 272, new Vector2(Projectile.velocity.X * 0.4f + velOffset.X, Projectile.velocity.Y * 0.4f + velOffset.Y), 100, default, 0.9f);
                dust.noGravity = true;
            }
        }

        public override bool? CanHitNPC(NPC target)
        {
            if (Projectile.ai[0] <= 42f)
                return false;
            return null;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Projectile.localAI[0] = target.whoAmI;
            /*
            for (int i = 0; i < 51; i++)
            {
                Vector2 spawnPosition = target.Center + Main.rand.NextVector2Circular(60f, 60f);
                StreamGougeMetaball.SpawnParticle(spawnPosition, Main.rand.NextVector2Circular(3f, 3f), 60f);

                float scale = MathHelper.Lerp(144f, 294f, CalamityUtils.Convert01To010(i / 49f));
                spawnPosition = target.Center + Projectile.velocity.SafeNormalize(Vector2.UnitY) * MathHelper.Lerp(-40f, 90f, i / 49f);
                Vector2 particleVelocity = Projectile.velocity.SafeNormalize(Vector2.UnitY).RotatedByRandom(0.23f) * Main.rand.NextFloat(8.5f, 15f);
                StreamGougeMetaball.SpawnParticle(spawnPosition, particleVelocity, scale);
            }
            */
        }

        public override bool PreKill(int timeLeft)
        {
            Player player = Main.player[Projectile.owner];
            //This is what we call fucking IMPACT (3).
            Main.player[Projectile.owner].Calamity().GeneralScreenShakePower = 12;
            if (Main.zenithWorld)
                SoundEngine.PlaySound(Kunk, Projectile.Center);

            else
                SoundEngine.PlaySound(SlamHamSound, Projectile.Center);

            Particle pulse = new DirectionalPulseRing(Projectile.Center, Vector2.Zero, Color.Violet, new Vector2(2f, 2f), Main.rand.NextFloat(12f, 25f), 0.2f, 4f, 12);
            GeneralParticleHandler.SpawnParticle(pulse);
            Particle pulse2 = new DirectionalPulseRing(Projectile.Center, Vector2.Zero, Color.Aqua, new Vector2(2f, 2f), Main.rand.NextFloat(12f, 25f), 0.1f, 1.5f, 11);
            GeneralParticleHandler.SpawnParticle(pulse2);

            Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Projectile.velocity * 0f, ModContent.ProjectileType<GalaxySmasherBlast>(), Projectile.damage / 2, Projectile.knockBack, Projectile.owner, 0f);

            return false;
        }

        public override bool PreDraw(ref Color lightColor)
        {
            SpriteEffects spriteEffects = SpriteEffects.None;
            if (Projectile.spriteDirection == -1)
                spriteEffects = SpriteEffects.FlipHorizontally;
            Texture2D texture = Terraria.GameContent.TextureAssets.Projectile[Projectile.type].Value;
            Rectangle sourceRectangle = new Rectangle(0, 0, texture.Width + 12, texture.Height + 12);
            Vector2 origin = sourceRectangle.Size() / 2f;

            for (int i = 0; i < Projectile.oldPos.Length; i++)
            {
                float rot = MathHelper.ToRadians(22.5f) * Math.Sign(Projectile.velocity.X);
                Vector2 drawPos = Projectile.oldPos[i] - Main.screenPosition + origin + new Vector2(0f, Projectile.gfxOffY);
                Color color = Projectile.GetAlpha(lightColor) * ((Projectile.oldPos.Length - i) / (float)Projectile.oldPos.Length);
                Main.EntitySpriteDraw(texture, drawPos, new Rectangle?(), color, Projectile.rotation - i * rot, origin, Projectile.scale, spriteEffects, 0);
            }
            return false;
        }
    }
}
