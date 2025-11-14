using System;
using System.Collections.Generic;
using CalamityMod.Events;
using CalamityMod.NPCs;
using CalamityMod.NPCs.SupremeCalamitas;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;

namespace CalamityMod.Skies
{
    public class SCalSky : CustomSky
    {
        public class Cinder
        {
            public int Time;
            public int Lifetime;
            public int IdentityIndex;
            public float Scale;
            public float Depth;
            public Color DrawColor;
            public Vector2 Velocity;
            public Vector2 Center;
            public float Opacity = 0;

            public Cinder(int lifetime, int identity, float depth, Color color, Vector2 startingPosition, Vector2 startingVelocity)
            {
                Lifetime = lifetime;
                IdentityIndex = identity;
                Depth = depth;
                DrawColor = color;
                Center = startingPosition;
                Velocity = startingVelocity;
            }
        }

        private bool isActive = false;
        private float intensity = 0f;
        private int SCalIndex = -1;
        public List<Cinder> Cinders = new List<Cinder>();
        private Asset<Texture2D> backgroundTex;
        private Asset<Texture2D> cinderTex;

        public static bool RitualDramaProjectileIsPresent
        {
            get;
            internal set;
        }

        public static int CinderReleaseChance
        {
            get
            {
                if (!Main.npc.IndexInRange(CalamityGlobalNPC.SCal) || Main.npc[CalamityGlobalNPC.SCal].type != ModContent.NPCType<SupremeCalamitas>())
                    return int.MaxValue;

                NPC scal = Main.npc[CalamityGlobalNPC.SCal];
                float lifeRatio = scal.life / (float)scal.lifeMax;

                // Release a good amount of cinders during bullet hells. If this is too distracting this can be
                // inverted so that less appear than usual instead.
                if (scal.ModNPC<SupremeCalamitas>().bulletHellCounter2 % 900 != 0)
                    return lifeRatio <= 0.1f ? 8 : 11;

                // Release more and more cinders at the end of the battle until the acceptance phase, where it all slows down dramatically, with just a few ashes.
                if (lifeRatio < 0.1f)
                {
                    if (lifeRatio <= 0.01f)
                        return 25;

                    return (int)Math.Round(MathHelper.Lerp(2f, 11f, Utils.GetLerpValue(0.03f, 0.1f, lifeRatio, true)));
                }

                // Once the music hits, release more cinders
                if (scal.ModNPC<SupremeCalamitas>().postMusicHit)
                    return 6;

                // Release a good amount of cinders while brothers or Sepulcher-1 are alive. Sepulcher-2 falls into an above case and does not execute this return.
                if (NPC.AnyNPCs(ModContent.NPCType<SupremeCataclysm>()) || NPC.AnyNPCs(ModContent.NPCType<SupremeCatastrophe>()) || NPC.AnyNPCs(ModContent.NPCType<SepulcherHead>()))
                    return 10;

                // If one brother is alive release more cinders
                if ((NPC.AnyNPCs(ModContent.NPCType<SupremeCatastrophe>()) == false && NPC.AnyNPCs(ModContent.NPCType<SupremeCataclysm>()) == true) || (NPC.AnyNPCs(ModContent.NPCType<SupremeCataclysm>()) == false && NPC.AnyNPCs(ModContent.NPCType<SupremeCatastrophe>()) == true))
                    return 7;

                // Release a moderate amount of cinders normally.
                return 18;
            }
        }
        public static float CinderSpeed
        {
            get
            {
                if (!Main.npc.IndexInRange(CalamityGlobalNPC.SCal) || Main.npc[CalamityGlobalNPC.SCal].type != ModContent.NPCType<SupremeCalamitas>())
                    return 0f;

                NPC scal = Main.npc[CalamityGlobalNPC.SCal];
                float lifeRatio = scal.life / (float)scal.lifeMax;

                // Move more and more quickly at the end of the battle until the acceptance phase, where it all slows down dramatically.
                if (lifeRatio < 0.1f)
                {
                    if (lifeRatio <= 0.01f)
                        return 4.5f;

                    return MathHelper.Lerp(10.75f, 6.7f, Utils.GetLerpValue(0.03f, 0.1f, lifeRatio, true));
                }

                // Move a little quickly while brothers or Sepulcher-1 are alive. Sepulcher-2 falls into an above case and does not execute this return.
                if (NPC.AnyNPCs(ModContent.NPCType<SupremeCataclysm>()) || NPC.AnyNPCs(ModContent.NPCType<SupremeCatastrophe>()) || NPC.AnyNPCs(ModContent.NPCType<SepulcherHead>()))
                    return 7.4f;

                if (scal.ModNPC<SupremeCalamitas>().postMusicHit)
                    return 25 * MathHelper.Clamp(Utils.GetLerpValue(-60, 0, scal.ModNPC<SupremeCalamitas>().musicSyncCounter), 0.5f, 1f);
                // Move moderately quickly usually.
                return 5.6f;
            }
        }

