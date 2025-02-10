using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Terraria;
using static CalamityMod.CalamityUtils;

namespace CalamityMod
{
    /// <summary>
    /// Represents an entity that is capable of finding and following a complex path.<br/>
    /// To use this interface, supply all of the required properties, and then simply call <c>this.DoPathfinding()</c> in your entity's movement logic.
    /// </summary>
    public interface IPathFinder
    {
        /// <summary>
        /// The result of the pathfinding task that contains a set of points this entity should follow.
        /// </summary>
        public Task<List<Vector2>> Paths { get; set; }

        /// <summary>
        /// The current position of this entity. Only used to calculate movement for this entity, not pathfinding.<br/>
        /// In most cases, this should look like the following, where <c>ENTITY</c> is replace by your entity instance.
        /// <code>public Vector2 Position { get => ENTITY.center; }</code>
        /// </summary>
        public Vector2 Position { get; }

        /// <summary>
        /// The current velocity of this entity. Only used to calculate movement for this entity, not pathfinding.<br/>
        /// In most cases, this should look like the following, where <c>ENTITY</c> is replace by your entity instance.
        /// <code>public Vector2 Velocity { get => ENTITY.velocity; }</code>
        /// </summary>
        public Vector2 Velocity { get; set; }

        /// <summary>
        /// The acceleration this entity should use when moving. Only used to calculate movement for this entity, not pathfinding.
        /// </summary>
        public float Acceleration { get; }

        /// <summary>
        /// The maximum speed this entity should be able to reach while moving. Only used to calculate movement for this entity, not pathfinding.
        /// </summary>
        public float MaxSpeed { get; }

        /// <summary>
        /// Defines how this entity should move to the next point. Contains a default implementation with basic acceleration towards the next point.<br/>
        /// This method should return <see langword="true"/> when the entity has reached its target point, and <see langword="false"/> otherwise.<br/>
        /// <br/>
        /// The target point is specified by <paramref name="nextPoint"/> - usually index 0 in the <see cref="Paths"/> task result.
        /// </summary>
        /// <param name="nextPoint"></param>
        /// <returns></returns>
        public bool FollowPath(Vector2 nextPoint)
        {
            // Accelerate to the target point.
            Velocity += Vector2.Normalize(nextPoint - Position) * Acceleration;

            // Cap the speed if MaxSpeed has been surpassed.
            if (Velocity.LengthSquared() > MaxSpeed * MaxSpeed)
                Velocity = Vector2.Normalize(Velocity) * MaxSpeed;

            // If the entity is within 48 pixels of its target point, consider the point reached.
            if (Vector2.DistanceSquared(Position, nextPoint) < 48f * 48f)
                return true;

            // Otherwise, continue following.
            return false;
        }

        /// <summary>
        /// Defines how this entity should behave while there is no current path to follow.<br/>
        /// By default, decelerates and stops based on <see cref="Acceleration"/>.
        /// </summary>
        public void IdleBehavior() => Velocity -= Velocity.SafeNormalize(Vector2.Zero) * Math.Min(Velocity.Length(), Acceleration);

        /// <summary>
        /// Assigns the value of <see cref="Paths"/> to <see cref="FindPathAsync(PathfindingParams)"/> based on <see cref="Pathfinding"/>.<br/>
        /// You can override this and do <c
        /// </summary>
        public void FindPath(PathfindingParams parameters) => Paths = FindPathAsync(parameters);
    }

