using System.Collections.Generic;
using CalamityMod.Utilities.Daybreak;
using CalamityMod.Utilities.Daybreak.Buffers;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Light;
using Terraria.Graphics.Shaders;
using Terraria.ModLoader;

namespace CalamityMod.Graphics
{
    public class EnhancedDarknessSystem : ModSystem
    {
        public class LightSource
        {
            public Asset<Texture2D> texture;
            public float scale = 1;
            public Vector2 vectorScale = Vector2.One;
            public Vector2 center = Main.LocalPlayer.Center;
            public float rotation = 0;

            public LightSource() { }

            public LightSource(Vector2? center = null, Asset<Texture2D> texture = null, float scale = 1, float rotation = 0, Vector2? vectorScale = null)
            {
                this.texture = texture ?? ModContent.Request<Texture2D>("CalamityMod/Particles/BloomCircle");
                this.scale = scale;
                this.vectorScale = vectorScale ?? Vector2.One;
                this.center = center ?? Main.LocalPlayer.Center;
                this.rotation = rotation;
            }
        }

        public static List<LightSource> lights = new();
        public override void OnModLoad()
        {
            On_OverlayManager.Draw += DrawShadowOverlay;
            On_LightingEngine.UpdateLightDecay += AdjustTransmissiveness;
        }

        private void DrawShadowOverlay(On_OverlayManager.orig_Draw orig, OverlayManager self, SpriteBatch spriteBatch, RenderLayers layer, bool beginSpriteBatch)
        {
            orig(self, spriteBatch, layer, beginSpriteBatch);

            //This ensures that the shadows only draw
            //  - In the world
            //  - Right before UI is drawn (and right before the hideUI check), as that's where RenderLayers.All is drawn
            //  - 
            if (Main.gameMenu || layer != RenderLayers.All || Main.LocalPlayer.Calamity().darknessIntensity <= 0)
                return;
            var mp = Main.LocalPlayer.Calamity();

            var device = Main.instance.GraphicsDevice;
            using var lease = RenderTargetPool.Shared.Rent(
                device,
                Main.screenWidth / 2,
                Main.screenHeight / 2,
                RenderTargetDescriptor.Default
            );

            using (lease.Scope(clearColor: Color.Black))
            {
                Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.Default, Main.Rasterizer, null, Matrix.Identity);
                foreach (var item in lights)
                {
                    Main.spriteBatch.Draw(item.texture.Value, (item.center - Main.screenPosition) * 0.5f, null, Color.White, item.rotation, item.texture.Size() * 0.5f, item.vectorScale * item.scale * 0.5f, SpriteEffects.None, 0);
                }
                Main.spriteBatch.End();
            }

            using (Main.spriteBatch.Scope())
            {
                Main.spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, Main.Rasterizer, null, Main.GameViewMatrix.TransformationMatrix);
                var shader = GameShaders.Misc["CalamityMod:DozeLightingShader"];
                shader.UseOpacity(mp.darknessIntensity);
                shader.Apply();
                Main.spriteBatch.Draw(lease.Target, Vector2.Zero, null, Color.White, 0, Vector2.Zero, 2, 0, 0);
                spriteBatch.End();
            }
        }


        private const float VanillaWaterLightMult = 0.91f; //Vanilla water light multiplier.
        private void AdjustTransmissiveness(On_LightingEngine.orig_UpdateLightDecay orig, LightingEngine self)
        {
            orig(self);

            var mp = Main.LocalPlayer.Calamity();
            LightMap map = self._workingLightMap;
            if (mp.ZoneAbyss)
            {
                //This converts the light decay amount from the amount it normally is in water into the amount it normally is in air, depending on the intensity of the abyss darkness.
                //This is to offset the abyss darkness system to make the parts that are supposed to be visible easier to see.
                //Dividing by 0.91 brings the water back to 100% transmissiveness with the original color
                map.LightDecayThroughWater = Vector3.Lerp(map.LightDecayThroughWater, (map.LightDecayThroughWater / VanillaWaterLightMult) * 0.95f, MathHelper.Clamp(mp.darknessIntensity, 0, 1));
            }
        }

        public override void OnWorldUnload()
        {
            lights.Clear();
        }

        public override void PreUpdateEntities()
        {
            //Every frame, the light sources are determined by what was added on that frame only. Therefore, we reset the light list every frame.
            lights.Clear();
        }

    }
}
