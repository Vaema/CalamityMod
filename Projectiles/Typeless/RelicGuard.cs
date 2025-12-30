using System;
using System.Collections.Generic;
using CalamityMod.CalPlayer;
using CalamityMod.Dusts;
using CalamityMod.Items.Tools;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Typeless
{
    public class RelicGuard : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Typeless";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public Player Owner => Main.player[Projectile.owner];
        public CalamityPlayer moddedOwner => Owner.Calamity();
        public Color bColor = Color.White;
        public int time = 0;
        public float rotMult = 0.05f;
        public int direction = 1;
        public float rot2 = 0;
        public Vector2 aimVel;
        public float xLerp = 0;
        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 1;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = false;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
        }

        public override void AI()
        {
            float sine = (float)Math.Sin(time * 0.13f / MathHelper.Pi);
            float rate = Main.GlobalTimeWrappedHourly * 2;
            List<Color> eColors = new List<Color>()
            {
                Color.Sienna,
                Color.Peru
            };

            int colorIndex = (int)(rate / 2 % eColors.Count);
            Color currentColor = eColors[colorIndex];
            Color nextColor = eColors[(colorIndex + 1) % eColors.Count];
            bColor = Color.Lerp(currentColor, nextColor, rate % 2f > 1f ? 1f : rate % 1f);

            xLerp = MathHelper.Lerp(xLerp, (30 + 10 * sine) * Math.Sign(aimVel.X), 0.05f);
            Projectile.Center = Owner.MountedCenter + new Vector2(xLerp, -25 + 12 * sine);
            Owner.direction = Math.Sign(aimVel.X);

            if (Owner.HeldItem.type == ModContent.ItemType<RelicOfResilience>())
                Projectile.timeLeft++;
            else
                Projectile.Kill();
            if (Owner.dead)
                Projectile.Kill();

            // Emit some light
            Lighting.AddLight(Projectile.Center, Color.Goldenrod.ToVector3() * 1.5f);

            rot2 = Math.Abs((float)Math.Sin(time * 0.15f / MathHelper.Pi) * 0.2f) + 0.8f;

            if (time % 2 == 0 && false)
            {
                Vector2 dustVel = new Vector2(15, 15).RotatedByRandom(100);
                Dust dust = Dust.NewDustPerfect(Projectile.Center + dustVel, ModContent.DustType<LightDust>(), dustVel * Main.rand.NextFloat(0.1f, 0.4f));
                dust.noGravity = true;
                dust.scale = Main.rand.NextFloat(0.7f, 0.9f) * Utils.GetLerpValue(300, 0, Owner.Calamity().rOfResilienceCooldown, true);
                dust.color = bColor;
                dust.noLightEmittence = true;
            }

            // 15NOV2024: Ozzatron: clamped mouse position unnecessary, only used for direction
            aimVel = Utils.DirectionTo(Owner.Center, Owner.Calamity().mouseWorld);
            float adjustedRot = aimVel.ToRotation();
            Projectile.rotation = Utils.AngleLerp(Projectile.rotation, adjustedRot + (MathHelper.ToRadians(90)), 0.05f);

            float rot = Utils.DirectionFrom(Owner.Center, Projectile.Center).ToRotation() + (MathHelper.ToRadians(90));
            Owner.SetCompositeArmFront(true, Player.CompositeArmStretchAmount.Full, rot);
            Owner.SetCompositeArmBack(true, Player.CompositeArmStretchAmount.Full, rot);

            time++;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D rTexture = ModContent.Request<Texture2D>("CalamityMod/Items/RelicOfResilience").Value;
            Texture2D bTexture = ModContent.Request<Texture2D>("CalamityMod/Particles/BloomCircle").Value;
            Color drawColor = bColor;
            float CDScale = Utils.GetLerpValue(300, 0, Owner.Calamity().rOfResilienceCooldown, true);

            for (int i = 0; i < 2; i++)
            {
                float bScale2 = 0.55f;
                Main.EntitySpriteDraw(bTexture, Projectile.Center - Main.screenPosition, null, Color.Lerp(drawColor, Color.White, i * 0.15f) with { A = 0 }, 0, bTexture.Size() * 0.5f, (bScale2 - i * 0.15f) * rot2 * CDScale, SpriteEffects.None, 0);
            }

            Projectile.DrawProjectileWithBackglow(Color.OrangeRed with { A = 0 } * CDScale, Color.White, 3 * rot2 * CDScale, rTexture, null, Math.Sign(aimVel.X) < 0 ? SpriteEffects.FlipHorizontally : SpriteEffects.None);
            return false;
        }
        public override bool? CanDamage() => false;
    }
}
