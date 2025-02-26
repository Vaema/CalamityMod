using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CalamityMod.Projectiles.Typeless;
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
        /// Represents the working task for calculating this entity's complex path.<br/>
        /// <br/>
        /// If you want to manually access the result of this task, make sure to check <see cref="PathfindingTask.Ready"/> before accessing the value of <see cref="PathfindingTask.Result"/>.<br/>
        /// <br/>
        /// Accessing <see cref="PathfindingTask.Result"/> before execution has completed will result in the calling thread being blocked until execution is complete.
        /// </summary>
        public PathfindingTask Path { get; set; }

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
            Velocity += (nextPoint - Position).SafeNormalize(Vector2.Zero) * Acceleration;

            // Cap the speed if MaxSpeed has been surpassed.
            if (Velocity.LengthSquared() > MaxSpeed * MaxSpeed)
                Velocity = Velocity.SafeNormalize(Vector2.UnitY) * MaxSpeed;

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
        public void IdleBehavior() => Velocity *= 0.95f;

        /// <summary>
        /// Assigns the value of <see cref="Paths"/> to <paramref name="pathfindingTask"/> and executes the pathfinding calculations on a background thread.<br/>
        /// You can override this to do your own modifications to the pathfinding task before assigning and starting the calculations.
        /// </summary>
        public void FindPath(PathfindingTask pathfindingTask)
        {
            if (pathfindingTask == null)
                return;
            
            Path = pathfindingTask;
            Path.Run();
        }
    }

    /// <summary>
    /// Represents a task for calculating a complex path.<br/>
    /// Allows for synchronous access to an asynchronous execution state, while still providing blocking access where needed.
    /// </summary>
    public class PathfindingTask(
        Vector2 start,
        Vector2 end,
        Func<Point, bool> tileValidity = null,
        Func<Point, Point, float> distanceFunction = null,
        Func<Point, Point, float> heuristic = null)
    {
        public PathfindingTask(Vector2 start, Vector2 end) : this(start, end, null, null, null)
        { }

        /// <summary>
        /// The result of this pathfinding task.
        /// </summary>
        public List<Vector2> Result { get => _task.Task.Result; }

        /// <summary>
        /// Whether this pathfinding task has completed running.
        /// </summary>
        public bool Ready { get; private set; } = false;

        /// <summary>
        /// Whether this pathfinding task is still currently running.
        /// </summary>
        public bool Running { get; private set; } = false;

        /// <summary>
        /// The starting position of this pathfinding task.
        /// </summary>
        public Vector2 StartPosition { get; init; } = start;

        /// <summary>
        /// The ending position of this pathfinding task.
        /// </summary>
        public Vector2 EndPosition { get; init; } = end;

        // Interal task representation of the work.
        private readonly TaskCompletionSource<List<Vector2>> _task = new();

        // Internal containing object for the actual work.
        private readonly PathfindingWork _work = new(start, end, tileValidity, distanceFunction, heuristic);

        /// <summary>
        /// Begins execution of this pathfinding task on a background thread.
        /// </summary>
        public void Run()
        {
            Running = true;

            ThreadPool.QueueUserWorkItem((pathfindingTask) =>
            {
                var task = pathfindingTask as PathfindingTask;
                task._task.TrySetResult(task._work.CalculatePath());

                task.Ready = true;
                task.Running = false;
            },
            this);
        }
    }

    public static partial class CalamityUtils
    {
        /// <summary>
        /// Causes this <see cref="IPathFinder"/> to perform path finding and follow the path result, idling when a path is not available.<br/>
        /// This method should be called every frame that you want your entity to follow it's pathfinding logic.
        /// </summary>
        public static void DoPathfinding(this IPathFinder pathfinder, Vector2 target) => pathfinder.DoPathfinding(new PathfindingTask(pathfinder.Position, target));

        /// <summary>
        /// Causes this <see cref="IPathFinder"/> to perform path finding and follow the path result, idling when a path is not available.<br/>
        /// This method should be called every frame that you want your entity to follow it's pathfinding logic.
        /// </summary>
        public static void DoPathfinding(this IPathFinder pathfinder, PathfindingTask task, bool continuouslyUpdatePath = false)
        {
            // If the task has been started but is not finished yet, just idle.
            if (pathfinder.Path?.Running ?? false)
                pathfinder.IdleBehavior();

            // If the task has not been started,
            // or was previously unable to find a path,
            // OR is now being told to follow a new path...
            //
            // start the task (potentially again).
            else if (pathfinder.Path == null || // Not started
                pathfinder.Path.Result == null || // Unable to find previous path
                pathfinder.Path.Result.Count == 0 || // Previous path is done being followed
                (continuouslyUpdatePath && pathfinder.Path.EndPosition != task.EndPosition)) // New path
            {
                pathfinder.FindPath(task);
                pathfinder.IdleBehavior();
            }

            // Otherwise, the task has been started, completed, and found a valid path.
            // Follow the found path.
            else
            {
                // Follows the point at index 0 of the path result.
                Vector2 nextPoint = pathfinder.Path.Result[0];

                // Once that point is reached, it is removed from the list of points to follow.
                if (pathfinder.FollowPath(nextPoint))
                    pathfinder.Path.Result.RemoveAt(0);
            }
        }

        /// <summary>
        /// Represents the working computation for a <see cref="PathfindingTask"/>.
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
        public record PathfindingWork(Node Start, Node End, Func<Point, bool> TileValidity, Func<Point, Point, float> DistanceFunction, Func<Point, Point, float> Heuristic)
        {
            private static readonly Func<Point, bool> DefaultTileValidity = point => true;
            private static readonly Func<Point, Point, float> DefaultDistanceFunction = OctileDistance;
            private static readonly Func<Point, Point, float> DefaultHeuristic = OctileDistance;

            public PathfindingWork(
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

            public PathfindingWork(
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

            /// <summary>
            /// Calculates a path from the start to the end position using the A* pathfinding algorithm.
            /// </summary>
            /// 
            /// <returns>
            /// A list of <see cref="Vector2"/> positions representing the path from start to end, or <see langword="null"/> if no path is found.
            /// </returns>
            public List<Vector2> CalculatePath()
            {
                if (Main.tile[End.Position].IsTileSolid())
                    return null;

                var openSet = new List<Node>();
                var closedSet = new HashSet<Node>();
                openSet.Add(Start);

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
                    if (current.Position == End.Position)
                        return current.ReconstructPath();

                    // Generate neighbors.
                    foreach (var neighbor in current.GetNeighbors())
                    {
                        if (closedSet.Contains(neighbor) || Main.tile[neighbor.Position].IsTileSolid() || !TileValidity.Invoke(neighbor.Position))
                            continue;

                        float tentativeG = current.G + DistanceFunction.Invoke(current.Position, neighbor.Position);

                        if (!openSet.Contains(neighbor) || tentativeG < neighbor.G)
                        {
                            neighbor.Parent = current;
                            neighbor.G = tentativeG;
                            neighbor.H = Heuristic.Invoke(neighbor.Position, End.Position);

                            if (!openSet.Contains(neighbor))
                                openSet.Add(neighbor);
                        }
                    }
                }

                return null; // No path found.
            }
        }

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

            public List<Node> GetNeighbors()
            {
                var neighbors = new List<Node>(Directions.Count);

                int nodeX = Position.X;
                int nodeY = Position.Y;

                foreach (var direction in Directions)
                {
                    int newX = nodeX + (int)direction.X;
                    int newY = nodeY + (int)direction.Y;

                    if (WorldGen.InWorld(newX, newY))
                        neighbors.Add(new Node(new Point(newX, newY)));
                }

                return neighbors;
            }

            public List<Vector2> ReconstructPath()
            {
                var pathStack = new Stack<Vector2>();
                var node = this;

                while (node != null)
                {
                    pathStack.Push(node.Position.ToWorldCoordinates());
                    node = node.Parent;
                }

                var path = new List<Vector2>(pathStack.Count);
                path.AddRange(pathStack);

                return path;
            }
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
}
