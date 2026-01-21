using System;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Graphics.Primitives;
using CalamityMod.Particles;
using Terraria.Graphics.Shaders;
using Microsoft.Xna.Framework.Graphics;

namespace CalamityMod.Projectiles.Magic
{
    public class AquamarineBolt : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Magic";
        public Particle bloom = null;

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailingMode[Projectile.type] = 2;
            ProjectileID.Sets.TrailCacheLength[Projectile.type] = 11;
        }

        public override void SetDefaults()
        {
            Projectile.width = 16;
            Projectile.height = 16;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.penetrate = 1;
            Projectile.timeLeft = 180;
            Projectile.DamageType = DamageClass.Magic;
            Projectile.timeLeft = 180;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 10;
            Projectile.alpha = 255;
        }

        public override void AI()
        {
            // The main bolt simply just homes
            if (Projectile.ai[0] == 0)
            {
                CalamityUtils.HomeInOnNPC(Projectile, false, 600, 10f, 10f);
            }
            // The secondary bolt """"orbits"""" the main one
            else
            {
                Projectile parent = Main.projectile[(int)Projectile.ai[2]];
                if (parent.active && parent.type == Type)
                {
                    Projectile.timeLeft = parent.timeLeft;
                    Projectile.ai[1] += 0.18f;
                    float angle = parent.velocity.ToRotation() + MathHelper.PiOver2;
                    float pulse = (float)Math.Sin(Projectile.ai[1]);
                    float radius = 20.8f;
                    Vector2 offset = angle.ToRotationVector2() * pulse * radius;
                    Projectile.Center = parent.Center - offset;
                }
                else
                {
                    Projectile.tileCollide = true;
                    Projectile.penetrate = 1;
                    Projectile.ai[0] = 0;
                }
            }
            // Bloom particle behind the bolt. If one already exists, update its position and assure it doesn't prematurely die
            if (bloom == null)
            {
                bloom = new GenericBloom(Projectile.Center, Vector2.Zero, Color.LightBlue * 0.5f, 0.5f * Projectile.scale, 600);
                GeneralParticleHandler.SpawnParticle(bloom);
            }
            else
            {
                bloom.Position = Projectile.position;
                bloom.Velocity = Projectile.velocity;
                bloom.Time = 300;
            }

            // fade out and kill the bloom
            if (Projectile.timeLeft < 60)
            {
                Projectile.alpha += 10;
                if (bloom != null)
                {
                    bloom.Kill();
                }
            }
            // if it's not fading out, make some idle sparkles
            else
            {
                if (Projectile.alpha > 0)
                {
                    Projectile.alpha -= 50;
                }
                if (Main.rand.NextBool(4) && Projectile.velocity.Length() > 2)
                {
                    Particle sparkle = new SnowflakeSparkle(Projectile.Center, Vector2.Zero, Color.AliceBlue, Color.AliceBlue, Main.rand.NextFloat(0.05f, 0.5f), 20);
                    GeneralParticleHandler.SpawnParticle(sparkle);
                }
            }
            
            // lite
            Lighting.AddLight(Projectile.Center, (255 - Projectile.alpha) * 0f / 255f, (255 - Projectile.alpha) * 0.25f / 255f, (255 - Projectile.alpha) * 0.25f / 255f);
            Projectile.rotation += 0.3f * Projectile.direction;
        }

        public override Color? GetAlpha(Color lightColor) => Color.White;
        public float PrimitiveWidthFunction(float completionRatio, Vector2 vertexPos)
        {
            return (1 - completionRatio) * 20 * Projectile.scale;
        }

        public Color PrimitiveColorFunction(float completionRatio, Vector2 vertexPos)
        {
            return Color.Lerp(new Color(86, 176, 240), Color.Azure, (float)Math.Pow(completionRatio, 1.5D)) * 0.6f * Projectile.Opacity;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone) => target.AddBuff(ModContent.BuffType<RiptideDebuff>(), 120);
        public override void OnHitPlayer(Player target, Player.HurtInfo info) => target.AddBuff(ModContent.BuffType<RiptideDebuff>(), 120);

        public override bool PreDraw(ref Color lightColor)
        {
            // streakin'
            Main.spriteBatch.EnterShaderRegion();

            GameShaders.Misc["CalamityMod:ImpFlameTrail"].SetShaderTexture(ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Trails/ScarletDevilStreak"));
            PrimitiveRenderer.RenderTrail(Projectile.oldPos, new(PrimitiveWidthFunction, PrimitiveColorFunction, (_,_) => Vector2.Zero, shader: GameShaders.Misc["CalamityMod:ImpFlameTrail"]), 66);
            Main.spriteBatch.ExitShaderRegion();
            Main.EntitySpriteDraw(TextureAssets.Projectile[Type].Value, Projectile.position - Main.screenPosition, null, Projectile.GetAlpha(lightColor) * Projectile.Opacity, Projectile.rotation, Projectile.getRect().Size() * 0.5f, Projectile.scale, 0, 0);

            Main.spriteBatch.ExitShaderRegion();
            return false;
        }

        public override void OnKill(int timeLeft)
        {
            // kill the bloom when the projectile dies if it hasn't already (aka if it hits a tile or npc)
            if (bloom != null)
            {
                bloom.Kill();
            }
        }
    }
}
