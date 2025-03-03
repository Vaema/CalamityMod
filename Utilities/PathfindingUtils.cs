using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Terraria;
using static CalamityMod.CalamityUtils;

namespace CalamityMod
{
    #region Pathfinding Parameters
    public readonly struct PathfindingParameters
    {
        internal static readonly Func<Point, bool> DefaultTileValidity = point => true;
        internal static readonly Func<Point, Point, float> DefaultDistanceFunction = OctileDistance;
        internal static readonly Func<Point, Point, float> DefaultHeuristic = OctileDistance;

        internal readonly Vector2 Start;
        internal readonly Vector2 End;
        internal readonly Func<Point, bool> TileValidity;
        internal readonly Func<Point, Point, float> DistanceFunction;
        internal readonly Func<Point, Point, float> Heuristic;

        public PathfindingParameters(
            Vector2 start,
            Vector2 end,
            Func<Point, bool> tileValidity = null,
            Func<Point, Point, float> distanceFunction = null,
            Func<Point, Point, float> heuristic = null)
        {
            Start = start;
            End = end;
            TileValidity = tileValidity;
            DistanceFunction = distanceFunction;
            Heuristic = heuristic;
        }
    }
    #endregion

    #region Pathfinding Manager
    /// <summary>
    /// Creates a new PathfindingManager for the selected entity.<br />
    /// Entities should only ever need one PathfindingManager for their lifetime, but more can be created.
    /// </summary>
    /// <param name="e">The entity to pathfind for.</param>
    public class PathfindingManager(Entity e)
    {
        public List<Vector2> Path { get => lastSuccessfulTask?.Result ?? []; }
        
        /// <summary>
        /// The entity which this PathfindingManager manages. This cannot be changed after creation.
        /// </summary>
        internal readonly Entity entity = e;

        /// <summary>
        /// The acceleration this PathfindingManager will impart to its Entity when making it follow a found path.<br />
        /// This has no impact on the Entity's other behaviors, AI, etc.
        /// </summary>
        internal float Acceleration { get; set; } = 0.2f;

        /// <summary>
        /// The maximum speed this PathfindingManager allow its Entity to move at when making it follow a found path.<br />
        /// This has no impact on the Entity's other behaviors, AI, etc.
        /// </summary>
        internal float MaxSpeed { get; set; } = 4f;

        /// <summary>
        /// The minimum distance this PathdingManager requires its Entity to reach from its target point before the point is marked as "reached".<br />
        /// This has no impact on the Entity's other behaviors, AI, etc.
        /// </summary>
        internal float MinimumPointDistance { get; set; } = 48f;

        /// <summary>
        /// The last completed pathfinding task that this manager has performed.<br />
        /// This is kept in place so that the entity may continue to follow its last successful path while calculating a new one.<br />
        /// <br />
        /// This task will always be in a Completed state with a valid Result.
        /// </summary>
        private PathfindingTask lastSuccessfulTask;

        /// <summary>
        /// Represents the working task for calculating this entity's complex path.<br/>
        /// <br/>
        /// Accessing <see cref="PathfindingTask.Result"/> before execution has completed will result in the calling thread being blocked until execution is complete.
        /// </summary>
        private PathfindingTask currentTask;

        /// <summary>
        /// You may override the path-following behavior imposed on the Entity by the PathfindingManager by providing a Func here.
        /// <br /><br />
        /// This Func should return <see langword="true"/> when the entity has reached its target point, and <see langword="false"/> otherwise.<br/>
        /// </summary>
        public Func<Vector2, bool> CustomFollowPath { get; set; }

        /// <summary>
        /// You may override the idling behavior imposed on the Entity by the PathfindingManager by providing an Action here.
        /// <br /><br />
        /// This Action has no parameters and no return value, and can do basically anything you want.
        /// <br />
        /// It will usually only run for a frame or two while the PathfindingManager is finding the next path to take.
        /// </summary>
        public Action CustomIdleBehavior { get; set; }

