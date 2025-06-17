using CalamityMod.Events;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;

namespace CalamityMod.NPCs.DevourerofGods
{
    public class DoGSky : CustomSky
    {
        private bool isActive = false;
        private float intensity = 0f;
        private int DoGIndex = -1;
        private Color currentColor = Color.Black;
        public override void Update(GameTime gameTime)
        {
            if (DoGIndex == -1)
            {
                UpdateDoGIndex();
                if (DoGIndex == -1 && Main.LocalPlayer.Calamity().monolithDevourerBShader <= 0 && Main.LocalPlayer.Calamity().monolithDevourerPShader <= 0)
                    isActive = false;
            }

            if (isActive && intensity < 1f)
            {
                intensity += 0.01f;
            }
            else if (!isActive && intensity > 0f)
            {
                intensity -= 0.01f;
            }
        }

        private float GetIntensity()
        {
            if (UpdateDoGIndex())
            {
                float x = 0f;
                if (DoGIndex != -1)
                    x = Vector2.Distance(Main.LocalPlayer.Center, Main.npc[DoGIndex].Center);

                float intensityScalar = 0.5f;
                return (1f - Utils.SmoothStep(3000f, 6000f, x)) * intensityScalar;
            }
            return 0f;
        }

        public override Color OnTileColor(Color inColor)
        {
            float intensity = GetIntensity();
            return Color.Lerp(inColor, currentColor, intensity);
        }

        private bool UpdateDoGIndex()
        {
            int DoGType = ModContent.NPCType<DevourerofGodsHead>();
            if (DoGIndex >= 0 && Main.npc[DoGIndex].active && Main.npc[DoGIndex].type == DoGType)
            {
                return true;
            }
            DoGIndex = -1;
            foreach (NPC n in Main.ActiveNPCs)
            {
                if (n.type == DoGType)
                {
                    DoGIndex = n.whoAmI;
                    break;
                }
            }
            return DoGIndex != -1;
        }

        public override void Draw(SpriteBatch spriteBatch, float minDepth, float maxDepth)
        {
            // (new Color(48, 9, 66), new Color(26, 4, 31)
            if (maxDepth >= 0 && minDepth < 0)
            {
                if (DoGIndex != -1)
                {
                    var DoG = Main.npc[DoGIndex].ModNPC<DevourerofGodsHead>();
                    if (DoG.NPC.active)
                    {
                        float intensity = GetIntensity();
                        float lifeRatio = Main.npc[DoGIndex].life / (float)Main.npc[DoGIndex].lifeMax;
                        double blackScreenLife_GateValue = (lifeRatio < 0.6f && Main.npc[DoGIndex].localAI[2] <= 2f) ? 0.15 : 0.75;

                        float timeToReachNextColor = DevourerofGodsHead.SkyColorTransitionTime;
                        float phaseTimer = Main.npc[DoGIndex].Calamity().newAI[2];
                        float colorChangeProgress = phaseTimer / timeToReachNextColor;
                        if (colorChangeProgress > 1f)
                            colorChangeProgress = 1f;


                        Color goalSkyColor = Color.Black;
                        if (DoG.isInAgressiveState)
                            goalSkyColor = Color.Fuchsia;
                        if (DoG.isInPassiveState)
                            goalSkyColor = Color.Cyan;
                        if (DoG.isInLaserWallState)
                            goalSkyColor = new Color(48, 9, 66);
                        if (DoG.isInPostWallState)
                            goalSkyColor = new Color(26, 4, 31);
                            currentColor = Color.Lerp(currentColor, goalSkyColor, 0.1f);
                            spriteBatch.Draw(TextureAssets.BlackTile.Value, new Rectangle(0, 0, Main.screenWidth * 2, Main.screenHeight * 2), Color.Lerp(currentColor,Color.Black, 0.5f) * intensity);
                    }
                }
                else
                {
                    if (Main.LocalPlayer.Calamity().monolithDevourerBShader > 0)
                    {
                        float intensity = MathHelper.Min(MathHelper.Lerp(0, 0.5f, (float)Main.LocalPlayer.Calamity().monolithDevourerBShader / 15), 0.45f);
                        Color regularSkyColor = Color.Cyan;
                        spriteBatch.Draw(TextureAssets.BlackTile.Value, new Rectangle(0, 0, Main.screenWidth * 2, Main.screenHeight * 2), regularSkyColor * intensity);
                    }
                    if (Main.LocalPlayer.Calamity().monolithDevourerPShader > 0)
                    {
                        float intensity = MathHelper.Min(MathHelper.Lerp(0, 0.5f, (float)Main.LocalPlayer.Calamity().monolithDevourerPShader / 15), 0.45f);
                        Color regularSkyColor = Color.Fuchsia;
                        spriteBatch.Draw(TextureAssets.BlackTile.Value, new Rectangle(0, 0, Main.screenWidth * 2, Main.screenHeight * 2), regularSkyColor * intensity);
                    }
                }
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
