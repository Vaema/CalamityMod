using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria.ModLoader;

namespace CalamityMod.Pathfinding.Movements
{
    public abstract class BaseEntityMovement<TEntity>(TEntity pathfinder) : IMovement where TEntity : ModType, IPathfinder
    {
        public TEntity Pathfinder { get; } = pathfinder;
        public abstract string RegistrationName { get; }
        public abstract Func<Point, Point, float> Cost { get; }

        public abstract void Start();
        public abstract bool FollowPath(Vector2 nextPoint);
        public abstract IEnumerable<Point> GetDestinations(Point current);
    }
}
