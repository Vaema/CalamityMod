using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace CalamityMod.Pathfinding.Movements
{
    public abstract class Movement
    {
        public abstract float Cost { get; }

        public abstract bool FollowPath(IPathfinder pathfinder, Vector2 nextPoint);

        public abstract IEnumerable<Point> GetNeighborPositions(Point current);
    }
}