        public static float OverridingIntensity = 0f;

        public override void Update(GameTime gameTime)
        {
            intensity = GetIntensity();
            if (SCalIndex == -1)
                UpdateSCalIndex();

            if (!Main.npc.IndexInRange(CalamityGlobalNPC.SCal) || Main.npc[CalamityGlobalNPC.SCal].type != ModContent.NPCType<SupremeCalamitas>() || SCalIndex == -1)
            {
                intensity -= 0.1f;
                if (intensity <= 0)
                    isActive = false;
                return;
            }

            static Color selectCinderColor()
            {
                return SupremeCalamitas.CurrentColor;
            }

            // Randomly add cinders.
            if (Main.rand.NextBool(CinderReleaseChance) && CalamityGlobalNPC.SCal >= 0)
            {
                var box = Main.npc[CalamityGlobalNPC.SCal].ModNPC<SupremeCalamitas>().ArenaBox;
                int lifetime = Main.rand.Next(485, 645);
                float depth = Main.rand.NextFloat(1.8f, 5f);
                Vector2 startingPosition = Main.screenPosition + new Vector2(Main.screenWidth * Main.rand.NextFloat(-0.1f, 1.1f), Main.screenHeight * Main.rand.NextFloat(-0.1f, 1.1f));
                startingPosition = Main.rand.NextVector2FromRectangle(box.HitboxRectangle) + new Vector2(0, 180);
                startingPosition = Vector2.Lerp(box.BottomLeft, box.BottomRight, Main.rand.NextFloat());
                Vector2 startingVelocity = -Vector2.UnitY.RotatedByRandom(0.91f);
                Cinders.Add(new Cinder(lifetime, Cinders.Count, depth, selectCinderColor(), startingPosition, startingVelocity));
            }
            else if ((!Main.npc.IndexInRange(CalamityGlobalNPC.SCal) || Main.npc[CalamityGlobalNPC.SCal].type != ModContent.NPCType<SupremeCalamitas>()) == false)
            {
                NPC scal = Main.npc[CalamityGlobalNPC.SCal];
                if (scal.ModNPC<SupremeCalamitas>().musicSyncCounter == 0)
                {
                    for (int i = 0; i < 70; i++)
                    {
                        int lifetime = Main.rand.Next(285, 445);
                        float depth = Main.rand.NextFloat(1.8f, 5f);
                        Vector2 startingPosition = Main.screenPosition + new Vector2(Main.screenWidth * Main.rand.NextFloat(-0.1f, 1.1f), Main.screenHeight * Main.rand.NextFloat(1.05f, 1.45f));
                        Vector2 startingVelocity = -Vector2.UnitY.RotatedByRandom(0.91f) * Main.rand.NextFloat(0.5f, 2.5f);

                        Cinders.Add(new Cinder(lifetime, Cinders.Count, depth, SupremeCalamitas.LamentColor, startingPosition, startingVelocity));
                    }
                }
            }

            // Update all cinders.
            for (int i = 0; i < Cinders.Count; i++)
            {
                if (Cinders[i].Opacity < 1)
                    Cinders[i].Opacity += 0.1f;
                Cinders[i].Scale = Utils.GetLerpValue(Cinders[i].Lifetime, Cinders[i].Lifetime / 3, Cinders[i].Time, true) * 1.5f;
                Cinders[i].Scale *= MathHelper.Lerp(0.6f, 0.9f, Cinders[i].IdentityIndex % 6f / 6f);

                Vector2 idealVelocity = -Vector2.UnitY.RotatedBy(MathHelper.Lerp(-0.94f, 0.94f, (float)Math.Sin(Cinders[i].Time / 36f + Cinders[i].IdentityIndex) * 0.5f + 0.5f)) * CinderSpeed;
                float movementInterpolant = MathHelper.Lerp(0.01f, 0.08f, Utils.GetLerpValue(45f, 145f, Cinders[i].Time, true));
                Cinders[i].Velocity = Vector2.Lerp(Cinders[i].Velocity, idealVelocity, movementInterpolant);
                Cinders[i].Velocity = Cinders[i].Velocity.SafeNormalize(-Vector2.UnitY) * CinderSpeed;
                Cinders[i].Time++;

                Cinders[i].Center += Cinders[i].Velocity;
            }

            // Clear away all dead cinders.
            Cinders.RemoveAll(c => c.Time >= c.Lifetime);
        }

        private float GetIntensity()
        {
            if (RitualDramaProjectileIsPresent)
                return OverridingIntensity;

            OverridingIntensity = 0f;
            if (UpdateSCalIndex() && SCalIndex > -1)
            {
                float x = 0f;
                if (SCalIndex != -1)
                    x = Vector2.Distance(Main.LocalPlayer.Center, Main.npc[this.SCalIndex].Center);
                float intensityFactor = BossRushEvent.BossRushActive ? -0.2f : 1f;

                return (1f - Utils.SmoothStep(4500f, 9000f, x)) * intensityFactor;
            }

            return intensity;
        }