        /// <summary>
        /// Finds a path based on the specified parameters and assigns it to this manager's entity.<br/>
        /// This method does not force the entity to begin following the path - it only locates a new path.
        /// </summary>
        public void FindPath(PathfindingParameters parameters, bool forceNewTask = true)
        {
            var potentialNewTask = new PathfindingTask(parameters);

            // Case 1: No current pathfinding task.
            // Set the current task to be the potential new task.
            if (currentTask is null)
            {
                currentTask = potentialNewTask;
                currentTask.Run();
            }

            // Case 2: Current task exists and is currently running.
            // Make the Entity idle and continue to await results.
            if (currentTask.Running)
            {
                // Case 2A: We don't care about the previous pathfinding task. We have changed our goals.
                // In order to trigger this behavior, you must set forceNewTask to true, and provide a different end goal position.
                bool sameEndPosition = parameters.End.ToTileCoordinates() == currentTask.work.End.Position;
                bool forceNewPathAttempt = forceNewTask && !sameEndPosition;

                if (forceNewPathAttempt)
                {
                    currentTask = potentialNewTask;
                    currentTask.Run();
                }

                // Regardles of whether a new pathing execution is forced, the entity will continue to behave based on any previous successful pathing.
                // Its calculations have not finished.
                return;
            }

            // Case 3: Current task exists, but is not currently running.
            // There are two reasons why this can be the case.
            bool previousTaskHadNoResult = currentTask.Result is null;
            bool previousTaskPathFollowed = (currentTask.Result?.Count ?? -1) == 0;
            // The third case is that you are forcing it to start a new task, either to the same or a new destination.

            // Regardless of which reason the pathfinding has ended, start the new task.
            if (previousTaskHadNoResult || previousTaskPathFollowed || forceNewTask)
            {
                currentTask = potentialNewTask;
                currentTask.Run();
                return;
            }

            // Case 4: "Your paths found."
            // The pathfinding task has completed its execution and a valid path has been found.
            // Lock in that successful pathfinding task so the entity may consume its results.
            lastSuccessfulTask = currentTask;
        }

        /// <summary>
        /// Glue code to cause the Entity to either follow its last successful path, or idle.
        /// </summary>
        public void PathfindingBehavior()
        {
            if (lastSuccessfulTask is null)
            {
                IdleBehavior();
                return;
            }

            Vector2 nextPoint = lastSuccessfulTask.Result[0];

            // If that point is reached, it is removed from the list of points to follow.
            bool nextPointReached = FollowPath(nextPoint);
            if (nextPointReached)
                lastSuccessfulTask.Result.RemoveAt(0);

            // If the previous successful found path has been followed to its endpoint, delete the task.
            if (lastSuccessfulTask.Result.Count == 0)
                lastSuccessfulTask = null;
        }

        /// <summary>
        /// Performs appropriate "path-finding behavior" for the Entity this PathfindingManager is attached to.<br />
        /// This method is intended to be called every frame that you want the Entity to obey its pathfinding logic.
        /// </summary>
        public void DoPathfinding(PathfindingParameters parameters, bool forceNewTask = false)
        {
            FindPath(parameters, forceNewTask);
            PathfindingBehavior();
        }

        private bool DefaultFollowPath(Vector2 nextPoint)
        {
            // Accelerate to the target point.
            entity.velocity += (nextPoint - entity.Center).SafeNormalize(Vector2.Zero) * Acceleration;

            // Cap the speed if MaxSpeed has been surpassed.
            if (entity.velocity.LengthSquared() > MaxSpeed * MaxSpeed)
                entity.velocity = entity.velocity.SafeNormalize(Vector2.UnitY) * MaxSpeed;

            // If the entity is within 48 pixels of its target point, consider the point reached.
            if (Vector2.DistanceSquared(entity.Center, nextPoint) < MinimumPointDistance * MinimumPointDistance)
                return true;

            // Otherwise, continue following.
            return false;
        }

        /// <summary>
        /// Defines how this entity should move to the next point. Contains a default implementation with basic acceleration towards the next point.<br/>
        /// This method should return <see langword="true"/> when the entity has reached its target point, and <see langword="false"/> otherwise.<br/>
        /// <br/>
        /// The target point is specified by <paramref name="nextPoint"/> - usually index 0 in the <see cref="Paths"/> task result.
        /// <br /><br />
        /// The behavior of this function can be completely overridden by providing a CustomFollowPath Func.
        /// </summary>
        /// <param name="nextPoint">The next point in the path that this PathfindingManager has made.</param>
        /// <returns>Whether or not the destination has been reached.</returns>
        public bool FollowPath(Vector2 nextPoint) => CustomFollowPath?.Invoke(nextPoint) ?? DefaultFollowPath(nextPoint);

        private static readonly float DefaultSlowdown = 0.95f;
        private void DefaultIdleBehavior() => entity.velocity *= DefaultSlowdown;

        /// <summary>
        /// Defines how this entity should behave while there is no current path to follow.<br/>
        /// By default, decelerates to a stop using the DefaultSlowdown value.<br />
        /// This behavior will usually only run for a frame or two while the PathfindingManager is finding the next path to take.
        /// </summary>
        public void IdleBehavior()
        {
            if (CustomIdleBehavior is not null)
                CustomIdleBehavior();
            else
                DefaultIdleBehavior();
        }

