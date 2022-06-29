using CalamityMod.Tiles.BaseTiles;

namespace CalamityMod.Tiles
{
    public class PerennialTree : DynamicallyGrownTree
    {
        // The max amount that branches can travel. Once the distance of the branches reaches or exceeds this threshold, the tree is done growing.
        public override float MaxDistanceBeforeCutoff => 2100f;

        public override float DistanceUsedForTrunk => 220f;

        public override float BranchMaxBendFactor => 0.67f;

        public override float BranchTurnAngleVariance => 0.45f;

        public override float MinBranchLength => 10f;

        public override float TrunkWidth => 16f;

        public override float ChanceToCreateNewBranches => 0.75f;

        public override float VerticalStretchFactor => 10f;
    }
}
