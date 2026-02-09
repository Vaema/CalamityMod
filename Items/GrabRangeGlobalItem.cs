using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items;

internal sealed class GrabRangeGlobalItem : GlobalItem
{
    /// <summary>
    /// If set to a value greater than 1, applies a multiplier to the item's grab range.<br/>
    /// Used by coin items spawned from hitting ricoshot coins.
    /// </summary>
    public float grabRangeMultiplier = 1f;

    public override bool InstancePerEntity => true;

    public override bool AppliesToEntity(Item entity, bool lateInstantiation)
    {
        return entity.type switch
        {
            ItemID.CopperCoin
            or ItemID.SilverCoin
            or ItemID.GoldCoin
            or ItemID.PlatinumCoin => true,
            _ => false,
        };
    }

    public override GlobalItem Clone(Item item, Item itemClone)
    {
        GrabRangeGlobalItem myClone = (GrabRangeGlobalItem)base.Clone(item, itemClone);
        myClone.grabRangeMultiplier = grabRangeMultiplier;
        return myClone;
    }

    // GrabRange hook is Placed in CalamityGlobalItem for sake of ordering
}
