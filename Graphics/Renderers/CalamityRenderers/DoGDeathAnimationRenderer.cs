using CalamityMod.Enums;
using CalamityMod.NPCs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;

namespace CalamityMod.Graphics.Renderers.CalamityRenderers
{
    public class DoGDeathAnimationRenderer : BaseRenderer
    {
        /// <summary>
        /// Used to determine whether the npc should return true in predraw or not, for exclusively drawing to the drawer target.
        /// </summary>
        public static bool ActuallyDoPreDraw
        {
            get;
            private set;
        }

        public override GeneralDrawLayer Layer => GeneralDrawLayer.AfterNPCs;

        public override bool ShouldDraw => !Main.gameMenu && CalamityDrawParameterNPC.DoGDeathAnimationTimer > 0;

        public static bool ValidToDraw(NPC npc)
        {
            // Do not draw inactive npcs, or ones with weird MP types less than or equal to 0.
            if (!npc.active || npc.type <= NPCID.None)
                return false;

            if (!CalamityDrawParameterNPC.DrawingDoGDeathAnimation[npc.whoAmI])
                return false;

            return true;
        }

        public override void DrawToTarget(SpriteBatch spriteBatch)
        {
            // Indicate that DoG and his segments should draw.
            ActuallyDoPreDraw = true;

            // Draw every npc to a single target that should have the disintegration visual.
            foreach (NPC npc in Main.ActiveNPCs)
            {
                // Extra check to ensure that index errors will not occur. If not in range, something has gone wrong and the loop should terminate.
                if (!Main.npc.IndexInRange(npc.whoAmI))
                    break;

                // Draw the NPCs to the target once everything is valid and set the disintegration progress variable.
                // Also ensure to indicate that the renderer should allow drawing the target here as well.
                if (ValidToDraw(npc))
                    Main.instance.DrawNPC(npc.whoAmI, npc.behindTiles);
            }

            // Indicate that DoG and his segments should no longer draw.
            ActuallyDoPreDraw = false;
        }

        public override void DrawTarget(SpriteBatch spriteBatch)
        {
            var disintegrationShader = GameShaders.Misc["CalamityMod:DoGDisintegration"].Shader;
            Vector2 screenSize = new(Main.screenWidth, Main.screenHeight);
            Vector2 worldPosition = Main.screenPosition / MainTarget.Size;
            float disintegrationProgress = CalamityDrawParameterNPC.DoGDeathAnimationTimer / 600f;

            disintegrationShader.Parameters["disintegrationProgress"].SetValue(disintegrationProgress);
            disintegrationShader.Parameters["disintegrationScale"].SetValue(1.75f);
            disintegrationShader.Parameters["worldPosition"].SetValue(new Vector2(worldPosition.X, worldPosition.Y));
            disintegrationShader.Parameters["pixelSize"].SetValue(screenSize * 0.5f);

            Main.instance.GraphicsDevice.Textures[1] = Main.Assets.Request<Texture2D>("Images/Misc/Perlin").Value;
            Main.instance.GraphicsDevice.SamplerStates[1] = SamplerState.LinearWrap;

            disintegrationShader.Techniques[0].Passes[0].Apply();
            spriteBatch.Draw(MainTarget, Vector2.Zero, Color.White);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer);
        }
    }
}
