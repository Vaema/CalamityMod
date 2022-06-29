using Terraria.ModLoader;
using Microsoft.Xna.Framework.Graphics;
using CalamityMod.Tiles.BaseTiles;

namespace CalamityMod.Tiles
{
    public class PerennialTree : DynamicallyGrownTree
    {
        // The max amount that branches can travel. Once the distance of the branches reaches or exceeds this threshold, the tree is done growing.
        public override float MaxDistanceBeforeCutoff => 940f;

        public override float PercentageOfDistanceUsedForTrunk => 0.2f;

        public override float BranchMaxBendFactor => 0.45f;

        public override float BranchTurnAngleVariance => 0.91f;

        public override float MinBranchLength => 10f;

        public override float TrunkWidth => 16f;

        public override float ChanceToCreateNewBranches => 0.3f;
    }
}
