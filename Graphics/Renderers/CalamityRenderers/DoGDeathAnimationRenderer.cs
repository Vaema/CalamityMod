using System;
using CalamityMod.Enums;
using CalamityMod.NPCs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Graphics.Shaders;
using Terraria.ID;
using Terraria.ModLoader;

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

        public override bool ShouldDraw => false;//!Main.gameMenu && CalamityDrawParameterNPC.DoGDeathAnimationTimer > 0;

        public static bool ValidToDraw(NPC npc)
        {
            return false;
            /*
            // Do not draw inactive npcs, or ones with weird MP types less than or equal to 0.
            if (!npc.active || npc.type <= NPCID.None)
                return false;

            if (!CalamityDrawParameterNPC.DrawingDoGDeathAnimation[npc.whoAmI])
                return false;

            return true;
            */
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
            
            spriteBatch.Draw(MainTarget, Vector2.Zero, Color.White);

            spriteBatch.End();
            spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, RasterizerState.CullCounterClockwise);
        }
    }
}
