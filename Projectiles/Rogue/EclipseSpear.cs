using System;
using System.Collections.Generic;
using System.Linq;
using CalamityMod.Graphics.Primitives;
using CalamityMod.Items.Weapons.Rogue;
using CalamityMod.NPCs;
using CalamityMod.Particles;
using CalamityMod.Utilities.Daybreak;
using CalamityMod.Utilities.Daybreak.Buffers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Rogue
{
    [PierceResistException]
    public class EclipseSpear : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";

        bool initialized = false;
        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 6;
            ProjectileID.Sets.TrailingMode[Type] = 0;
        }

        (Vector2 start, Vector2 end) LinePos = (new(), new());
        float LineWidth = 0;

        public override void SetDefaults()
        {
            Projectile.width = Projectile.height = 48;
            Projectile.friendly = true;
            Projectile.ignoreWater = true;
            Projectile.tileCollide = false;
            Projectile.DamageType = RogueDamageClass.Instance;
            Projectile.MaxUpdates = 1;
            Projectile.penetrate = -1;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = 1;
            Projectile.timeLeft = 600 * Projectile.MaxUpdates;
            Projectile.ContinuouslyUpdateDamageStats = true;
        }
        public override void AI()
        {
            if (!initialized)
            {
                Projectile.ai[0] = -1;
                Projectile.localAI[0] = Projectile.ai[2];
                Projectile.ai[2] = 0;
                initialized = true;
            }

            Projectile.originalDamage = (int)(1000 * (Projectile.ai[2]/20f));
            if (Projectile.ai[0] >= 0 && Projectile.ai[2] >= Projectile.localAI[0])
            {
                LinePos.end = Projectile.position;
                Projectile.velocity = Projectile.DirectionTo(Main.npc[(int)Projectile.ai[0]].Center) * 64;
                Projectile.rotation = Projectile.velocity.ToRotation() + MathHelper.PiOver4;
                Projectile.Center = Main.npc[(int)Projectile.ai[0]].Center + Projectile.velocity;
                Projectile.Damage();
                Projectile.ai[0] = -1;
                LineWidth = 1f;
                LinePos.start = Projectile.Center;
            }
            Projectile.velocity *= 0.9f;
            LineWidth -= 0.05f;
        }
        public override bool PreDraw(ref Color lightColor)
        {
            
            if (LineWidth > 0)
            {
                var device = Main.instance.GraphicsDevice;
                using var lease = RenderTargetPool.Shared.Rent(
                    device,
                    Main.screenWidth / 2,
                    Main.screenHeight / 2,
                    RenderTargetDescriptor.Default
                );

                using (Main.spriteBatch.Scope())
                {
                    using (lease.Scope(clearColor: Color.Transparent))
                    {

                        List<Vector2> posList = [];
                        //For the prim to render properly I need to divide the distance between the positions into a couple points. Just using start and end doesn't render.
                        for (var i = 0; i <= 10; i++)
                        {
                            posList.Add(Vector2.Lerp(Projectile.position, LinePos.end, i / 10f));
                        }
                        var pos = posList.ToArray();

                        GameShaders.Misc["CalamityMod:ImpFlameTrail"].SetShaderTexture(ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Trails/ScarletDevilStreak"));
                        PrimitiveRenderer.RenderTrail(pos, new(FireWidthFunction, FireColorFunction, (_, _) => Projectile.Size * 0.5f, smoothen: true, pixelate: false, shader: GameShaders.Misc["CalamityMod:ImpFlameTrail"], useUnscaledMatrices: true), pos.Length + 32);
                        GameShaders.Misc["CalamityMod:ImpFlameTrail"].SetShaderTexture(ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/Trails/SylvestaffStreak"));
                        PrimitiveRenderer.RenderTrail(pos, new(FireCoreWidthFunction, FireCoreColorFunction, (_, _) => Projectile.Size * 0.5f, smoothen: true, pixelate: false, shader: GameShaders.Misc["CalamityMod:ImpFlameTrail"], useUnscaledMatrices: true), pos.Length + 24);
                    }

                    Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                    Main.spriteBatch.Draw(lease.Target, Vector2.Zero, null, Color.White, 0f, Vector2.Zero, 2f, SpriteEffects.None, 0f);
                    Main.spriteBatch.End();

                }
            }
            var tex = TextureAssets.Item[ModContent.ItemType<EclipsesFall>()];
            Main.EntitySpriteDraw(tex.Value, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, tex.Size() * 0.5f, Projectile.scale, 0);
            using (Main.spriteBatch.Scope())
            {
                Main.spriteBatch.Begin(default, BlendState.NonPremultiplied,null,null,null,null,Main.Transform);
                Main.EntitySpriteDraw(TextureAssets.Projectile[Type].Value, Projectile.Center - Main.screenPosition, null, Color.White * (Projectile.localAI[0] > 0 ? Projectile.ai[2]/Projectile.localAI[0] : 1), Projectile.rotation, TextureAssets.Projectile[Type].Size() * 0.5f, Projectile.scale, 0);
                Main.spriteBatch.End();
            }
            return false;
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            float _ = 0;
            if (Collision.CheckAABBvLineCollision(targetHitbox.TopLeft(),targetHitbox.Size(),Projectile.oldPosition+ new Vector2(Projectile.width,Projectile.height)*0.5f,Projectile.Center,48f,ref _))
            {
                return true;
            }

            return base.Colliding(projHitbox, targetHitbox);
        }

        public override bool? CanHitNPC(NPC target)
        {
            if (Projectile.ai[0] < 0)
                return false;
            return null;
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            //Half damage to any NPC that isn't the primary target
            if (Projectile.ai[0] != target.whoAmI)
                modifiers.SourceDamage *= 0.5f;
        }

        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {
            Projectile.ai[0] = -1;
            Projectile.netUpdate = true;
        }

        public override void OnKill(int timeLeft)
        {
            return;
        }

        public float FireWidthFunction(float completion, Vector2 vertexPos)
        {
            return 96 * LineWidth * MathHelper.Clamp((Projectile.ai[2] / 20f), 0.25f, 1f);
        }

        public Color FireColorFunction(float completion, Vector2 vertexPos)
        {
            Color mainColor = Color.Lerp(new Color(238, 226, 153), new Color(255, 191, 73), (MathF.Sin(completion * MathHelper.TwoPi + Main.GlobalTimeWrappedHourly * 5) + 1) * 0.5f);
            return mainColor * MathF.Pow(1 - completion*1.1f, 0.5f);
        }

        public float FireCoreWidthFunction(float completion, Vector2 vertexPos)
        {
            return 32 * LineWidth * MathHelper.Clamp((Projectile.ai[2] / 20f), 0.25f, 1f);
        }

        public Color FireCoreColorFunction(float completion, Vector2 vertexPos)
        {
            return Color.Black * MathF.Pow(1 - completion * 1.1f, 0.5f);
        }
    }
}
