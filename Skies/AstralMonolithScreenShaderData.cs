using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.Graphics.Effects;
using Terraria.Graphics.Shaders;

namespace CalamityMod.Skies
{
    public class AstralMonolithScreenShaderData : ScreenShaderData
    {
        public AstralMonolithScreenShaderData(string passName)
            : base(passName)
        {
        }

        public override void Apply()
        {
            Vector3 vec = Main.ColorOfTheSkies.ToVector3();
            vec *= 0.4f;
            base.UseOpacity(Math.Max(vec.X, Math.Max(vec.Y, vec.Z)));
            base.Apply();
        }

        public override void Update(GameTime gameTime)
        {
            if (Main.LocalPlayer.Calamity().monolithAstralShader <= 0)
                Filters.Scene["CalamityMod:MonolithAstral"].Deactivate(Array.Empty<object>());
        }
    }
}
