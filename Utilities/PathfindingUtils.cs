using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
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
        /// A <see cref="HeapDict{TKey, TValue}"/> is a Dictionary-like data structure 
        /// that also keeps its entries in a binary-heap ordered by <see cref="TValue"/> (The priority).<br/>
        /// This allows for O(log n) retrievals of the <see cref="TKey"/> with the lowest <see cref="TValue"/>
        /// (Thanks to the MinHeap/PriorityQueue structure) and O(1) lookups of any element (Thanks to the Dictionary structure).<br/>
        /// This data structure is specially useful for graph algorithms where getting the minimum element is a constant process, like A*.<br/>
        /// It essentially combines a <see cref="PriorityQueue{TElement, TPriority}"/> and a <see cref="Dictionary{TKey, TValue}"/> into one data structure.
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        public class HeapDict<TKey, TValue> where TValue : IComparable<TValue>
        {
            private readonly Dictionary<TKey, int> _indexMap = [];
            private readonly List<(TKey Key, TValue Value)> _heap = [];

            public int Count => _heap.Count;

            public void Add(TKey key, TValue value)
            {
                if (_indexMap.TryGetValue(key, out int index))
                {
                    TValue oldValue = _heap[index].Value;
                    _heap[index] = (key, value);

                    int cmp = value.CompareTo(oldValue);
                    if (cmp < 0)
                        HeapifyUp(index);
                    else if (cmp > 0)
                        HeapifyDown(index);
                }
                else
                {
                    _heap.Add((key, value));
                    index = _heap.Count - 1;
                    _indexMap[key] = index;
                    HeapifyUp(index);
                }
            }

            public (TKey, TValue) PopMin()
            {
                if (_heap.Count == 0)
                    throw new InvalidOperationException("Heap is empty");

                var min = _heap[0];
                var last = _heap[^1];
                _heap[0] = last;
                _indexMap[last.Key] = 0;
                _heap.RemoveAt(_heap.Count - 1);
                _indexMap.Remove(min.Key);
                if (_heap.Count > 0)
                    HeapifyDown(0);

                return min;
            }

            public (TKey, TValue) PeekMin()
            {
                if (_heap.Count == 0)
                    throw new InvalidOperationException("Heap is empty");

                return _heap[0];
            }

            private void HeapifyUp(int index)
            {
                while (index > 0)
                {
                    int parent = (index - 1) / 2;
                    if (_heap[index].Value.CompareTo(_heap[parent].Value) >= 0)
                        break;

                    Swap(index, parent);
                    index = parent;
                }
            }

            private void HeapifyDown(int index)
            {
                int lastIndex = _heap.Count - 1;
                while (true)
                {
                    int left = 2 * index + 1;
                    int right = 2 * index + 2;
                    int smallest = index;

                    if (left <= lastIndex && _heap[left].Value.CompareTo(_heap[smallest].Value) < 0)
                        smallest = left;

                    if (right <= lastIndex && _heap[right].Value.CompareTo(_heap[smallest].Value) < 0)
                        smallest = right;

                    if (smallest == index)
                        break;

                    Swap(index, smallest);
                    index = smallest;
                }
            }

            private void Swap(int i, int j)
            {
                (_heap[i], _heap[j]) = (_heap[j], _heap[i]);
                _indexMap[_heap[i].Key] = i;
                _indexMap[_heap[j].Key] = j;
            }

            public bool ContainsKey(TKey key) => _indexMap.ContainsKey(key);

            public TValue GetValue(TKey key) => _heap[_indexMap[key]].Value;
        }

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
                // Trying to pathfind to a solid tile would be a waste of resources, we don't even consider pathfinding to it.
                Tile endTile = ParanoidTileRetrieval(End.Position.X, End.Position.Y);
                if (endTile.IsTileSolid())
                    return null;

                var candidates = new HeapDict<PathfindingNode, float>();
                var explored = new HashSet<PathfindingNode>();

                // At first, we only know the starting node.
                (Start.Distance, Start.Total) = (0, Heuristic.Invoke(Start.Position, End.Position));
                candidates.Add(Start, Start.Total);

                while (candidates.Count > 0)
                {
                    // Get the node with the lowest cost.
                    var current = candidates.PeekMin().Item1;

                    // Check if we've reached the goal.
                    if (current.Position == End.Position)
                        return current.ReconstructPath();

                    // Add the current node to the set of explored nodes so we don't have to check it again,
                    // and also delete it from the candidates.
                    candidates.PopMin();
                    explored.Add(current);

                    foreach (var neighbor in current.GetNeighbors())
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
                        neighbor.Total = newDistance + Heuristic.Invoke(neighbor.Position, End.Position);

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
        private class PathfindingNode(Point position)
        {
            internal Point Position { get; set; } = position;
            internal float Distance { get; set; } = float.MaxValue; // Cost from start to this node.
            internal float Total { get; set; } = float.MaxValue; // Total cost.
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
                var path = new List<Vector2>();
                var current = this;
                while (current != null)
                {
                    path.Add(current.Position.ToWorldCoordinates());
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
