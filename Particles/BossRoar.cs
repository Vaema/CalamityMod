using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Particles;

public class BossRoar : Particle
{
    public override string Texture => "CalamityMod/Particles/RoarPulse";
    public override bool SetLifetime => true;
    public override bool UseCustomDraw => true;

    private float OriginalScale;
    private float FinalScale;
    private float BaseOpacity;
    private float opacity;
    private Color BaseColor;

    public BossRoar(Vector2 position, Color color, float rotation, float originalScale, float finalScale, int lifeTime, float baseOpacity = 1f)
    {
        Position = position;
        BaseColor = color;
        OriginalScale = originalScale;
        FinalScale = finalScale;
        Scale = originalScale;
        Lifetime = lifeTime;
        BaseOpacity = baseOpacity;
        Rotation = rotation;
    }

    public override void Update()
    {
        Scale = MathHelper.Lerp(OriginalScale, FinalScale, LifetimeCompletion);

        opacity = 1f;
        if (LifetimeCompletion < 0.1f)
            opacity = MathHelper.Lerp(0f, 1f, LifetimeCompletion * 10);

        Color = BaseColor * opacity;
    }

    public override void CustomDraw(SpriteBatch spriteBatch)
    {
        Texture2D tex = ModContent.Request<Texture2D>(Texture).Value;
        spriteBatch.Draw(tex, Position - Main.screenPosition, null, Color * BaseOpacity, Rotation, tex.Size() / 2f, Scale, SpriteEffects.None, 0);
    }
}
