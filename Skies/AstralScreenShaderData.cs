using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;

namespace CalamityMod.Skies
{
    public class AstralScreenShaderData : ScreenShaderData
    {
        public AstralScreenShaderData(Ref<Effect> shader, string passName) : base(shader, passName) { }

        public override void Apply()
        {
            Vector3 vec = Main.ColorOfTheSkies.ToVector3();
            vec *= 0.4f;
            base.UseOpacity(Math.Max(vec.X, Math.Max(vec.Y, vec.Z)));
            base.Apply();
        }

        public override void Update(GameTime gameTime)
        {
            if ((!Main.LocalPlayer.Calamity().ZoneAstral && Main.LocalPlayer.Calamity().monolithAstralShader <= 0) || Main.gameMenu)
                Filters.Scene["CalamityMod:Astral"].Deactivate(Array.Empty<object>());
        }
    }
}
