using CalamityMod.Projectiles.Typeless;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Ammo;

public class AstralSolution : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Ammo";
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 99;
        ItemID.Sets.SortingPriorityTerraforming[Type] = 94; // Red Solution
    }

    public override void SetDefaults() => Item.DefaultToSolution(ModContent.ProjectileType<AstralSpray>());

    public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup)
    {
        itemGroup = ContentSamples.CreativeHelper.ItemGroup.Solutions;
    }
}
