using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.ForegroundDrawing.LoopingTextures;

public abstract class LoopingTextureForeground : ModSystem
{
    public virtual string Texture => "CalamityMod/ExtraTextures/Miscellaneous/NuclearTorrent";

    /// <summary>
    /// How much the effect has moved. Increments by Speed.
    /// </summary>
    float Progress = 0f;

    /// <summary>
    /// The speed at which Progress is incremented every frame.
    /// </summary>
    public virtual float Speed => 60f;

    /// <summary>
    /// How opaque the visual is. Caps out at IntensityMaximum.
    /// </summary>
    public float Intensity = 0f;

    /// <summary>
    /// How opaque the visual can become before capping out.
    /// </summary>
    public virtual float IntensityMaximum => 0.15f;

    /// <summary>
    /// Current parallax.
    /// </summary>
    Vector2 Parallax = Vector2.Zero;

    /// <summary>
    /// Current parallax depth.
    /// </summary>
    public virtual Vector2 ParallaxDepth => Vector2.Zero;

    /// <summary>
    /// When true, the Intensity will increase until it caps out. If false it will decrease until it hits 0.
    /// </summary>
    /// <returns>Whether or not the visual is active.</returns>
    public virtual bool DoesThisShow()
    {
        return false;
    }

    public override void PostDrawTiles()
    {
        if (DoesThisShow())
        {
            Intensity = MathHelper.Lerp(Intensity, IntensityMaximum, 0.1f);
        }
        else
        {
            Intensity = MathHelper.Lerp(Intensity, 0, 0.1f);
        }

        Intensity = MathHelper.Clamp(Intensity, 0f, 1f);

        Parallax += Main.LocalPlayer.velocity * ParallaxDepth;

        Progress += Speed;

        Main.spriteBatch.Begin();
        Draw();
        PostDraw();
        Main.spriteBatch.End();

        Update();
    }

    public override void PostUpdateEverything()
    {
        Update();
    }

    /// <summary>
    /// Extra drawing logic can be placed here.
    /// </summary>
    public virtual void PostDraw()
    {

    }

    /// <summary>
    /// Called every frame. Run behavior such as particle effects here.
    /// </summary>
    public virtual void Update()
    {

    }

    /// <summary>
    /// Override this to replace existing drawing logic. Only recommended if you know what you're doing.
    /// </summary>
    public virtual void Draw()
    {
        var tex = ModContent.Request<Texture2D>(Texture);

        for (var i = -4; i < 14; i++)
        {
            float wid = tex.Width();
            float hei = tex.Height();

            var rect = new Rectangle(0, 0, (int)wid, (int)hei);

            var rot = -Main.WindForVisuals;

            var col = new Color(Intensity, Intensity, Intensity, 0f);

            Main.EntitySpriteDraw(tex.Value, new Vector2(Parallax.X % wid, 0).RotatedBy(rot) + new Vector2(tex.Width() * i, 0).RotatedBy(rot), rect, col, rot, new Vector2(0, (hei - Progress) % hei), 1, SpriteEffects.None);
            Main.EntitySpriteDraw(tex.Value, new Vector2(Parallax.X % wid, 0).RotatedBy(rot) + new Vector2(tex.Width() * i, -hei).RotatedBy(rot), rect, col, rot, new Vector2(0, (hei - Progress) % hei), 1, SpriteEffects.None);
            Main.EntitySpriteDraw(tex.Value, new Vector2(Parallax.X % wid, 0).RotatedBy(rot) + new Vector2(tex.Width() * i, -hei * 2).RotatedBy(rot), rect, col, rot, new Vector2(0, (hei - Progress) % hei), 1, SpriteEffects.None);
        }
    }
}
