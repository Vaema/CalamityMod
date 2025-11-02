using System;
using System.Linq;
using CalamityMod.Graphics.Primitives;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;
namespace CalamityMod.Projectiles.Summon
{
    public class SarosSunfire : ModProjectile, ILocalizedModType, IPixelatedPrimitiveRenderer
    {
        public new string LocalizationCategory => "Projectiles.Summon";
        public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 20;
            ProjectileID.Sets.TrailingMode[Type] = 2;
            ProjectileID.Sets.MinionShot[Type] = true;
        }

        public override void SetDefaults()
        {
            Projectile.width = 24;
            Projectile.height = 24;
            Projectile.friendly = true;
            Projectile.timeLeft = 180;
            Projectile.DamageType = DamageClass.Summon;
            Projectile.MaxUpdates = 3;
            Projectile.tileCollide = false;
            Projectile.penetrate = 3;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 24;
            Projectile.stopsDealingDamageAfterPenetrateHits = true;
        }

        public override void OnSpawn(IEntitySource source)
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 8;
            SoundEngine.PlaySound(SoundID.Item20, Projectile.Center);
        }

        NPC target = null;
        public override void AI()
        {
            if (Projectile.ai[0] > 1)
            {
                Projectile.ai[0]--;
                Projectile.timeLeft = 120;
            }

            if (target != null && Projectile.localNPCImmunity[target.whoAmI] <= 0 && target.active && !target.dontTakeDamage)
            {
                Projectile.Calamity().HomingTarget = target.whoAmI;
                Projectile.velocity = Projectile.velocity.ToRotation().AngleLerp(Projectile.DirectionTo(target.Center).ToRotation(), 0.5f * (1 - Projectile.ai[0] / 120f)).ToRotationVector2() * Projectile.velocity.Length();
            }
            else
            {
                target = null;
                int targ = -1;
                Projectile.Minion_FindTargetInRange(5000, ref targ, false, (ent, num) =>
                {
                    return (ent is NPC && Projectile.localNPCImmunity[(ent as NPC).whoAmI] <= 0);
                });
                if (targ != -1)
                    target = Main.npc[targ];
            }

            if (Projectile.damage <= 0)
            {
                Projectile.ai[0] = 0;
                Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero) * 4;
                Projectile.timeLeft = (int)MathHelper.Min(8, Projectile.timeLeft);
            }
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            for (int i = 0; i < 12; i++)
            {
                Vector2 dustVelocity = Main.rand.NextVector2Circular(1f, 1f) * 6f;
                float dustScale = Main.rand.NextFloat(3f, 5f);
                Color dustColor = Color.Lerp(Color.OrangeRed, Color.Gold, Main.rand.NextFloat(0.5f, 1f));

                Dust dust = Dust.NewDustDirect(Projectile.Center, 1, 1, DustID.TintableDustLighted, dustVelocity.X, dustVelocity.Y, 0, dustColor, dustScale);
                dust.noGravity = true;
                dust.noLight = false;
                dust.noLightEmittence = true;
            }
        }

        public override void OnKill(int timeLeft)
        {
        }

        public override bool? CanDamage()
        {
            return Projectile.ai[0] <= 90;
        }

        public override bool PreDraw(ref Color lightColor) => false;

        public float FireWidthFunction(float completion)
        {
            float width;
            float maxBodyWidth = 48f * Projectile.scale;
            float curveRatio = 0.2f;
            var positions = Projectile.oldPos.ToList();
            positions.RemoveAll(x => x == Vector2.Zero);
            // Crop the tip of the trail into a conic shape.
            if (completion < curveRatio)
                width = MathF.Pow(completion / curveRatio, 0.5f) * maxBodyWidth;
            else
                width = Utils.Remap(completion, curveRatio, 1f, maxBodyWidth, 0f);

            // Pulse inwards and outwards over time.
            float pulseInterpolant = MathF.Cos(MathHelper.Pi * completion - Main.GlobalTimeWrappedHourly * 20f) * 0.5f + 0.5f;
            float additionalPulseWidth = MathHelper.Lerp(0f, 12f, pulseInterpolant);
            return (width + additionalPulseWidth) * positions.Count() / (float)ProjectileID.Sets.TrailCacheLength[Type];
        }

        public Color FireColorFunction(float completion)
        {
            Color mainColor = new Color(238, 226, 153);
            return Color.Lerp(mainColor, Color.Transparent, completion);
        }

        public float FireCoreWidthFunction(float completion)
        {
            float width;
            float maxBodyWidth = Projectile.scale * 32;
            float curveRatio = 0.25f;
            var positions = Projectile.oldPos.ToList();
            positions.RemoveAll(x => x == Vector2.Zero);

            if (completion < curveRatio)
                width = MathF.Sin(completion / curveRatio * MathHelper.PiOver2) * maxBodyWidth + curveRatio;
            else
                width = Utils.Remap(completion, curveRatio, 1f, maxBodyWidth, 0f);
            return width * positions.Count() / (float)ProjectileID.Sets.TrailCacheLength[Type];
        }

        public Color FireCoreColorFunction(float completion)
        {
            Color mainColor = new Color(255, 191, 73);
            return mainColor;// Color.Lerp(mainColor, Color.Transparent, completion);
        }

        public void RenderPixelatedPrimitives(SpriteBatch spriteBatch, PixelationPrimitiveLayer layer)
        {
            // Render the main trail for the body for the flame.
            GameShaders.Misc["CalamityMod:ImpFlameTrail"].SetShaderTexture(ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Trails/ScarletDevilStreak"));
            PrimitiveRenderer.RenderTrail(Projectile.oldPos, new(FireWidthFunction, FireColorFunction, (_) => Projectile.Size * 0.5f, true, true, GameShaders.Misc["CalamityMod:ImpFlameTrail"]), Projectile.oldPos.Length + 32);

            // Render a smaller, pure white trail in the same position to represent the glowing core of the flame.
            Vector2[] fireCoreLength = Projectile.oldPos.Take(8).ToArray();
            GameShaders.Misc["CalamityMod:ImpFlameTrail"].SetShaderTexture(ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Trails/SylvestaffStreak"));
            PrimitiveRenderer.RenderTrail(fireCoreLength, new(FireCoreWidthFunction, FireCoreColorFunction, (_) => Projectile.Size * 0.5f, true, true, GameShaders.Misc["CalamityMod:ImpFlameTrail"]), fireCoreLength.Length + 24);
        }
    }
}
