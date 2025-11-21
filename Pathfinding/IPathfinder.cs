using System.Collections.Generic;
using CalamityMod.Pathfinding.Movements;

namespace CalamityMod.Pathfinding
{
    /// <summary>
    /// Defines an interface for a class that can use pathfinding.
    /// </summary>
    public interface IPathfinder
    {
        /// <summary>
        /// A enumerable collection of <see cref="IMovement"/> this pathfinder can use.
        /// </summary>
        IEnumerable<IMovement> Movements { get; }

        /// <summary>
        /// A method that defines behavior when the pathfinding is still being calculated.
        /// </summary>
        void AwaitingPathBehavior();
    }
}
