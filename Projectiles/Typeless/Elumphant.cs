using System;
using System.Collections.Generic;
using CalamityMod.Buffs.StatDebuffs;
using CalamityMod.Dusts;
using CalamityMod.Items.Accessories;
using CalamityMod.Particles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Typeless
{
    public class Elumphant : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Typeless";
        public Player Owner => Main.player[Projectile.owner];
        public static Color color1 = new Color(60, 103, 207);
        public static Color color2 = new Color(103, 188, 214);
        public Color usedColor = Color.White;
        public ref float time => ref Projectile.ai[0];
        public ref float attackTimer => ref Projectile.ai[1];

        public float fxFade = 0; // The glow visuals multiplier
        public float followSpeed = 12; // The speed it follows you, lower is faster
        public float lerpDir = 0;
        public int facing = 0;
        public float actionSpeed = 1;

        public float verticalSquash = 0;
        public float horizontalSquash = 0;

        public float trunkRotation = 0;
        public bool dashing = false;

        Vector2 goalPosition;
        public override void SetStaticDefaults()
        {
            Main.projFrames[Type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 46;
            Projectile.height = 34;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 300;
            Projectile.tileCollide = false;
            Projectile.friendly = true;
            Projectile.penetrate = -1;
        }
        public void SetElumphantPower()
        {
            int usedDefense = (Owner.statDefense / 3);
            Owner.statDefense -= usedDefense;
            Owner.Calamity().frozenCubePower = usedDefense * 0.05f;
        }
        public float GetPower(float efficiency) => 1 + Owner.Calamity().frozenCubePower * efficiency;
        public void GetColor()
        {
            float rate = Main.GlobalTimeWrappedHourly * 5;
            List<Color> eColors = new List<Color>()
            {
                color1,
                color2
            };
            int colorIndex = (int)(rate / 2 % eColors.Count);
            Color currentColor = eColors[colorIndex];
            Color nextColor = eColors[(colorIndex + 1) % eColors.Count];
            usedColor = Color.Lerp(Color.Lerp(currentColor, nextColor, rate % 2f > 1f ? 1f : rate % 1f), Color.White, 0.7f);
        }
        public void ManageSquash()
        {
            float resolutionSpeed = 0.01f / Projectile.MaxUpdates;
            verticalSquash = MathHelper.Lerp(verticalSquash, 0, resolutionSpeed);
            horizontalSquash = MathHelper.Lerp(horizontalSquash, 0, resolutionSpeed);
        }
        public override void AI()
        {
            Projectile.timeLeft++;
            SetElumphantPower();
            GetColor();
            ManageSquash();

            float sine = (float)Math.Sin(time * 0.1f * actionSpeed / MathHelper.Pi);
            float sine2 = (float)Math.Sin(time * 0.15f * actionSpeed / MathHelper.Pi);


            Vector2 baseDestination = Owner.MountedCenter - Vector2.UnitY * (Owner.height / 2 + Projectile.height / 2);
            if (time == 0)
                goalPosition = baseDestination;

            if (dashing)
            {
                Projectile.frame = 1;
                facing = Math.Sign(Projectile.velocity.X);

                goalPosition = Vector2.Lerp(goalPosition, baseDestination, 0.08f);
                Projectile.velocity = (goalPosition - Projectile.Center) / (followSpeed);
            }
            else
            {
                Projectile.frame = 0;
                facing = Math.Sign(Projectile.Center.X == Owner.Center.X ? Owner.direction : Projectile.Center.DirectionTo(Owner.Center).X);

                goalPosition = Vector2.Lerp(goalPosition, baseDestination, 0.08f);

                Projectile.Center = goalPosition;
            }




            Lighting.AddLight(Projectile.Center, usedColor.ToVector3() * 0.3f);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Asset<Texture2D> tex = ModContent.Request<Texture2D>(Texture);
            Texture2D bTexture = ModContent.Request<Texture2D>("CalamityMod/Particles/BloomCircle").Value;
            Texture2D cTexture = ModContent.Request<Texture2D>("CalamityMod/Particles/HalfStar").Value;
            Color drawColor = usedColor * Utils.GetLerpValue(0, 100, time, true);
            Color bodyColor = lightColor;
            float drawMult = 1;
            float attackFade = (float)Math.Pow(1 + (float)Math.Pow(Utils.GetLerpValue(0, 30, attackTimer, true), 4), 3);

            Vector2 shake = Main.rand.NextVector2Circular((attackFade - 1) * 5, (attackFade - 1) * 5);

            for (int i = 0; i < 18; i++) // Backglow
            {
                Vector2 drawOffset = (MathHelper.TwoPi * i / 18f).ToRotationVector2() * 2 * drawMult * attackFade;
                if (attackFade > 1)
                    Main.EntitySpriteDraw(tex.Value, Projectile.Center - Main.screenPosition + drawOffset + shake, null, Color.Lerp(drawColor, Color.DodgerBlue, attackFade - 1) with { A = 0 } * 0.2f * drawMult, Projectile.rotation, tex.Size() * 0.5f, Projectile.scale, facing == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally);
            }

            // Main body
            Rectangle frame = tex.Value.Frame(1, Main.projFrames[Type], 0, Projectile.frame);
            Main.EntitySpriteDraw(tex.Value, Projectile.Center - Main.screenPosition + shake, frame, Color.Lerp(bodyColor, Color.DodgerBlue with { A = 0 }, attackFade - 1), Projectile.rotation, frame.Size() * 0.5f, new Vector2(1f, 1f) * Projectile.scale, facing == -1 ? SpriteEffects.None : SpriteEffects.FlipHorizontally);

            return false;
        }
        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {

        }
        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            return false;
        }
        public override bool? CanDamage() => false;
    }
}
