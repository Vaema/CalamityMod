using Microsoft.Xna.Framework;
using ReLogic.Utilities;
using Terraria;
using Terraria.WorldBuilding;

namespace CalamityMod.World
{
    public class CustomShimmer
    {
        public static void NewShimmer()
        {
            // Large = 4, Medium = 3, Small = 2
            int worldSize = (int)(Main.maxTilesX / 4200f * 2f);
            int minOffsetX = 240;
            int maxOffsetX;
            switch (worldSize)
            {
                default:
                case 2:
                    maxOffsetX = 300;
                    break;

                case 3:
                    maxOffsetX = 360;
                    break;

                case 4:
                    maxOffsetX = 420;
                    break;
            }

            int offset = 50;
            int shimmerMinDepth = (int)((Main.worldSurface + Main.rockLayer) * 0.5) + offset;
            int shimmerMaxDepth = (int)(((double)((Main.maxTilesY - 250) * 2) + Main.rockLayer) * 0.33);
            shimmerMaxDepth = (int)MathHelper.Clamp(shimmerMaxDepth, shimmerMinDepth + 50, Main.maxTilesY - 460);

            int shimmerPositionY = WorldGen.genRand.Next(shimmerMinDepth, shimmerMaxDepth);
            int shimmerPositionX = ((GenVars.dungeonSide < 0) ? WorldGen.genRand.Next(Main.maxTilesX - maxOffsetX, Main.maxTilesX - minOffsetX) : WorldGen.genRand.Next(minOffsetX, maxOffsetX));
            int remixSeedShimmerMinDepth = (int)Main.worldSurface + 150;
            int remixSeedShimmerMaxDepth = (int)((Main.rockLayer + Main.worldSurface + 200D) * 0.5);
            if (remixSeedShimmerMaxDepth <= remixSeedShimmerMinDepth)
                remixSeedShimmerMaxDepth = remixSeedShimmerMinDepth + 50;

            if (WorldGen.tenthAnniversaryWorldGen)
                shimmerPositionY = WorldGen.genRand.Next(remixSeedShimmerMinDepth, remixSeedShimmerMaxDepth);

            int attempts = 0;
            while (!WorldGen.ShimmerMakeBiome(shimmerPositionX, shimmerPositionY))
            {
                attempts++;
                if (WorldGen.tenthAnniversaryWorldGen && attempts < 10000)
                {
                    shimmerPositionY = WorldGen.genRand.Next(remixSeedShimmerMinDepth, remixSeedShimmerMaxDepth);
                    shimmerPositionX = ((GenVars.dungeonSide < 0) ? WorldGen.genRand.Next(Main.maxTilesX - maxOffsetX, Main.maxTilesX - minOffsetX) : WorldGen.genRand.Next(minOffsetX, maxOffsetX));
                }
                else if (attempts > 20000)
                {
                    shimmerPositionY = WorldGen.genRand.Next((int)Main.worldSurface + 120, shimmerMaxDepth);
                    shimmerPositionX = ((GenVars.dungeonSide < 0) ? WorldGen.genRand.Next(Main.maxTilesX - maxOffsetX * 2, Main.maxTilesX - minOffsetX) : WorldGen.genRand.Next(minOffsetX, maxOffsetX * 2));
                }
                else
                {
                    shimmerPositionY = WorldGen.genRand.Next((int)((Main.worldSurface + Main.rockLayer) * 0.5) + 20, shimmerMaxDepth);
                    shimmerPositionX = ((GenVars.dungeonSide < 0) ? WorldGen.genRand.Next(Main.maxTilesX - maxOffsetX, Main.maxTilesX - minOffsetX) : WorldGen.genRand.Next(minOffsetX, maxOffsetX));
                }
            }

            GenVars.shimmerPosition = new Vector2D(shimmerPositionX, shimmerPositionY);
            int genArea = 200;
            GenVars.structures.AddProtectedStructure(new Rectangle(shimmerPositionX - (int)(genArea * 0.5f), shimmerPositionY - (int)(genArea * 0.5f), genArea, genArea));
        }
    }
}
