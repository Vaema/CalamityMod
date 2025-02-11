using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Terraria;

namespace CalamityMod
{
    public static partial class CalamityUtils
    {
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