    public static partial class CalamityUtils
    {
        /// <summary>
        /// Causes this <see cref="IPathFinder"/> to perform path finding and follow the path result, idling when a path is not available.<br/>
        /// This method should be called every frame that you want your entity to follow it's pathfinding logic.
        /// </summary>
        /// <param name="pathfinder">The entity that should be moving.</param>
        public static void DoPathfinding(this IPathFinder pathfinder, PathfindingParams parameters)
        {
            // If the task is NOT in-progress...
            if (pathfinder.Paths == null || pathfinder.Paths.IsCompleted)
            {
                // If the task result has entries...
                if ((pathfinder.Paths?.Result?.Count ?? 0) > 0)
                {
                    // Follows the point at index 0 of the path result.
                    Vector2 nextPoint = pathfinder.Paths.Result[0];

                    // Once that point is reached, it is removed from the list of points to follow.
                    if (pathfinder.FollowPath(nextPoint))
                        pathfinder.Paths.Result.RemoveAt(0);
                }

                // If it does not have entries, that means the pathfinding task is
                // complete OR the last pathfinding attempt was invalid.
                // We need to attempt pathfinding again. Idle in the meantime.
                else
                {
                    pathfinder.FindPath(parameters);
                    pathfinder.IdleBehavior();
                }
            }

            // If the task IS in-progress, just idle while waiting for it to complete.
            else
                pathfinder.IdleBehavior();
        }

        public static void DoPathfinding(this IPathFinder pathfinder, Vector2 target) => pathfinder.DoPathfinding(new PathfindingParams(pathfinder.Position, target));

        /// <summary>
        /// Represents a node in a pathfinding algorithm, containing position, cost, and parent information.
        /// </summary>
        public class Node(Point position)
        {
            public Point Position { get; set; } = position;
            public float G { get; set; } // Cost from start to this node.
            public float H { get; set; } // Heuristic cost to goal.
            public float F => G + H;     // Total cost.
            public Node Parent { get; set; }

            public override bool Equals(object obj)
            {
                if (obj is Node other)
                    return Position == other.Position;
                return false;
            }

            public override int GetHashCode() => Position.GetHashCode();
        }

        /// <summary>
        /// Represents the parameters required for the <see cref="FindPathAsync(PathfindingParams)"/> method.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Has 2 overloads, one for inputting the start and end nodes as <see cref="Vector2"/>, and the other one as <see cref="Point"/>.
        /// </para>
        /// <para>
        /// This record contains the start and end nodes, along with functions for tile validity, distance calculation, and heuristic estimation.
        /// </para>
        /// <para>
        /// - <see cref="TileValidity"/>: A function that checks if a tile at a given point is valid for traversal. If not provided, all non-solid tiles are considered valid.<br/>
        /// - <see cref="DistanceFunction"/>: A function that calculates the distance between two points. Defaults to <see cref="OctileDistance"/>.<br/>
        /// - <see cref="Heuristic"/>: A function that estimates the cost from a given point to the end point. Defaults to <see cref="OctileDistance"/>.<br/>
        /// </para>
        /// </remarks>
        public record PathfindingParams(Node Start, Node End, Func<Point, bool> TileValidity, Func<Point, Point, float> DistanceFunction, Func<Point, Point, float> Heuristic)
        {
            private static readonly Func<Point, bool> DefaultTileValidity = point => true;
            private static readonly Func<Point, Point, float> DefaultDistanceFunction = OctileDistance;
            private static readonly Func<Point, Point, float> DefaultHeuristic = OctileDistance;

            public PathfindingParams(
                Point start,
                Point end,
                Func<Point, bool> tileValidity = null,
                Func<Point, Point, float> distanceFunction = null,
                Func<Point, Point, float> heuristic = null) : this(
                    new Node(start),
                    new Node(end),
                    tileValidity ?? DefaultTileValidity,
                    distanceFunction ?? DefaultDistanceFunction,
                    heuristic ?? DefaultHeuristic
                )
            {
            }

            public PathfindingParams(
                Vector2 start,
                Vector2 end,
                Func<Point, bool> tileValidity = null,
                Func<Point, Point, float> distanceFunction = null,
                Func<Point, Point, float> heuristic = null) : this(
                    new Node(start.ToTileCoordinates()),
                    new Node(end.ToTileCoordinates()),
                    tileValidity ?? DefaultTileValidity,
                    distanceFunction ?? DefaultDistanceFunction,
                    heuristic ?? DefaultHeuristic
                )
            {
            }
        }

