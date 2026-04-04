using System;
using System.Linq;
using CalamityMod.Graphics.Primitives;
using CalamityMod.Utilities.Daybreak;
using CalamityMod.Utilities.Daybreak.Buffers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Rogue
{
    public class EclipseFragment : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";
        public static int lifetime => 1200;
        Color? color = null;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.CultistIsResistantTo[Type] = true;
            ProjectileID.Sets.TrailCacheLength[Type] = 32;
            ProjectileID.Sets.TrailingMode[Type] = 2;
        }
        public override void SetDefaults()
        {
            Projectile.width = 40;
            Projectile.height = 40;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.penetrate = 5;
            Projectile.timeLeft = lifetime;
            Projectile.DamageType = RogueDamageClass.Instance;
            Projectile.usesIDStaticNPCImmunity = true;
            Projectile.idStaticNPCHitCooldown = 10;
            Projectile.MaxUpdates = 2;
            Projectile.stopsDealingDamageAfterPenetrateHits = true;
            Projectile.tileCollide = false;
        }

        public override void AI()
        {
            Projectile.rotation += Projectile.direction * 0.02f;
            if (Projectile.ai[0] < -1)
            {
                Projectile.penetrate = 1;
                Projectile.damage = Projectile.originalDamage;
                Projectile.stopsDealingDamageAfterPenetrateHits = false;
                var target = Projectile.FindTargetWithinRange(4000);
                if (target is not null)
                {
                    Projectile.velocity += Projectile.DirectionTo(target.Center) * 2;
                    Projectile.velocity *= 0.95f;
                }
            }

            if (Projectile.ai[0] == 0f || Projectile.ai[2] > 0)
            {
                if (Projectile.timeLeft < (lifetime - Projectile.ai[1]) && Projectile.ai[2] >= 0)
                {
                    Projectile.velocity = Projectile.velocity.SafeNormalize(Vector2.Zero);
                    Projectile.velocity *= Projectile.ai[2];
                    Projectile.ai[2]--;
                }
            }
            else
            {
                if (Main.projectile.IndexInRange((int)Projectile.ai[0] - 1) && Main.projectile[(int)Projectile.ai[0] - 1].active)
                {
                    if (Projectile.timeLeft > 100)
                        Projectile.timeLeft = 100;
                    Projectile.Center = Vector2.Lerp(Projectile.Center, Main.projectile[(int)Projectile.ai[0] - 1].Center, (1 - (Projectile.timeLeft / 100f)));
                    Projectile.velocity = new(0, 1E-05f);
                    if (Projectile.Distance(Main.projectile[(int)Projectile.ai[0] - 1].Center) < 16)
                    {
                        Main.projectile[(int)Projectile.ai[0] - 1].ai[2]++;
                        Main.projectile[(int)Projectile.ai[0] - 1].netUpdate = true;
                        Projectile.active = false;
                    }
                }
            }
        }
        public override bool? CanHitNPC(NPC target)
        {
            if (Projectile.ai[0] < -1 && target.Calamity().IsArmored())
                return false;
            return null;
        }

        public override void OnKill(int timeLeft)
        {
            if (Main.netMode != NetmodeID.MultiplayerClient)
                Projectile.NewProjectile(Projectile.GetSource_Death(), Projectile.Center, Vector2.Zero, ModContent.ProjectileType<EclipseStealthBoom>(), Projectile.damage, Projectile.knockBack, Projectile.owner);
        }
        public override bool PreDraw(ref Color lightColor)
        {
            Main.spriteBatch.End(out var ss);
            var device = Main.instance.GraphicsDevice;
            using var lease = RenderTargetPool.Shared.Rent(
                device,
                Main.screenWidth / 2,
                Main.screenHeight / 2,
                RenderTargetDescriptor.Default
            );
            using (lease.Scope(clearColor: Color.Transparent))
            {
                var list = Projectile.oldPos.Take(16).ToArray();

                GameShaders.Misc["CalamityMod:ImpFlameTrail"].SetShaderTexture(ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Trails/ScarletDevilStreak"));
                PrimitiveRenderer.RenderTrail(list, new(FireWidthFunction, FireColorFunction, (_, _) => Projectile.Size * 0.5f, smoothen: true, pixelate: false, shader: GameShaders.Misc["CalamityMod:ImpFlameTrail"], useUnscaledMatrices: true), 32);

                GameShaders.Misc["CalamityMod:ImpFlameTrail"].SetShaderTexture(ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Trails/ScarletDevilStreak"));
                PrimitiveRenderer.RenderTrail(list, new(FireCoreWidthFunction, FireCoreColorFunction, (_, _) => Projectile.Size * 0.5f, smoothen: true, pixelate: false, shader: GameShaders.Misc["CalamityMod:ImpFlameTrail"], useUnscaledMatrices: true), 32);
            }
            float dis = Projectile.position.Distance(Projectile.oldPos.Last()) / 32;
            if (dis > 1)
            {
                dis = 1f;
            }
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, Main.Rasterizer, null, Main.Transform);
            Main.spriteBatch.Draw(lease.Target, Vector2.Zero, null, Color.White * dis, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0f);
            Main.spriteBatch.End();

            Main.spriteBatch.Begin(ss);

            using (Main.spriteBatch.Scope())
            {
                Main.spriteBatch.Begin(default, BlendState.NonPremultiplied, null, null, null, null, Main.Transform);
                Main.EntitySpriteDraw(TextureAssets.Projectile[Type].Value, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, TextureAssets.Projectile[Type].Size() * 0.5f, Projectile.scale, 0);
                Main.spriteBatch.End();
            }
            return false;
        }

        // Matches Saros Possesion sunfire with slight edits
        public float FireWidthFunction(float completion, Vector2 vertexPos)
        {
            float width;
            float maxBodyWidth = 24f * Projectile.scale;
            float curveRatio = 0.2f;
            var positions = Projectile.oldPos.ToList();
            positions.RemoveAll(x => x == Vector2.Zero);
            if (completion < curveRatio)
                width = MathF.Pow(completion / curveRatio, 0.5f) * maxBodyWidth;
            else
                width = Utils.Remap(completion, curveRatio, 1f, maxBodyWidth, 0f);

            // Pulse inwards and outwards over time.
            float pulseInterpolant = MathF.Cos(MathHelper.Pi * completion - Main.GlobalTimeWrappedHourly * 20f) * 0.5f + 0.5f;
            float additionalPulseWidth = MathHelper.Lerp(0f, 12f, pulseInterpolant);
            return (width + additionalPulseWidth) * positions.Count() / (float)ProjectileID.Sets.TrailCacheLength[Type];
        }

        public Color FireColorFunction(float completion, Vector2 vertexPos)
        {
            Color mainColor = new Color(238, 226, 153);
            return Color.Lerp(mainColor, Color.Transparent, completion);
        }

        public float FireCoreWidthFunction(float completion, Vector2 vertexPos)
        {
            float width;
            float maxBodyWidth = Projectile.scale * 16;
            float curveRatio = 0.25f;
            var positions = Projectile.oldPos.ToList();
            positions.RemoveAll(x => x == Vector2.Zero);

            if (completion < curveRatio)
                width = MathF.Sin(completion / curveRatio * MathHelper.PiOver2) * maxBodyWidth + curveRatio;
            else
                width = Utils.Remap(completion, curveRatio, 1f, maxBodyWidth, 0f);
            return width * positions.Count() / (float)ProjectileID.Sets.TrailCacheLength[Type];
        }

        public Color FireCoreColorFunction(float completion, Vector2 vertexPos)
        {
            Color mainColor = new Color(255, 191, 73);
            return mainColor;
        }
    }
}
