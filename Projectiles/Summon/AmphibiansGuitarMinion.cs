using System;
using System.Collections.Generic;
using CalamityMod.Buffs.Summon;
using CalamityMod.Dusts;
using CalamityMod.Particles;
using CalamityMod.Projectiles.BaseProjectiles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.ModLoader;
using static Terraria.ModLoader.ModContent;

namespace CalamityMod.Projectiles.Summon
{
    public class AmphibiansGuitarMinion : BaseMinionProjectile
    {
        public override int AssociatedProjectileTypeID => ProjectileType<AmphibiansGuitarMinion>();

        public override int AssociatedBuffTypeID => BuffType<AmphibiansGuitarBuff>();

        public override ref bool AssociatedMinionBool => ref ModdedOwner.AmphibiansGuitarBool;

        /// <summary>
        /// A property that states which guitar sprite is using from the spritesheet.<br/>
        /// First guitar goes from 0 and the last to 7.
        /// </summary>
        private int GuitarSprite
        {
            get => (int)Projectile.ai[0];
            set => Projectile.ai[0] = MathHelper.Clamp(value, 0, 7);
        }
        public float rotSpeed = 1;
        public int time = 0;
        public int shootCount = 0;
        private float IntendedRotationAngle => MathHelper.TwoPi / (Owner == null ? 1f : MathHelper.Clamp(Owner.ownedProjectileCounts[Type], 1f, 8f)) * Projectile.ai[0] + Main.GlobalTimeWrappedHourly * 2.4f;

        // Position is same for targeting and non targeting but is left open to be changed because they may want it changed
        private Vector2 RotationPosition => Target == null ? Owner.MountedCenter : Owner.MountedCenter;

        private ref float ShootTimer => ref Projectile.ai[1];

        public Color useColor = Color.White;
        public override void SetDefaults()
        {
            base.SetDefaults();
            (Projectile.width, Projectile.height) = (92, 92);
        }