        public void ClearResults()
        {
            currentTask = null;
            lastSuccessfulTask = null;
        }

        #region Pathfinding Task
        /// <summary>
        /// Represents a task for calculating a complex path.<br/>
        /// Allows for synchronous access to an asynchronous execution state, while still providing blocking access where needed.
        /// </summary>
        private class PathfindingTask(PathfindingParameters parameters)
        {
            /// <summary>
            /// The result of this pathfinding task.
            /// </summary>
            internal List<Vector2> Result { get => _task.Task.Result; }

            /// <summary>
            /// Whether this pathfinding task has completed running.
            /// </summary>
            internal bool Ready { get; private set; } = false;

            /// <summary>
            /// Whether this pathfinding task is still currently running.
            /// </summary>
            internal bool Running { get; private set; } = false;

            // Interal task representation of the work.
            internal readonly TaskCompletionSource<List<Vector2>> _task = new();

            // Internal containing object for the actual work.
            internal readonly PathfindingWork work = new(parameters);

            /// <summary>
            /// Begins execution of this pathfinding task on a background thread.
            /// </summary>
            internal void Run()
            {
                Running = true;

                ThreadPool.QueueUserWorkItem((pathfindingTask) =>
                {
                    var task = pathfindingTask as PathfindingTask;
                    task._task.TrySetResult(task.work.CalculatePath());

                    task.Ready = true;
                    task.Running = false;
                },
                this);
            }
        }
        #endregion

        #region Pathfinding Work Record
        /// <summary>
        /// Represents the working computation for a <see cref="PathfindingTask"/>.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Has 2 constructors. One option provides the start and end positions as <see cref="Vector2"/>, and the other one as <see cref="Point"/>.
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
        private record PathfindingWork(PathfindingNode Start, PathfindingNode End, Func<Point, bool> TileValidity, Func<Point, Point, float> DistanceFunction, Func<Point, Point, float> Heuristic)
        {
            public PathfindingWork(
                Point start,
                Point end,
                float minimalPointDistance,
                Func<Point, bool> tileValidity = null,
                Func<Point, Point, float> distanceFunction = null,
                Func<Point, Point, float> heuristic = null) : this(
                    new PathfindingNode(start),
                    new PathfindingNode(end),
                    tileValidity ?? PathfindingParameters.DefaultTileValidity,
                    distanceFunction ?? PathfindingParameters.DefaultDistanceFunction,
                    heuristic ?? PathfindingParameters.DefaultHeuristic
                )
            {
            }

            public PathfindingWork(
                PathfindingParameters p) : this(
                    new PathfindingNode(p.Start.ToTileCoordinates()),
                    new PathfindingNode(p.End.ToTileCoordinates()),
                    p.TileValidity ?? PathfindingParameters.DefaultTileValidity,
                    p.DistanceFunction ?? PathfindingParameters.DefaultDistanceFunction,
                    p.Heuristic ?? PathfindingParameters.DefaultHeuristic
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
            internal List<Vector2> CalculatePath()
            {
                if (Main.tile[End.Position].IsTileSolid())
                    return null;

                var openSet = new List<PathfindingNode>();
                var closedSet = new HashSet<PathfindingNode>();
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
        #endregion

        #region Pathfinding Node
        /// <summary>
        /// Represents a node in a pathfinding algorithm, containing position, cost, and parent information.
        /// </summary>
        private class PathfindingNode(Point position)
        {
            internal Point Position { get; set; } = position;
            internal float G { get; set; } // Cost from start to this node.
            internal float H { get; set; } // Heuristic cost to goal.
            internal float F => G + H;     // Total cost.
            internal PathfindingNode Parent { get; set; }

            public override bool Equals(object obj)
            {
                if (obj is PathfindingNode other)
                    return Position == other.Position;
                return false;
            }

            public override int GetHashCode() => Position.GetHashCode();

            internal List<PathfindingNode> GetNeighbors()
            {
                var neighbors = new List<PathfindingNode>(Directions.Count);

                int nodeX = Position.X;
                int nodeY = Position.Y;

                foreach (var direction in Directions)
                {
                    int newX = nodeX + (int)direction.X;
                    int newY = nodeY + (int)direction.Y;

                    if (WorldGen.InWorld(newX, newY))
                        neighbors.Add(new PathfindingNode(new Point(newX, newY)));
                }

                return neighbors;
            }

            internal List<Vector2> ReconstructPath()
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
        #endregion
    }
    #endregion

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
}