        public override Color OnTileColor(Color inColor) => new(Vector4.Lerp(new Vector4(0.5f, 0.8f, 1f, 1f), inColor.ToVector4(), 1f - GetIntensity()));

        private bool UpdateSCalIndex()
        {
            int SCalType = ModContent.NPCType<SupremeCalamitas>();
            if (SCalIndex >= 0 && Main.npc[SCalIndex].active && Main.npc[SCalIndex].type == SCalType)
                return true;

            SCalIndex = NPC.FindFirstNPC(SCalType);

            return SCalIndex != -1;
        }

        public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth)
        {
            if (maxDepth >= 0 && minDepth < 0)
            {
                var viewport = Main.graphics.GraphicsDevice.Viewport;
                var tex = CalamityUtils.GetTextureEfficient(ref backgroundTex, "CalamityMod/Skies/CalamitasBackground").Value;

                //We add 2 to the repeat count as a buffer for the texture position offset, to ensure the screen is always fully covered.
                float repeatX = (float)viewport.Width / tex.Width + 2;
                float repeatY = (float)viewport.Height / tex.Height + 2;

                float intensity = GetIntensity();
                //This ratio is the scale of one viewport pixel to one game pixel. Needed to accomodate for different resolutions and zoom levels.
                //Changing zoom while this is on does cause the background to move rapidly while zoom is being changed since the screen position is changing. This is more effort to fix than it's worth.
                //Changing zoom also does not change the scale of the background texture, leaving it full-size regardless of zoom. Once again, more work than it's worth.
                Vector2 ViewportToGamesizeRatio = new Vector2(viewport.Width, viewport.Height) / new Vector2(Main.screenWidth, Main.screenHeight) * Main.GameZoomTarget;

                //This is the offset the texture has from the screen position. Subtracting the player postion is needed for it to move with the player like a background shouls. Also accounts for parallax setting.
                Point posOffset = new Vector2(MathF.Sin((Main.GlobalTimeWrappedHourly) * 0.03f + MathHelper.PiOver2) * 10 * tex.Width, MathF.Cos((Main.GlobalTimeWrappedHourly) * 0.1f + MathHelper.PiOver2) * 17 * tex.Height).ToPoint() - (Main.LocalPlayer.position * ViewportToGamesizeRatio * Main.caveParallax).ToPoint();

                //Using a modulo to clamp the offset to within one texture size - the looping of the modulo looks seamless due to the texture looping
                posOffset.X = posOffset.X % tex.Width;
                posOffset.Y = posOffset.Y % tex.Height;

                Rectangle dest = new Rectangle(posOffset.X - tex.Width, posOffset.Y - tex.Height, (int)(tex.Width * repeatX), (int)(tex.Height * repeatY));

                //We need the texture to loop, so we use a linearwrap sampler state. We draw it one extra texture size in all directions around the screen to provide buffer for the posOffset.
                spriteBatch.SafeAction(() =>
                {
                    spriteBatch.TryEnd();
                    spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearWrap, DepthStencilState.None, RasterizerState.CullCounterClockwise);

                    spriteBatch.Draw(
                        tex,
                        destinationRectangle: dest,
                        sourceRectangle: new Rectangle(0, 0, (int)(tex.Width * repeatX), (int)(tex.Height * repeatY)),
                        color: Color.White * intensity
                    );
                });
            }

            // Draw cinders.

            var cinderTexture = CalamityUtils.GetTextureEfficient(ref cinderTex, "CalamityMod/ExtraTextures/SmallGreyscaleCircle").Value;
            for (int i = 0; i < Cinders.Count; i++)
            {
                float OpacityMult = 0.1f + 0.3f * ((MathF.Sin(Cinders[i].Time * 0.1f + Cinders[i].IdentityIndex) + 1) * 0.5f);
                Vector2 drawPosition = Cinders[i].Center - Main.screenPosition;
                Vector2 cinderOrigin = cinderTexture.Size() * 0.5f;
                spriteBatch.Draw(cinderTexture, drawPosition, null, Cinders[i].DrawColor * Cinders[i].Opacity * OpacityMult, 0f, cinderOrigin, Cinders[i].Scale * 0.15f, SpriteEffects.None, 0f);
            }
        }

        public override float GetCloudAlpha()
        {
            return 0f;
        }

        public override void Activate(Vector2 position, params object[] args)
        {
            isActive = true;
        }

        public override void Deactivate(params object[] args)
        {
            isActive = false;
        }

        public override void Reset()
        {
            isActive = false;
        }

        public override bool IsActive()
        {
            return isActive || intensity > 0f;
        }
    }
}
