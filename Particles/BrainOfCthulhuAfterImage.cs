using System.Collections.Generic;
using CalamityMod.DataStructures;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;

namespace CalamityMod.Particles;

public class BrainOfCthulhuAfterImage : Particle
{
    public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

    public override bool UseCustomDraw => false;

    public override bool SetLifetime => true;

    public override bool Important => true;

    private float StartFade;
    private float Opacity = 1f;
    List<Vector2> Path;
    private Vector2 MyScale = Vector2.One;
    private Rectangle Frame;

    public BrainOfCthulhuAfterImage(BezierCurve path, float rotation, Vector2 scale, int lifeTime, Rectangle frame, float startFadeRatio = 0f)
    {
        Path = path.GetPoints(lifeTime);
        Position = Path[0];
        Rotation = rotation;
        StartFade = startFadeRatio;
        MyScale = scale;
        Frame = frame;
        Lifetime = lifeTime + 1;
    }

    public override void Update()
    {
        float timeRatio = Time / (float)Lifetime;
        Opacity = timeRatio;
        if (StartFade != 0f)
            Opacity = Utils.GetLerpValue(StartFade, 1f, timeRatio, true);

        Opacity = 1 - CalamityUtils.SineInEasing(Opacity, 1);

        List<Vector2> pathPosition = Path;

        if (Time >= pathPosition.Count)
        {
            Kill();
            return;
        }

        Position = pathPosition[Time];
    }

    public override void CustomDraw(SpriteBatch spriteBatch)
    {
        float timeRatio = Time / (float)Lifetime;
        spriteBatch.Draw(TextureAssets.Npc[NPCID.BrainofCthulhu].Value, Position - Main.screenPosition, Frame, Lighting.GetColor(Position.ToTileCoordinates()) * Opacity * 0.5f, Rotation, Frame.Size() * 0.5f, MyScale, 0, 0);
    }
}

