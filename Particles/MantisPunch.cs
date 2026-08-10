using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using ReLogic.Content;
using Terraria.ModLoader;

namespace CalamityMod.Particles;

public class MantisPunch : Particle
{
    int Frame = 0;
    int FrameTimer = 0;

    public override string Texture => "CalamityMod/Particles/MantisPunch";
    public override bool UseCustomDraw => true;
    public override void CustomDraw(SpriteBatch spriteBatch)
    {
        Asset<Texture2D> tex = ModContent.Request<Texture2D>(Texture);

        Main.EntitySpriteDraw(tex.Value, Position - Main.screenPosition, tex.Frame(1, 6, 0, Frame), Lighting.GetColor((Position / 16).ToPoint()), Rotation, Origin, 1f, SpriteEffects.None);
    }
    public MantisPunch(Vector2 position, float rotation)
    {
        Position = position; 
        Rotation = rotation;

        Origin = new Vector2(32, 56);

        AffectedByLight = true;
    }
    public override void Update()
    {
        FrameTimer++;
        if (FrameTimer % 3 == 0)
        {
            Frame++;
        }
        if (Frame >= 6)
        {
            Kill();
        }
    }
}
