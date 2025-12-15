using CalamityMod.Enums;
using CalamityMod.NPCs.VanillaNPCAIOverrides.Bosses;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.Graphics.Shaders;

namespace CalamityMod.Graphics.Renderers.CalamityRenderers
{
    public class DeerclopsShadowRenderer : BaseRenderer
    {
        #region Fields/Properties

        public override GeneralDrawLayer Layer => GeneralDrawLayer.AfterEverything;

        public static NPC Deerclops => Main.npc[NPC.deerclopsBoss];

        // Should only draw if not in the main menu, deerclops is active or the border is fading post-death,
        // It's rev+ and the boolean for drawing the border is true.
        public override bool ShouldDraw => !Main.gameMenu && DeerclopsAI.borderScalar > 0f && DeerclopsAI.shouldDrawEnrageBorder;
        #endregion

        #region Methods
        public override void DrawToTarget(SpriteBatch spriteBatch)
        {
            bool shouldDraw;
            var deerclopsInactive = false;
            if (NPC.deerclopsBoss >= 0 && NPC.deerclopsBoss.WithinBounds(Main.npc.Length))
            {
                shouldDraw = Deerclops.HasValidTarget;
            }
            else
            {
                shouldDraw = DeerclopsAI.borderScalar > 0f;
                deerclopsInactive = true;
            }

            if (shouldDraw)
            {
                var minRadius = DeerclopsAI.innerBorder;
                var maxRadius = DeerclopsAI.outerBorder;

                // Begin drawing the shadow
                var blackTile = TextureAssets.MagicPixel;

                var shader = GameShaders.Misc["CalamityMod:DeerclopsShadowShader"].Shader;
                shader.Parameters["minRadius"].SetValue(minRadius);
                shader.Parameters["maxRadius"].SetValue(maxRadius);
                shader.Parameters["anchorPoint"].SetValue(DeerclopsAI.lastDeerclopsPosition);
                shader.Parameters["screenPosition"].SetValue(Main.screenPosition);
                shader.Parameters["screenSize"].SetValue(Main.ScreenSize.ToVector2());
                shader.Parameters["maxOpacity"].SetValue(DeerclopsAI.borderScalar);

                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer, shader, Main.Transform);

                Rectangle rekt = new(Main.screenWidth / 2, Main.screenHeight / 2, Main.screenWidth, Main.screenHeight);
                spriteBatch.Draw(blackTile.Value, rekt, null, default, 0f, blackTile.Value.Size() * 0.5f, 0, 0f);

                // Shadow drawing complete
                spriteBatch.End();
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, Main.DefaultSamplerState, DepthStencilState.None, Main.Rasterizer);
            }

            if (deerclopsInactive)
            {
                // Push the border away and fade out when deerclops is deadge
                DeerclopsAI.borderScalar = MathHelper.Clamp(DeerclopsAI.borderScalar - 0.015f, 0f, 1f);
                DeerclopsAI.innerBorder += 30f;
                DeerclopsAI.outerBorder += 30f;
            }
        }
        #endregion
    }
}
