using CalamityMod.CalPlayer;
using System;
using Microsoft.Xna.Framework;
using Terraria;

namespace CalamityMod
{
    public static partial class CalamityUtils
    {
        /// <summary>
        /// Gets a unit direction towards an arbitrary destination for an entity based on its center. Has <see cref="float.NaN"/> safety in the form of a fallback vector.
        /// </summary>
        /// <param name="entity">The entity to check from.</param>
        /// <param name="destination">The destination to get the direction to.</param>
        /// <param name="fallback">A fallback value to use in the event of an unsafe normalization.</param>
        public static Vector2 SafeDirectionTo(this Entity entity, Vector2 destination, Vector2? fallback = null)
        {
            // Fall back to zero by default. default(Vector2) could be used in the parameter definition, but
            // this is more clear.
            if (!fallback.HasValue)
                fallback = Vector2.Zero;

            return (destination - entity.Center).SafeNormalize(fallback.Value);
        }

        /// <summary>
        /// Adds screenshake to the local player, using the given position and range to determine whether the player is able to see the screenshake.
        /// </summary>
        /// <param name="position">The center of the screenshake, where it is most intense.</param>
        /// <param name="intensity">The maximum intensity of the screenshake.</param>
        /// <param name="range">The distance from which the screenshake's power becomes zero.</param>
        public static void AddScreenshakeAt(Vector2 position, float intensity, float range = 1000)
        {
            float dist = 1;
            dist -= position.Distance(Main.LocalPlayer.Center) / range;

            dist = Math.Max(dist, 0);

            Main.LocalPlayer.GetModPlayer<CalamityPlayer>().GeneralScreenShakePower += (intensity * dist);
        }
    }
}
