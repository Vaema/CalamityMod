using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Projectiles.Magic
{
    public class SHPB : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Magic";
        public int explosionTimer = 120;

        public override void SetStaticDefaults()
        {
            Main.projFrames[Projectile.type] = 4;
        }

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.alpha = 255;
            Projectile.scale = 0.4f;
            Projectile.timeLeft = 300;
            Projectile.DamageType = DamageClass.Magic;
        }

        public static Color FindColorForSoul(int projai)
        {
            Color returnColor = new(0, 0, 0);
            switch (projai)
            {
                case 0:
                    returnColor = new(240, 29, 196);
                    break;
                case 1:
                    returnColor = new(123, 29, 220);
                    break;
                case 2:
                    returnColor = new(106, 240, 250);
                    break;
                case 3:
                    returnColor = new(4, 51, 222);
                    break;
                case 4:
                    returnColor = new(79, 255, 124);
                    break;
                case 5:
                    returnColor = new(255, 128, 20);
                    break;
            }
            return returnColor;
        }

        public override void AI()
        {
            // Light and fade in
            float lights = (float)Main.rand.Next(90, 111) * 0.01f;
            lights *= Main.essScale;
            Lighting.AddLight(Projectile.Center, 1f * lights, 0.2f * lights, 0.75f * lights);
            Projectile.alpha -= 2;

            // Animation
            Projectile.frameCounter++;
            if (Projectile.frameCounter > 4)
            {
                Projectile.frameCounter = 0;
                Projectile.frame++;
                if (Projectile.frame > 3)
                    Projectile.frame = 0;
            }

            // Size pulsing
            if (Projectile.localAI[0] == 0f)
            {
                Projectile.scale += 0.05f;
                if (Projectile.scale > 1.9f)
                    Projectile.localAI[0] = 1f;
            }
            else
            {
                Projectile.scale -= 0.05f;
                if (Projectile.scale < 1.5f)
                    Projectile.localAI[0] = 0f;
            }

            Projectile.velocity.X *= 0.985f;
            Projectile.velocity.Y *= 0.985f;
            float explodeRange = 250f;
            bool canExplode = false;
            foreach (NPC n in Main.ActiveNPCs)
            {
                if (n.CanBeChasedBy(Projectile, false) && Collision.CanHit(Projectile.Center, 1, 1, n.Center, 1, 1))
                {
                    float npcX = n.position.X + (float)(n.width / 2);
                    float npcY = n.position.Y + (float)(n.height / 2);
                    float npcDist = Math.Abs(Projectile.position.X + (float)(Projectile.width / 2) - npcX) + Math.Abs(Projectile.position.Y + (float)(Projectile.height / 2) - npcY);
                    if (npcDist < explodeRange)
                    {
                        explodeRange = npcDist;
                        canExplode = true;
                    }
                }
            }
            if (canExplode)
            {
                explosionTimer--;
                if (explosionTimer <= 0)
                {
                    Projectile.Kill();
                }
            }
        }

        public override Color? GetAlpha(Color lightColor) => FindColorForSoul((int)Projectile.ai[0]);

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
            Rectangle frame = tex.Frame(1, Main.projFrames[Type], 0, Projectile.frame);
            Main.EntitySpriteDraw(tex, Projectile.Center - Main.screenPosition, frame, Projectile.GetAlpha(lightColor), Projectile.rotation, frame.Size() / 2f, Projectile.scale, SpriteEffects.None);
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            SoundEngine.PlaySound(SoundID.Item105, Projectile.Center);
            if (Main.LocalPlayer.Calamity().GeneralScreenShakePower < 3.5f)
                Main.LocalPlayer.Calamity().GeneralScreenShakePower = 3.5f;

            if (Projectile.owner == Main.myPlayer)
            {
                Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<SHPExplosion>(), (int)(Projectile.damage * 1.5f), Projectile.knockBack, Projectile.owner, Projectile.ai[0], 0f);
            }
        }
    }
}