        /// <summary>
        /// Asynchronously finds a path from the start to the end position using the A* pathfinding algorithm.
        /// </summary>
        /// 
        /// <param name="parameters">The parameters for the pathfinding algorithm, including start, end, and cost functions.</param>
        /// 
        /// <returns>
        /// A task that represents the asynchronous operation.<br/> The task result contains a list of <see cref="Vector2"/> positions
        /// representing the path from start to end, or <see langword="null"/> if no path is found.
        /// </returns>
        /// 
        /// <remarks>
        /// <para>
        /// This method runs the pathfinding calculations on a separate CPU thread using <see cref="Task.Run"/>.<br/>
        /// Use <see cref="Task.IsCompleted"/> to check if the pathfinding is complete, and <see cref="Task{TResult}.Result"/>
        /// to retrieve the resulting path.
        /// </para>
        /// </remarks>
        /// 
        /// <example>
        /// <code>
        /// var parameters = new PathfindingParams(start, end, TileValidity, DistanceFunction, Heuristic);
        /// var pathTask = FindPathAsync(parameters);
        /// if (pathTask.Result != null)
        ///     PathPoints = pathTask.Result;
        /// </code>
        /// </example>
        public static async Task<List<Vector2>> FindPathAsync(PathfindingParams parameters)
        {
            return await Task.Run(() =>
            {
                // If the entity isn't actually supposed to follow a path right now, null
                // can be passed as the parameters to make implementation more concise.
                if (parameters is null || Main.tile[parameters.End.Position].IsTileSolid())
                    return null;

                var openSet = new List<Node>();
                var closedSet = new HashSet<Node>();
                openSet.Add(parameters.Start);

                while (openSet.Count > 0)
                {
                    // Get the node with the lowest F value.
                    var current = openSet[0];
                    for (int i = 1; i < openSet.Count; i++)
                    {
                        if (openSet[i].F < current.F || (openSet[i].F == current.F && openSet[i].H < current.H))
                            current = openSet[i];
                    }

                    openSet.Remove(current);
                    closedSet.Add(current);

                    // Check if we've reached the goal.
                    if (current.Position == parameters.End.Position)
                        return ReconstructPath(current);

                    // Generate neighbors.
                    foreach (var neighbor in GetNeighbors(current))
                    {
                        if (closedSet.Contains(neighbor) || Main.tile[neighbor.Position].IsTileSolid() || !parameters.TileValidity.Invoke(neighbor.Position))
                            continue;

                        float tentativeG = current.G + parameters.DistanceFunction.Invoke(current.Position, neighbor.Position);

                        if (!openSet.Contains(neighbor) || tentativeG < neighbor.G)
                        {
                            neighbor.Parent = current;
                            neighbor.G = tentativeG;
                            neighbor.H = parameters.Heuristic.Invoke(neighbor.Position, parameters.End.Position);

                            if (!openSet.Contains(neighbor))
                                openSet.Add(neighbor);
                        }
                    }
                }

                return null; // No path found.
            });
        }

        private static List<Node> GetNeighbors(Node node)
        {
            var neighbors = new List<Node>(Directions.Count);

            int nodeX = node.Position.X;
            int nodeY = node.Position.Y;

            foreach (var direction in Directions)
            {
                int newX = nodeX + (int)direction.X;
                int newY = nodeY + (int)direction.Y;

                if (WorldGen.InWorld(newX, newY))
                    neighbors.Add(new Node(new Point(newX, newY)));
            }

            return neighbors;
        }

        private static List<Vector2> ReconstructPath(Node node)
        {
            var pathStack = new Stack<Vector2>();
            while (node != null)
            {
                pathStack.Push(node.Position.ToWorldCoordinates());
                node = node.Parent;
            }

            var path = new List<Vector2>(pathStack.Count);
            path.AddRange(pathStack);

            return path;
        }

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
    }
}
