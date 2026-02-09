using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalamityMod.NPCs.VanillaNPCAIOverrides.Bosses.BrainOfCthulhu;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.ID;

namespace CalamityMod.Particles;

public class BrokenTendril : Particle
{
    public override string Texture => "CalamityMod/Projectiles/InvisibleProj";

    public override bool UseCustomDraw => true;

    private int TimeLeft;
    private float Opacity = 1f;
    private Vector2 InitalScale = Vector2.One;

    public BrokenTendril(Vector2 position, Vector2 velocity, float rotation, Vector2 scale, int lifeTime)
    {
        Position = position;
        Velocity = velocity;
        Scale = 1f;
        Rotation = rotation;
        TimeLeft = lifeTime;
        InitalScale = scale - Vector2.One;
    }

    public override void Update()
    {
        if (InitalScale != Vector2.Zero)
        {
            InitalScale *= 0.98f;
            if (InitalScale.X < 0.05f)
                InitalScale.X = 0;
            if (InitalScale.Y < 0.05f)
                InitalScale.Y = 0;
        }

        Point tilePos = Position.ToTileCoordinates();
        if (!WorldGen.InWorld(tilePos.X, tilePos.Y))
        {
            Kill();
            return;
        }

        if (Main.tile[tilePos].IsTileSolid() || TileID.Sets.Platforms[Main.tile[tilePos].TileType])
        {
            Velocity.Y = 0;
            if (Velocity.X > 0.05f)
                Velocity.X *= 0.9f;
            else
                Velocity.X = 0;
        }
        else
        {
            Velocity.Y += 0.25f;
            Velocity.X *= 0.975f;
        }

        Rotation += Velocity.X * 0.025f;

        if (Velocity == Vector2.Zero)
        {
            if (TimeLeft < 30)
            {
                Opacity = TimeLeft / 30f;
                if (TimeLeft <= 0)
                    Kill();
            }
            TimeLeft--;
        }
    }

    public override void CustomDraw(SpriteBatch spriteBatch)
    {
        Texture2D tex = BrainOfCthulhuSystem.tendril.Value;
        float rot = Rotation - MathHelper.PiOver2;
        Vector2 center = Position + rot.ToRotationVector2() * tex.Size().Y * InitalScale.Y * 0.5f;
        spriteBatch.Draw(tex, Position - Main.screenPosition, null, Lighting.GetColor(center.ToTileCoordinates()) * Opacity, rot, tex.Size() * Vector2.UnitX * 0.5f, Vector2.One + InitalScale, SpriteEffects.None, 0f);
    }
}

