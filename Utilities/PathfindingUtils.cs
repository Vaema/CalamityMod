using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria;

namespace CalamityMod;

public static partial class CalamityUtils
{
    #region Heuristic Functions

    public static float TaxicabDistance(Point a, Point b) => MathF.Abs(a.X - b.X) + MathF.Abs(a.Y - b.Y);

    public static float EuclideanDistance(Point a, Point b)
    {
        float dx = MathF.Abs(a.X - b.X);
        float dy = MathF.Abs(a.Y - b.Y);
        return MathF.Sqrt(dx * dx + dy * dy);
    }

    public static float ChebyshevDistance(Point a, Point b) => MathF.Max(MathF.Abs(a.X - b.X), MathF.Abs(a.Y - b.Y));

    public static float OctileDistance(Point a, Point b)
    {
        float dx = MathF.Abs(a.X - b.X);
        float dy = MathF.Abs(a.Y - b.Y);
        return dx + dy - 0.586f * MathF.Min(dx, dy);
    }

    #endregion

    /// <summary>
    /// A quick method to see if an enemy fits in a path on the grid.
    /// </summary>
    public static bool DoesEntityFitInPath(this Entity entity, Point point, int fluffX = 0, int fluffY = 0)
    {
        Rectangle hitbox = entity.Hitbox;
        Vector2 worldCoordinatePoint = point.ToWorldCoordinates();
        hitbox.Inflate(fluffX, fluffY);

        bool doesFit = true;
        for (int coordX = (int)(worldCoordinatePoint.X - hitbox.Width / 2); coordX < worldCoordinatePoint.X + hitbox.Width / 2; coordX++)
        {
            for (int coordY = (int)(worldCoordinatePoint.Y - hitbox.Height / 2); coordY < worldCoordinatePoint.Y + hitbox.Height / 2; coordY++)
            {
                Point p = new Vector2(coordX, coordY).ToTileCoordinates();
                if (Main.tile[p].IsTileSolid())
                {
                    doesFit = false;
                    break;
                }
            }
        }

        return doesFit;
    }

    public static List<Point> GetIntersectingHitboxPoints(this Entity entity, Point position, int fluffX = 0, int fluffY = 0)
    {
        Rectangle hitbox = entity.Hitbox;
        hitbox.Location = new Point(position.X - hitbox.Width / 2, position.Y - hitbox.Height / 2);
        hitbox.Inflate(fluffX, fluffY);

        int startX = (int)MathF.Floor(hitbox.Left / 16);
        int endX = (int)Math.Floor((hitbox.Right - float.Epsilon) / 16);
        int startY = (int)MathF.Floor(hitbox.Top / 16);
        int endY = (int)Math.Floor((hitbox.Bottom - float.Epsilon) / 16);

        List<Point> intersectingPoints = [];
        for (int i = startX; i <= endX; i++)
        {
            for (int j = startY; j <= endY; j++)
                intersectingPoints.Add(new Point(i, j));
        }

        return intersectingPoints;
    }
}
