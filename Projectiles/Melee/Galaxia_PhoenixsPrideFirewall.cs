using CalamityMod.CalPlayer;
using CalamityMod.Graphics.Primitives;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Melee
{
    public class PhoenixsPrideFirewall : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Melee";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";
        public const int TornadoHeight = 480;

        public float Scale; // Size of the firewall, scales with NPC width
        public float fadeOut = 0.75f; // Used to control the firewall fading out at the end of its lifespan
        public NPC Target => Main.npc[(int)Projectile.ai[0]];
        public override void SetStaticDefaults() => ProjectileID.Sets.DrawScreenCheckFluff[Type] = 10000;

        public override void SetDefaults()
        {
            Projectile.width = 408;
            Projectile.height = 102;
            Projectile.friendly = true;
            Projectile.tileCollide = false;
            Projectile.ignoreWater = true;
            Projectile.penetrate = -1;
            Projectile.alpha = 255;
            Projectile.scale = Scale;
            Projectile.timeLeft = 150;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
        }

        public override void AI()
        {
            // Constantly follow its target
            // Die if the target is no longer active
            if (!Target.active)
            {
                Projectile.Kill();
                return;
            }
            else
                Projectile.Center = Target.Center + Target.velocity;

            // Fade out at the end of its lifespan
            if (Projectile.timeLeft < 15)
                fadeOut -= 0.05f;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float _ = 0f;
            return Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(),
                targetHitbox.Size(),
                Projectile.Bottom,
                Projectile.Bottom - Vector2.UnitY * TornadoHeight,
                72,
                ref _);
        }

        public float WidthFunction(float completionRatio) => 50f * Scale * (completionRatio > 0.5f ? 1f : completionRatio * 2f);

        public override bool PreDraw(ref Color lightColor)
        {
            Main.spriteBatch.EnterShaderRegion();

            GameShaders.Misc["CalamityMod:Bordernado"].UseSaturation(-0.2f);
            GameShaders.Misc["CalamityMod:Bordernado"].UseOpacity(fadeOut);
            GameShaders.Misc["CalamityMod:Bordernado"].SetShaderTexture(ModContent.Request<Texture2D>("Terraria/Images/Misc/Perlin"));
            Vector2[] drawPoints = new Vector2[5];
            Vector2 upwardAscent = Vector2.UnitY * TornadoHeight;

            Vector2 bottom = Projectile.Center;
            Vector2 top = bottom - upwardAscent;
            for (int i = 0; i < drawPoints.Length - 1; i++)
                drawPoints[i] = Vector2.Lerp(top, bottom, i / (float)(drawPoints.Length - 1));

            drawPoints[drawPoints.Length - 1] = bottom;
            PrimitiveRenderer.RenderTrail(drawPoints, new(WidthFunction, (_) => Color.Orange, shader: GameShaders.Misc["CalamityMod:Bordernado"]), 80);

            Main.spriteBatch.ExitShaderRegion();

            Texture2D vortexTexture = ModContent.Request<Texture2D>("CalamityMod/Projectiles/Boss/OldDukeVortex").Value;
            for (int i = 0; i < 90; i++)
            {
                float angle = MathHelper.TwoPi * i / (25f * Scale) + Main.GlobalTimeWrappedHourly * MathHelper.TwoPi;
                Color drawColor = Color.White * 0.04f;
                drawColor.A = 0;
                Vector2 drawPosition = bottom + angle.ToRotationVector2() * 4f - Main.screenPosition;

                drawPosition += (angle + Main.GlobalTimeWrappedHourly * i / 16f).ToRotationVector2() * 6f;
                Main.EntitySpriteDraw(vortexTexture, drawPosition, null, drawColor * fadeOut, angle + MathHelper.PiOver2, vortexTexture.Size() * 0.5f, 0.2f * Scale, SpriteEffects.None, 0);
            }

            return false;
        }
    }
}
