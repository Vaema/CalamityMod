using System;
using CalamityMod.Dusts;
using CalamityMod.Systems.Graphic.PixelationSystem;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Projectiles.Rogue
{
    public class SealedSingularityProjectile : ModProjectile, ILocalizedModType
    {
        public new string LocalizationCategory => "Projectiles.Rogue";
        public override string Texture => "CalamityMod/Items/Weapons/Rogue/SealedSingularity";

        public override void SetStaticDefaults()
        {
            ProjectileID.Sets.TrailCacheLength[Type] = 3;
            ProjectileID.Sets.TrailingMode[Type] = 0;
        }

        public override void SetDefaults()
        {
            Projectile.width = 32;
            Projectile.height = 32;
            Projectile.friendly = true;
            Projectile.tileCollide = true;
            Projectile.penetrate = -1;
            Projectile.MaxUpdates = 2;
            Projectile.timeLeft = 300;
            Projectile.DamageType = RogueDamageClass.Instance;
            Projectile.usesLocalNPCImmunity = true;
            Projectile.localNPCHitCooldown = -1;
        }

        int targetID = -1;

        NPC target => Main.npc[targetID];
        ref float Timer => ref Projectile.ai[0];
        ref float TimerMax => ref Projectile.ai[1];
        ref float AIState => ref Projectile.ai[2];

        bool Stealth => Projectile.Calamity().stealthStrike;

        int bounceCooldown = 0;
        public override void AI()
        {
            Lighting.AddLight(Projectile.Center, new Vector3(1, 1, 1));
            Timer++;
            bounceCooldown--;

            if (AIState == 0)
            {
                Projectile.rotation += 0.175f * Projectile.direction;
                if (Projectile.timeLeft < 280)
                    Projectile.velocity.Y += 0.22f;

                if (TimerMax - Timer < 30)
                    Projectile.velocity *= 0.925f;
            }
            if (AIState == 1)
            {
                Projectile.timeLeft++;
                Projectile.velocity = new Vector2(
                    MathF.Sin(Timer),
                    MathF.Sin(Timer * 0.7f)
                    );
                if (Stealth)
                {
                    Projectile.velocity *= 1 + Math.Clamp(Timer / TimerMax, 0f, 1f) * 2f;
                }
            }

            if (AIState == 2)
            {
                if (targetID == -1 || !target.active || !target.CanBeChasedBy())
                {
                    Timer = TimerMax;
                }
                else
                {
                    Projectile.velocity = Projectile.DirectionTo(target.Center) * MathHelper.Clamp(Timer / 4f, 0f, 16f);
                }
            }

            if (Timer > TimerMax && AIState == 0)
            {
                var sizee = Stealth ? 900 : 600;
                Projectile.localNPCHitCooldown = 60;
                Projectile.ResetLocalNPCHitImmunity();
                Projectile.tileCollide = false;
                Timer = 0;
                TimerMax = Stealth ? 600 : 300;
                Projectile.timeLeft += 180;
                AIState = 1;
            }
            if (Timer > TimerMax && AIState == 1)
            {
                AIState = 2;
                Timer = 0;
                TimerMax = 300;
                Projectile.ResetLocalNPCHitImmunity();
            }

            if (Timer >= TimerMax && AIState == 2)
            {
                Projectile.ResetLocalNPCHitImmunity();
                Projectile.Damage();

                for (var i = 0; i < 20; i++)
                {
                    var d = Dust.NewDustPerfect(Projectile.Center, ModContent.DustType<LemonNadeExplodeDust>(), Main.rand.NextVector2CircularEdge(15, 15) * Main.rand.NextFloat(0.25f, 1f), Scale: Main.rand.NextFloat(0.5f, 1f));
                }

                SoundEngine.PlaySound(SoundID.Item62 with { pitch = 1f }, Projectile.Center);
                SoundEngine.PlaySound(SoundID.Item111 with { pitch = 0.5f }, Projectile.Center);
                for (int index = 0; index < 3; ++index)
                {
                    float SpeedX = -Projectile.velocity.X * Main.rand.Next(40, 70) * 0.01f + Main.rand.Next(-20, 21) * 0.4f;
                    float SpeedY = -Projectile.velocity.Y * Main.rand.Next(40, 70) * 0.01f + Main.rand.Next(-20, 21) * 0.4f;
                    Projectile.NewProjectile(Projectile.GetSource_FromThis(), Projectile.Center.X + SpeedX, Projectile.Center.Y + SpeedY, SpeedX, SpeedY, ModContent.ProjectileType<SealedSingularityGore>(), 20, 0f, Projectile.owner, index, 0f);
                }
                Projectile.Kill();
            }
        }

        public override bool PreDraw(ref Color lightColor)
        {
            if (AIState == 1)
                PixelationManager.AddPixelatedDrawer((matrix) => DrawAuraOutside(this, matrix), Enums.GeneralDrawLayer.AfterEverything);
            Main.EntitySpriteDraw(TextureAssets.Projectile[Type].Value, Projectile.Center - Main.screenPosition, null, Color.White, Projectile.rotation, TextureAssets.Projectile[Type].Size() * 0.5f, Projectile.scale, 0);
            return false;
        }
        private static void DrawAuraOutside(SealedSingularityProjectile mproj, Matrix matrix)
        {

            Vector2 drawPosition = mproj.Projectile.Center - Main.screenPosition;

            //Draw the outer particles
            Main.spriteBatch.EnterShaderRegion(matrix: matrix);
            GameShaders.Misc["CalamityMod:OtherworldBarrierDistortion"].UseOpacity((mproj.Stealth ? 4 : 2) * (mproj.Timer / mproj.TimerMax));
            GameShaders.Misc["CalamityMod:OtherworldBarrierDistortion"].UseSaturation(0.1f);
            GameShaders.Misc["CalamityMod:OtherworldBarrierDistortion"].SetShaderTexture(ModContent.Request<Texture2D>("CalamityMod/ExtraTextures/GreyscaleGradients/Neurons"), 1);
            GameShaders.Misc["CalamityMod:OtherworldBarrierDistortion"].Apply();
            Texture2D telegraphBase = ModContent.Request<Texture2D>("CalamityMod/Particles/BloomRing").Value;
            Main.EntitySpriteDraw(telegraphBase, drawPosition, null, Color.White * mproj.Projectile.Opacity * 0.75f * Math.Clamp(mproj.Timer / 15f, 0f, 1f), mproj.Projectile.whoAmI, telegraphBase.Size() / 2f, (mproj.Stealth ? 900f : 600f) * mproj.Projectile.Opacity / telegraphBase.Width * Math.Clamp(1 - (mproj.Timer - mproj.TimerMax + 15) / 15f, 0f, 1f), 0, 0);
            Main.EntitySpriteDraw(telegraphBase, drawPosition, null, new Color(36, 0, 66) * mproj.Projectile.Opacity * 0.75f * Math.Clamp(mproj.Timer / 15f, 0f, 1f), mproj.Projectile.whoAmI, telegraphBase.Size() / 2f, (mproj.Stealth ? 900f : 600f) * mproj.Projectile.Opacity / telegraphBase.Width * Math.Clamp(1 - (mproj.Timer - mproj.TimerMax + 15) / 15f, 0f, 1f), 0, 0);
            Main.spriteBatch.ExitShaderRegion(matrix: matrix);
        }

        public override void ModifyHitPlayer(Player target, ref Player.HurtModifiers modifiers)
        {
            modifiers.SourceDamage *= 0.002f;
        }
        public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
        {

            switch (AIState)
            {
                case 0:
                    {
                        if (bounceCooldown <= 0)
                        {
                            Projectile.velocity *= -1f;
                            bounceCooldown = 5;
                        }
                        if (TimerMax - Timer > 30)
                            Timer = TimerMax - 30;
                        goto case 1;
                    }
                case 1:
                    {
                        if (target.Calamity().IsArmored())
                            return;
                        if (targetID == -1 || !this.target.active || !this.target.CanBeChasedBy())
                        {
                            targetID = target.whoAmI;
                        }
                        else
                        {
                            if (this.target.life < target.life)
                            {
                                targetID = target.whoAmI;
                            }
                        }
                        return;
                    }
                case 2:
                    {
                        if (targetID == target.whoAmI)
                            Timer = TimerMax;
                        return;
                    }
            }
            return;
        }

        public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
        {
            switch (AIState)
            {
                case 1:
                    modifiers.SourceDamage /= 6;
                    return;
                case 2:
                    modifiers.SourceDamage *= 2;
                    return;
            }
        }

        public override bool? Colliding(Rectangle projHitbox, Rectangle targetHitbox)
        {
            if (AIState == 1)
            {
                return CalamityUtils.CircularHitboxCollision(projHitbox.Center(), (Stealth ? 900 : 600) * 0.4f, targetHitbox);
            }
            return base.Colliding(projHitbox, targetHitbox);
        }

        // Make it bounce on tiles.
        public override bool OnTileCollide(Vector2 oldVelocity)
        {
            if (Projectile.width < 100)
            {
                SoundEngine.PlaySound(SoundID.Dig, Projectile.Center);
                Collision.HitTiles(Projectile.position, Projectile.velocity, Projectile.width, Projectile.height);
            }

            if (Projectile.velocity.X != oldVelocity.X)
            {
                Projectile.velocity.X = -oldVelocity.X;
            }
            if (Projectile.velocity.Y != oldVelocity.Y)
            {
                Projectile.velocity.Y = -oldVelocity.Y;
            }
            Projectile.velocity *= 0.75f;
            return false;
        }
    }

}
