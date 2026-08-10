using System;

namespace CalamityMod.Enums;

/// <summary>
/// The flags corresponding for the possible spawns for a Sunken Sea NPC.
/// </summary>
[Flags]
public enum SunkenSeaBiomeFlags : byte
{
    None = 0,
    UndergroundDesert = 1,
    TimelessShores = 2,
    RadiantReefs = 4,
    PolypForest = 8,
    GleamingBurrows = 16,
    BasaltGully = 32,
    ClamDen = 64
}
