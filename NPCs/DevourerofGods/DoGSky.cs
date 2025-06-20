using System.Linq;
using CalamityMod.Events;
using CalamityMod.World;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.Graphics.Effects;
using Terraria.ModLoader;
using XPT.Core.Audio.MP3Sharp.Decoding.Decoders.LayerI;

namespace CalamityMod.NPCs.DevourerofGods
{
    public class DoGSky : CustomSky
    {
        private bool isActive = false;
        private float intensity = 0f;
        private int DoGIndex = -1;
        private Color currentColor = Color.Black;
        public static Color DoGSkyColor = Color.Black;
        public override void Update(GameTime gameTime)
        {
            if (DoGIndex == -1)
            {
                UpdateDoGIndex();
                if (DoGIndex == -1 && Main.LocalPlayer.Calamity().monolithDevourerBShader <= 0 && Main.LocalPlayer.Calamity().monolithDevourerPShader <= 0)
                    isActive = false;
            }

            if (isActive && intensity < 1f && Main.shimmerBrightenDelay <= 0)
            {
                intensity += 0.01f;
            }
            else if (!isActive && intensity > 0f)
            {
                intensity -= 0.01f;
            }
            //Main.NewText($"[c/{DoGSkyColor.Hex3()}:COLOR]");
        }

        public override Color OnTileColor(Color inColor) 
        {
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
            if (maxDepth >= 0 && minDepth < 0)
            {
                if (DoGIndex != -1)
                {
                    var DoG = Main.npc[DoGIndex].ModNPC<DevourerofGodsHead>();
                    if (DoG == null)
                    {
                        DoGIndex = -1;
                        return;
                    }
                    if (DoG.NPC.active)
                    {
                        Color goalSkyColor = Color.Black;
                        if (DoG.isInAgressiveState)
                            goalSkyColor = Color.Fuchsia;
                        if (DoG.isInPassiveState)
                            goalSkyColor = Color.Cyan;
                        if (DoG.isInLaserWallState)
                            goalSkyColor = new Color(117, 21, 161);
                        if (DoG.isInPostWallState || DoG.postTeleportTimer > 0 || DoG.teleportTimer > 0 || (DoG.NPC.localAI[2] < 180 && DoG.NPC.localAI[2] > 60))
                        {
                            if (DoG.Phase2Started)
                                goalSkyColor = Color.Black;
                            else
                                goalSkyColor = new Color(117, 21, 161);
                        }
                            currentColor = Color.Lerp(currentColor, goalSkyColor, 0.1f);
                            spriteBatch.Draw(TextureAssets.BlackTile.Value, new Rectangle(0, 0, Main.screenWidth * 2, Main.screenHeight * 2), Color.Lerp(currentColor,Color.Black, 0.5f) * intensity);
                    }
                }
                else
                {

                    Color goalSkyColor = Color.Black;
                    if (Main.LocalPlayer.Calamity().monolithDevourerPShader > 0)
                        goalSkyColor = Color.Fuchsia;
                    if (Main.LocalPlayer.Calamity().monolithDevourerBShader > 0)
                        goalSkyColor = Color.Cyan;
                    currentColor = Color.Lerp(currentColor, goalSkyColor, 0.1f);
                    spriteBatch.Draw(TextureAssets.BlackTile.Value, new Rectangle(0, 0, Main.screenWidth * 2, Main.screenHeight * 2), Color.Lerp(currentColor, Color.Black, 0.5f) * intensity);
                }
                DoGSkyColor = currentColor;
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

    public class DoGSkySunlightEnabler : ModSystem
    {
        public float FillProgress = 0;
        public override void ModifySunLightColor(ref Color tileColor, ref Color backgroundColor)
        {
            var cplayer = Main.LocalPlayer.Calamity();
            if (Main.shimmerDarken > 0.4f && (cplayer.monolithDevourerBShader > 0 || cplayer.monolithDevourerPShader > 0 || Main.npc.Any(x => x.active && x.type == ModContent.NPCType<DevourerofGodsHead>())))
            {
                FillProgress += 0.05f;
            } else
            {
                FillProgress = 0;
            }
            FillProgress = MathHelper.Clamp(FillProgress, 0, 1);
            if (FillProgress > 0)
            {
                backgroundColor = Color.Lerp(backgroundColor, DoGSky.DoGSkyColor, FillProgress);
                tileColor = Color.Lerp(tileColor, DoGSky.DoGSkyColor, FillProgress);
            }

        }
    }
}
