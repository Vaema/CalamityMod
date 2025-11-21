using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CalamityMod.DataStructures;
using CalamityMod.Pathfinding;
using CalamityMod.Pathfinding.Movements;
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

        internal readonly IPathfinder Pathfinder;
        internal readonly Vector2 Start;
        internal readonly Vector2 End;
        internal readonly Func<Point, bool> TileValidity;
        internal readonly Func<Point, Point, float> DistanceFunction;
        internal readonly Func<Point, Point, float> Heuristic;

        public PathfindingParameters(
            IPathfinder pathfinder,
            Vector2 start,
            Vector2 end,
            Func<Point, bool> tileValidity = null,
            Func<Point, Point, float> distanceFunction = null,
            Func<Point, Point, float> heuristic = null)
        {
            Pathfinder = pathfinder;
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
    /// <param name="p">The entity to pathfind for.</param>
    public class PathfindingManager(IPathfinder p)
    {
        public List<Vector2> Path
        {
            get
            {
                if (lastSuccessfulTask == null)
                    return [];
                else if (lastSuccessfulTask.Result == null)
                    return [];
                else
                {
                    var path = new List<Vector2>();
                    foreach (var node in lastSuccessfulTask.Result)
                        path.Add(node.Position.ToWorldCoordinates());
                    return path;
                }
            }
        }

        /// <summary>
        /// The entity which this PathfindingManager manages. This cannot be changed after creation.
        /// </summary>
        internal readonly IPathfinder pathfinder = p;

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
                bool sameEndPosition = parameters.End.ToTileCoordinates() == currentTask.work.End;
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
                pathfinder.AwaitingPathBehavior();
                return;
            }

            PathfindingNode nextPoint = lastSuccessfulTask.Result[0];

            // If that point is reached, it is removed from the list of points to follow.
            bool nextPointReached = nextPoint.Move.FollowPath(nextPoint.Position.ToWorldCoordinates());
            if (nextPointReached)
            {
                Main.NewText(lastSuccessfulTask.Result[0].Move.RegistrationName);
                lastSuccessfulTask.Result.RemoveAt(0);
            }

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
            internal List<PathfindingNode> Result { get => _task.Task.Result; }

            /// <summary>
            /// Whether this pathfinding task has completed running.
            /// </summary>
            internal bool Ready { get; private set; } = false;

            /// <summary>
            /// Whether this pathfinding task is still currently running.
            /// </summary>
            internal bool Running { get; private set; } = false;

            // Interal task representation of the work.
            internal readonly TaskCompletionSource<List<PathfindingNode>> _task = new();

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
        private record PathfindingWork(IPathfinder Pathfinder, Point Start, Point End, Func<Point, bool> TileValidity, Func<Point, Point, float> DistanceFunction, Func<Point, Point, float> Heuristic)
        {
            public PathfindingWork(
                PathfindingParameters p) : this(
                    p.Pathfinder,
                    p.Start.ToTileCoordinates(),
                    p.End.ToTileCoordinates(),
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
            internal List<PathfindingNode> CalculatePath()
            {
                // Trying to pathfind to a solid tile would be a waste of resources, we don't even consider pathfinding to it.
                Tile endTile = ParanoidTileRetrieval(End.X, End.Y);
                if (endTile.IsTileSolid())
                    return null;

                var candidates = new HeapDict<PathfindingNode, float>();
                var explored = new HashSet<PathfindingNode>();

                // At first, we only know the starting node.
                foreach (var movement in Pathfinder.Movements)
                {
                    var start = new PathfindingNode(Start, movement);
                    (start.Distance, start.Total) = (0f, Heuristic.Invoke(start.Position, End));
                    candidates.Add(start, start.Total);
                }

                while (candidates.Count > 0)
                {
                    // Get the node with the lowest cost.
                    var current = candidates.PeekMin().Item1;

                    // Check if we've reached the goal.
                    if (current.Position == End)
                        return current.ReconstructPath();

                    // Add the current node to the set of explored nodes so we don't have to check it again,
                    // and also delete it from the candidates.
                    candidates.PopMin();
                    explored.Add(current);

                    foreach (var neighbor in current.GetNeighborSteps(Pathfinder))
                    {
                        // If the node has already been explored or is not a valid neighbor, we can skip it.
                        if (explored.Contains(neighbor) || Main.tile[neighbor.Position].IsTileSolid() || !TileValidity.Invoke(neighbor.Position))
                            continue;

                        // If going to the neighbor node through the current node is cheaper, we record that node to our candidates.
                        // Since nodes have, by default, a cost of infinity, first meeting them will always record them.
                        float newDistance = current.Distance + DistanceFunction.Invoke(current.Position, neighbor.Position);
                        if (newDistance >= neighbor.Distance)
                            continue;

                        neighbor.Parent = current;
                        neighbor.Distance = newDistance;
                        neighbor.Total = newDistance + Heuristic.Invoke(neighbor.Position, End) + neighbor.Move.Cost.Invoke(current.Position, neighbor.Position);

                        // We put it on our heap of candidates.
                        // If the node was already on our heap of candidates, it updates its priority.
                        candidates.Add(neighbor, neighbor.Total);
                    }
                }

                // If we ran out of candidates and no path was found, we return null.
                return null;
            }
        }
        #endregion

        #region Pathfinding Node
        /// <summary>
        /// Represents a node in a pathfinding algorithm, containing position, cost, and parent information.
        /// </summary>
        public class PathfindingNode(Point position, IMovement move)
        {
            internal Point Position { get; set; } = position;
            internal float Distance { get; set; } = float.MaxValue; // Cost from start to this node.
            internal float Total { get; set; } = float.MaxValue; // Total cost.
            internal IMovement Move { get; set; } = move;
            internal PathfindingNode Parent { get; set; }

            public override bool Equals(object obj)
            {
                if (obj is PathfindingNode other)
                    return Position == other.Position && Move.RegistrationName.Equals(other.Move.RegistrationName);
                return false;
            }

            public override int GetHashCode() => Position.GetHashCode() ^ Move.RegistrationName.GetHashCode();

            internal IEnumerable<PathfindingNode> GetNeighborSteps(IPathfinder pathfinder)
            {
                foreach (var movement in pathfinder.Movements)
                {
                    foreach (var neighbor in movement.GetDestinations(Position))
                    {
                        yield return new PathfindingNode(neighbor, movement);
                    }
                }
            }

            internal List<PathfindingNode> ReconstructPath()
            {
                var path = new List<PathfindingNode>();
                var current = this;
                while (current != null)
                {
                    path.Add(current);
                    current = current.Parent;
                }
                path.Reverse();
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
