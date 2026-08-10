using System;
using System.Collections.Generic;
using CalamityMod.NPCs.SunkenSea;
using Microsoft.Xna.Framework;
using Terraria;

namespace CalamityMod.Pathfinding.Movements;

public class SunkenSeaSwimMovement(NPC creature) : IMovement
{
    public NPC Creature = creature;
    private SunkenSeaNPC ssn => Creature.ModNPC<SunkenSeaNPC>();

    public Func<Point, Point, float> Cost => (_, _) => 0;

    public string RegistrationName => "Swim";

    public bool FollowPath(Vector2 nextPoint)
    {
        // Accelerate to the target point.
        Creature.velocity += (nextPoint - Creature.Center).SafeNormalize(Vector2.Zero) * ssn.Acceleration;

        // Cap the speed if MaxSpeed has been surpassed.
        if (Creature.velocity.LengthSquared() > ssn.MaxSpeed * ssn.MaxSpeed)
            Creature.velocity = Creature.velocity.SafeNormalize(Vector2.UnitY) * ssn.MaxSpeed;

        // If the NPC is within 48 pixels of its target point, consider the point reached.
        if (Vector2.DistanceSquared(Creature.Center, nextPoint) < ssn.MinimumPointDistance * ssn.MinimumPointDistance)
            return true;

        // Otherwise, continue following.
        return false;
    }

    public IEnumerable<Point> GetDestinations(Point current)
    {
        int nodeX = current.X;
        int nodeY = current.Y;
        foreach (var direction in CalamityUtils.Directions)
            yield return new Point(nodeX + (int)direction.X, nodeY + (int)direction.Y);
    }

    public void Start() { }
}
