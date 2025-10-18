using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.ExtraTextures;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Melee
{
    class AmidiasTridentBoltProj : ModProjectile, ILocalizedModType
    {
        public override string Texture => "CalamityMod/Particles/VerticalSmear";
        public override void SetDefaults()
        {
            base.SetDefaults();

            Projectile.width = Projectile.height = 50;
            Projectile.DamageType = DamageClass.Melee;
            Projectile.timeLeft = 100;
            Projectile.friendly = true;
            Projectile.hostile = false;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = 1;
            Projectile.aiStyle = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.extraUpdates = 1;
        }
        bool start = false;
        public override bool PreKill(int timeLeft)
        {

            return base.PreKill(timeLeft);
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            SeaKingsAssurance.Apply(target, Vector2.Zero);
            base.OnHitNPC(target, hit, damageDone);
        }
        public override void AI()
        {
            float sc = MathHelper.Clamp(Projectile.velocity.Length() / 8f, 0f, 0.8f);

            Lighting.AddLight(Projectile.Center, new Vector3(0f, 0.2f, 0.5f));
            Projectile.ai[1]++;
            if (!start)
            {
                Projectile.velocity *= 2f;
                Projectile.ai[0] = MathHelper.ToRadians(Main.rand.NextFloat(-1.6f, 1.6f));
                Projectile.velocity = Projectile.velocity.RotatedBy(-Projectile.ai[0] * 10f);
                start = true;
            }
            else
            {
                NPC npc = CalamityUtils.ClosestNPCAt(Projectile.Center, 150);

                if (npc != null)
                {
                    float rot = Projectile.velocity.ToRotation();
                    float rot2 = rot.AngleLerp(Projectile.AngleTo(npc.Center), 0.1f);

                    Projectile.velocity = Projectile.velocity.RotatedBy(rot2 - rot);
                }
                else
                {
                    Projectile.velocity = Projectile.velocity.RotatedBy(Projectile.ai[0] / 1.5f);
                }
            }

            GeneralParticleHandler.SpawnParticle(
                new AltSparkParticle(Projectile.Center, Projectile.velocity * 0.1f, false, 8, sc, SeaKingsAssurance.BaseColor.MultiplyRGBA(new(1f, 1f, 1f, 0f)))
            );

            Projectile.velocity *= 0.89f;
            if (Projectile.velocity.Length() < 1f) Projectile.Kill();
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Asset<Texture2D> MainTexture = ModContent.Request<Texture2D>("CalamityMod/Particles/VerticalSmear");

            float vel = Projectile.velocity.Length() / 50f;

            Main.EntitySpriteDraw(MainTexture.Value, Projectile.Center - Main.screenPosition, MainTexture.Frame(),
                SeaKingsAssurance.BaseColor.MultiplyRGBA(new(1f, 1f, 1f, 0f)), Projectile.velocity.ToRotation() + MathHelper.PiOver2, MainTexture.Size() * new Vector2(0.5f, 0f), new Vector2(
                    vel, vel * 4f
                    ) * 0.7f, SpriteEffects.None);
            for (int i = 0; i < 5; i++)
            {
                Main.EntitySpriteDraw(MainTexture.Value, Projectile.Center - Main.screenPosition, MainTexture.Frame(),
                    SeaKingsAssurance.LightColor.MultiplyRGBA(new(1f, 1f, 1f, 0f)), Projectile.velocity.ToRotation() + MathHelper.PiOver2, MainTexture.Size() * new Vector2(0.5f, 0f), new Vector2(
                        vel, vel * 4f
                        ) * 0.4f, SpriteEffects.None);
            }

            return false;
        }
    }
}
