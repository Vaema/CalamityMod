using System;

namespace CalamityMod.Enums
{
    /// <summary>
    /// A general collection of points in Terraria's overall draw order that graphical systems in the mod draw to.
    /// </summary>
    [Flags]
    public enum GeneralDrawLayer
    {
        BeforeAllTiles = 0,
        BeforeSolidTiles = 1,
        BeforeNPCs = 2,
        AfterNPCs = 4,
        BeforeProjectiles = 8,
        AfterProjectiles = 16,
        AfterPlayers = 32,
        AfterDusts = 64,
        AfterEverything = 128,
    }
}
