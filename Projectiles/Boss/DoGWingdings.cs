using CalamityMod.Fonts;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Graphics;
using System;
using System.IO;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Boss
{
    public class DoGWingdings : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Boss";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public string dialogue;

        public override void SetDefaults()
        {
            Projectile.width = 28;
            Projectile.height = 28;
            Projectile.penetrate = -1;
            Projectile.hostile = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.timeLeft = 180;
            Projectile.scale = 0;
        }

        public override void SendExtraAI(BinaryWriter writer)
        {
            writer.Write(dialogue);
        }

        public override void ReceiveExtraAI(BinaryReader reader)
        {
            dialogue = reader.ReadString();
        }

        public override void AI()
        {
            // Replicae CombatText behaviour 
            float targetScale = 1f;
            Projectile.Opacity += (float)Projectile.ai[0] * 0.05f;
            if ((double)Projectile.Opacity <= 0.6)
            {
                Projectile.ai[0] = 1;
            }
            if (Projectile.Opacity >= 1f)
            {
                Projectile.Opacity = 1f;
                Projectile.ai[0] = -1;
            }
            Projectile.velocity.Y *= 0.8464f;
            
            Projectile.velocity.X *= 0.93f;

            if (Projectile.timeLeft <= 60)
            {
                Projectile.scale -= 0.1f * targetScale;
                if ((double)Projectile.scale < 0.1)
                {
                    Projectile.active = false;
                }
                Projectile.timeLeft = 60;
                Projectile.ai[0] = -1;
                Projectile.scale += 0.07f * targetScale;                
                return;
            }
            if (Projectile.velocity.X < 0f)
            {
                Projectile.rotation += 0.001f;
            }
            else
            {
                Projectile.rotation -= 0.001f;
            }
            if (Projectile.scale < targetScale)
            {
                Projectile.scale += 0.1f * targetScale;
            }
            if (Projectile.scale > targetScale)
            {
                Projectile.scale = targetScale;
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            // Do not draw if the font isn't loaded or text is Cyrillic or Chinese
            if (FontAssetSystem.Wingdings is null || System.Environment.OSVersion.Platform != PlatformID.Win32NT || GameCulture.FromCultureName(GameCulture.CultureName.Chinese).IsActive || GameCulture.FromCultureName(GameCulture.CultureName.Russian).IsActive)
                return false;
            // Otherwise identical drawcode to CombatText
            Vector2 stringSize = FontAssetSystem.Wingdings.Value.MeasureString(dialogue);
            Vector2 origin = new Vector2(stringSize.X * 0.5f, stringSize.Y * 0.5f);
            Color cyan = Color.Cyan;
            float scale = Projectile.scale;
            float red = (int)cyan.R;
            float green = (int)cyan.G;
            float blue = (int)cyan.B;
            float alpha = (int)cyan.A;
            red *= scale * Projectile.Opacity * 0.3f;
            blue *= scale * Projectile.Opacity * 0.3f;
            green *= scale * Projectile.Opacity * 0.3f;
            alpha *= scale * Projectile.Opacity;
            Color color = new Color((int)red, (int)green, (int)blue, (int)alpha);
            for (int l = 0; l < 5; l++)
            {
                float xOffset = 0f;
                float yOffset = 0f;
                switch (l)
                {
                    case 0:
                        xOffset -= 1f;
                        break;
                    case 1:
                        xOffset += 1f;
                        break;
                    case 2:
                        yOffset -= 1f;
                        break;
                    case 3:
                        yOffset += 1f;
                        break;
                    default:
                        red = (float)(int)cyan.R * scale * Projectile.Opacity;
                        blue = (float)(int)cyan.B * scale * Projectile.Opacity;
                        green = (float)(int)cyan.G * scale * Projectile.Opacity;
                        alpha = (float)(int)cyan.A * scale * Projectile.Opacity;
                        color = new Color((int)red, (int)green, (int)blue, (int)alpha);
                        break;
                }
                if (Main.LocalPlayer.gravDir != 1f)
                {
                    float finalYPos = Projectile.position.Y - Main.screenPosition.Y;
                    finalYPos = (float)Main.screenHeight - finalYPos;
                    Main.spriteBatch.DrawString(FontAssetSystem.Wingdings.Value, dialogue.ToUpper(), new Vector2(Projectile.position.X - Main.screenPosition.X + xOffset + origin.X, finalYPos + yOffset + origin.Y), color, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);
                }
                else
                {
                    Main.spriteBatch.DrawString(FontAssetSystem.Wingdings.Value, dialogue.ToUpper(), new Vector2(Projectile.position.X - Main.screenPosition.X + xOffset + origin.X, Projectile.position.Y - Main.screenPosition.Y + yOffset + origin.Y), color, Projectile.rotation, origin, Projectile.scale, SpriteEffects.None, 0f);
                }
            }
            return false;
        }
    }
}
