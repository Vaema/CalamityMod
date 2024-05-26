using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalamityMod.Particles;
using Microsoft.Build.Construction;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.Utilities.Terraria.Utilities;

namespace CalamityMod.Projectiles.Magic
{
    public class SongOfParadiseNote : ModProjectile, ILocalizedModType
    {
        public float PopRadius => 55f;
        public new string LocalizationCategory => "Projectiles.Magic";
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 8;
            ProjectileID.Sets.TrailingMode[Projectile.type] = 0;
        }
        public override void OnSpawn(IEntitySource source)
        {
            Projectile.ai[1] = Main.rand.NextFloat(4, 6);
            Projectile.frame = Main.rand.Next(2);

            if (Projectile.ai[2] == 0) Projectile.velocity = Projectile.velocity.RotatedBy(MathHelper.ToRadians(-5f));
            else Projectile.velocity = Projectile.velocity.RotatedBy(MathHelper.ToRadians(5f));
        }

        public override void SetDefaults()
        {
            Projectile.width = 12;
            Projectile.scale = 0.75f;
            Projectile.height = 12;
            Projectile.friendly = true;
            Projectile.tileCollide = true;
            Projectile.ignoreWater = true;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.penetrate = -1;
            Projectile.alpha = 255;
            Projectile.timeLeft = 300;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 30;
        }
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            Projectile.ai[2] = 2;
            Projectile.velocity = oldVelocity;
            return false;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Projectile.ai[2] = 2;
        }
        public override void AI()
        {
            if (Projectile.ai[2] <= 1)
            {
                Lighting.AddLight((int)Projectile.Center.X / 16, (int)Projectile.Center.Y / 16, TorchID.Mushroom, 0.5f);

                Projectile.ai[0]++;

                Projectile.velocity = Projectile.velocity.RotatedBy(Math.Sin((Projectile.ai[0] / Projectile.ai[1]) - 0.5f) * MathHelper.ToRadians(Projectile.ai[2] == 0 ? 1.5f : -1.5f));
            }
            else
            {
                Projectile.tileCollide = false;
                Projectile.velocity *= 0.85f;
                Projectile.ai[2]++;
                if (Projectile.ai[2] > 25)
                {
                    Projectile.Kill();
                }
            }
        }
        public override void Kill(int timeLeft)
        {
            if (Projectile.ai[2] > 1)
            {
                SoundEngine.PlaySound(SoundID.DD2_DrakinShot.WithPitchOffset(0.5f), Projectile.Center);
                foreach (NPC npc in Main.npc)
                {
                    if (!npc.dontTakeDamage && !npc.townNPC && npc.Distance(Projectile.Center) < PopRadius)
                    {
                        npc.SimpleStrikeNPC(Projectile.damage, Projectile.direction, damageType: DamageClass.Magic, damageVariation: true);
                    }
                }
            }
            SoundEngine.PlaySound(SoundID.DD2_DrakinShot.WithPitchOffset(1f), Projectile.Center);

            for (int j = 0; j < 360; j+=Main.rand.Next(60,100))
            {
                float ints = 0.5f;
                ints += Main.rand.NextFloat(0.2f);

                for (int i = 0; i < 15; i++)
                {
                    float rand = Main.rand.NextFloat(-30, 30);

                    float intensity = MathHelper.Lerp(1f, 0f, Math.Abs(rand) / 30f);

                    GeneralParticleHandler.SpawnParticle(new GlowOrbParticle(Projectile.Center, new Vector2(1, 0).RotatedBy(MathHelper.ToRadians(j + rand)) * 5 * intensity, false, 40, Main.rand.NextFloat(1f, 1.5f) * intensity, Color.SkyBlue, true));
                }
            }

        }
        public override bool PreDraw(ref Color lightColor)
        {
            Asset<Texture2D> tex = ModContent.Request<Texture2D>(Texture);
            Rectangle frame = tex.Frame(verticalFrames: 2, frameY: Projectile.frame);

            if (CalamityConfig.Instance.Afterimages)
            {
                for (float i = 1; i <= Projectile.oldPos.Length; i++)
                {
                    Main.EntitySpriteDraw(tex.Value, Projectile.oldPos[(int)i - 1] - Main.screenPosition + (Projectile.Size / 2), frame, Color.Lerp(Color.AliceBlue, Color.Navy, i / (int)Projectile.oldPos.Length).MultiplyRGBA(Color.Lerp(new Color(255f, 255f, 255f, 0f), new Color(0f, 0f, 0f, 0f), i / Projectile.oldPos.Length)), 0f, frame.Size() / 2, MathHelper.Lerp(1f, 0f, i / (int)Projectile.oldPos.Length), SpriteEffects.None);
                }
            }
            Main.EntitySpriteDraw(tex.Value, Projectile.Center - Main.screenPosition, frame, Color.White, 0f, frame.Size() / 2, 1f, SpriteEffects.None);
            return false;
        }
    }
}
