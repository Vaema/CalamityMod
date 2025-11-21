using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace CalamityMod.Pathfinding.Movements
{
    /// <summary>
    /// Defines an interface for what is a movement in the context of pathfinding.
    /// Every movement has a cost function, and a way to follow a path.
    /// </summary>
    public interface IMovement
    {
        /// <summary>
        /// The internal name given to this movement.<br/>
        /// Needed to differentiate between movements.
        /// </summary>
        string RegistrationName { get; }

        /// <summary>
        /// The cost function of this movement.<br/>
        /// Takes in two positions and returns the cost of the movement.
        /// </summary>
        Func<Point, Point, float> Cost { get; }

        /// <summary>
        /// Defines a method that is run whenever this movement starts execution.<br/>
        /// Useful to get any variable set up for this movement's action.
        /// </summary>
        void Start();

        /// <summary>
        /// A method that defines the way to follow from one position to another.
        /// </summary>
        /// <param name="nextPoint">The destination of this movement.</param>
        /// <returns><see langword="true"/> if it this movement has finished; otherwise <see langword="false"/>.</returns>
        bool FollowPath(Vector2 nextPoint);

        /// <summary>
        /// Defines a method to retrieve the possiblement destinations that this movement can achieve
        /// </summary>
        /// <param name="current">The starting position where this movement takes place.</param>
        /// <returns>An enumerable collection of <see cref="Point"/> with all the destinations.</returns>
        IEnumerable<Point> GetDestinations(Point current);
    }
}