        public override void MinionAI()
        {
            Projectile.rotation = IntendedRotationAngle;
            float sine = MathHelper.Clamp(Math.Abs((float)Math.Sin(Main.GlobalTimeWrappedHourly * 5f / MathHelper.Pi)), 0f, 1f);
            rotSpeed = sine;

            if (Projectile.ai[2] == 0f)
            {
                // Pitch matches the song if played with proper timing
                SoundStyle sound = new("CalamityMod/Sounds/Item/AmphibiansGuitarSummon");
                SoundEngine.PlaySound(sound with { Volume = 0.8f, Pitch = ((Projectile.ai[0] == 2 || Projectile.ai[0] == 5) ? -0.15f : 0f) }, Projectile.Center);

                ShootTimer = Projectile.ai[0] * 10;
                Projectile.ai[2]++;
                Projectile.netUpdate = true;
                Projectile.netSpam = 0;
            }

            Vector2 intendedPosition = RotationPosition - Vector2.UnitY.RotatedBy(IntendedRotationAngle) * (Target == null ? 100f : (Target.Size.Length() / 2f) + (600f - 350 * rotSpeed));
            Projectile.Center = Vector2.Lerp(Projectile.Center, intendedPosition, Utils.Remap(Projectile.DistanceSQ(intendedPosition), 6400f, 0f, 0.1f, 0.3f));

            if (Target != null)
            {
                bool bigShot = shootCount % 3 == 0;
                if (ShootTimer > 80f && Main.myPlayer == Projectile.owner)
                {
                    if (bigShot)
                    {
                        SoundStyle fire = new("CalamityMod/Sounds/Item/Evernote");
                        SoundEngine.PlaySound(fire with { Volume = 0.5f, Pitch = Main.rand.NextFloat(-0.1f, 0.1f), MaxInstances = 10 }, Projectile.Center);
                    }
                    else
                    {
                        SoundStyle fireSmall = new("CalamityMod/Sounds/Item/WulfrumProsthesisShoot");
                        SoundEngine.PlaySound(fireSmall with { Volume = 0.3f, Pitch = Main.rand.NextFloat(0.6f, 0.7f) }, Projectile.Center);
                    }


                    for (int i = 0; i < (bigShot ? 3 : 1); i++)
                    {
                        float rot = i == 1 ? -0.25f : i == 2 ? 0.25f : 0;
                        Projectile.NewProjectile(
                        Projectile.GetSource_FromThis(),
                        Projectile.Center,
                        CalamityUtils.CalculatePredictiveAimToTarget(Projectile.Center, Target, 18f * (1 - Math.Abs(rot))).RotatedBy(rot),
                        ProjectileType<AmphibiansGuitarProjectile>(),
                        Projectile.damage,
                        Projectile.knockBack,
                        Projectile.owner,
                        ai0: (Main.rand.NextBool(2) && Owner.ownedProjectileCounts[Type] == 8).ToInt(),
                        Main.rand.Next(0, 4 + 1),
                        (bigShot ? 5 : 0));
                    }
                    if (bigShot)
                    {
                        Particle blastRing = new CustomPulse(Projectile.Center, Vector2.Zero, useColor, "CalamityMod/Particles/HighResFoggyCircleHardEdge", Vector2.One, Main.rand.NextFloat(-10, 10), 0.01f, 0.09f, 17);
                        GeneralParticleHandler.SpawnParticle(blastRing);
                    }

                    shootCount++;
                    ShootTimer = 0;
                    Projectile.netUpdate = true;
                    Projectile.netSpam = 0;
                }
                if (ShootTimer < 10)
                    Projectile.Center -= Utils.DirectionTo(Projectile.Center, Target.Center) * 10;

                float rate = Main.GlobalTimeWrappedHourly * 2;
                List<Color> eColors = new List<Color>()
                {
                    Color.Red,
                    Color.Cyan,
                    Color.Goldenrod,
                    Color.Magenta,
                    Color.Lime
                };

                int colorIndex = (int)(rate / 2 % eColors.Count);
                Color currentColor = eColors[colorIndex];
                Color nextColor = eColors[(colorIndex + 1) % eColors.Count];
                useColor = Color.Lerp(Color.White, Color.Lerp(currentColor, nextColor, rate % 2f > 1f ? 1f : rate % 1f), 0.7f);

                if (Main.rand.NextBool(3))
                {
                    Dust dust = Dust.NewDustPerfect(Projectile.Center + Main.rand.NextVector2Circular(70, 70), ModContent.DustType<LightDust>(), (Utils.DirectionTo(Projectile.Center, intendedPosition) * -9).RotatedByRandom(0.3f) * Main.rand.NextFloat(0.3f, 1f));
                    dust.noGravity = true;
                    dust.scale = Main.rand.NextFloat(0.75f, 1.25f);
                    dust.color = useColor;
                    dust.noLightEmittence = true;
                }
                else
                {
                    Particle spark = new GlowOrbParticle(Projectile.Center + Main.rand.NextVector2Circular(70, 70), (Utils.DirectionTo(Projectile.Center, intendedPosition) * -9).RotatedByRandom(0.3f) * Main.rand.NextFloat(0.3f, 1f), false, 7, Main.rand.NextFloat(0.5f, 0.8f), useColor, true, false, false);
                    GeneralParticleHandler.SpawnParticle(spark);
                }

                ShootTimer += 1;
            }
            time++;
        }

        public override bool? CanDamage() => false;

        public override bool PreDraw(ref Color lightColor)
        {
            Texture2D texture = TextureAssets.Projectile[Type].Value;
            Rectangle frame = texture.Frame(horizontalFrames: 8, frameX: GuitarSprite);

            Projectile.DrawProjectileWithBackglow(useColor with { A = 0 }, lightColor, Target == null ? 0 : 8, texture, frame);

            Main.EntitySpriteDraw(
                texture,
                Projectile.Center - Main.screenPosition,
                frame,
                Color.White,
                Projectile.rotation,
                frame.Size() * 0.5f,
                Projectile.scale,
                SpriteEffects.None);
            return false;
        }
    }
}
