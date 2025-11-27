using System.Collections.Generic;
using System.Reflection;
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
        ManagedRenderTarget rt = null;

        public static List<LightSource> lights = new();
        public override void OnModLoad()
        {
            On_OverlayManager.Draw += DrawShadowOverlay;
            On_LightingEngine.UpdateLightDecay += AdjustTransmissiveness;
            RenderTargetManager.RenderTargetUpdateLoopEvent += PrepareTargets;
            rt = new ManagedRenderTarget(true, ManagedRenderTarget.CreateScreenSizedTarget);
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

            var gd = Main.graphics.GraphicsDevice;
            Main.spriteBatch.SafeBegin(SpriteSortMode.Immediate, BatchSetting.AlphaBlend, null, Main.GameViewMatrix.TransformationMatrix, () =>
            {
                Main.spriteBatch.EnterShaderRegion();
                var shader = GameShaders.Misc["CalamityMod:DozeLightingShader"];
                var abyssDarkness = Utils.Remap(Main.LocalPlayer.Center.Y, (float)Main.rockLayer * 16f, Main.UnderworldLayer * 16f, 0, 3, true);
                shader.UseOpacity(mp.darknessIntensity);
                shader.Apply();
                Main.spriteBatch.Draw(rt, Main.screenLastPosition - Main.screenPosition, Color.White);
                Main.spriteBatch.ExitShaderRegion();
            });
        }


        private void AdjustTransmissiveness(On_LightingEngine.orig_UpdateLightDecay orig, LightingEngine self)
        {
            orig(self);
            FieldInfo field = typeof(LightingEngine).GetField("_workingLightMap", BindingFlags.Instance | BindingFlags.NonPublic);

            if (field != null)
            {
                var mp = Main.LocalPlayer.Calamity();
                var map = (LightMap)field.GetValue(self);
                if (mp.ZoneAbyss)
                {
                    //This converts the light decay amount from the amount it normally is in water into the amount it normally is in air, depending on the intensity of the abyss darkness.
                    //This is to offset the abyss darkness system to make the parts that are supposed to be visible easier to see.
                    //Dividing by 0.91 brings the water back to 100% transmissiveness with the original color
                    map.LightDecayThroughWater = Vector3.Lerp(map.LightDecayThroughWater, (map.LightDecayThroughWater / 0.91f) * 0.95f, MathHelper.Clamp(mp.darknessIntensity, 0, 1));
                }
            }
        }

        private void PrepareTargets()
        {
            //This system works by drawing all the light sources additively onto a plain black RenderTarget.
            //The shader then uses this as an opacity mask, where black = opaque and white = transparent.
            var gd = Main.instance.GraphicsDevice;
            gd.SetRenderTarget(rt);
            gd.Clear(Color.Black);
            Main.spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.Default, Main.Rasterizer, null, Matrix.Identity);
            foreach (var item in lights)
            {
                Main.spriteBatch.Draw(item.texture.Value, item.center - Main.screenPosition, null, Color.White, item.rotation, item.texture.Size() * 0.5f, item.vectorScale * item.scale, SpriteEffects.None, 0);
            }
            Main.spriteBatch.End();
            gd.SetRenderTarget(null);
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
