using System.Collections.Generic;
using CalamityMod.CalPlayer;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Tools;

public class MomentumCapacitor : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Tools";
    internal const float MomentumChargePerFrame = 0.02f;
    internal const float MaxMomentumCharge = 5.8f; // +580% movemnt speed
    internal const int TotalFadeTime = 16;

    public override void SetDefaults()
    {
        Item.width = 28;
        Item.height = 32;
        Item.value = Item.buyPrice(gold: 50); // Sold by Bandit
        Item.rare = ItemRarityID.Pink;

        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.useAnimation = Item.useTime = 2;
        Item.noMelee = true;
        Item.channel = true;
        Item.autoReuse = true;
        Item.useTurn = true;
    }

    public override void ModifyResearchSorting(ref ContentSamples.CreativeHelper.ItemGroup itemGroup)
    {
        itemGroup = (ContentSamples.CreativeHelper.ItemGroup)CalamityResearchSorting.ToolsOther;
    }

    public override bool? UseItem(Player player)
    {
        if (!CalamityPlayer.areThereAnyDamnBosses)
        {
            CalamityPlayer modPlayer = player.Calamity();
            modPlayer.momentumCapacitorTime = TotalFadeTime;
            modPlayer.momentumCapacitorBoost += Main.rand.NextFloat(0.5f, 3.5f) * MomentumChargePerFrame;
            if (modPlayer.momentumCapacitorBoost >= MaxMomentumCharge && !Main.zenithWorld)
                modPlayer.momentumCapacitorBoost = MaxMomentumCharge;
        }
        return null;
    }

    public override void UpdateInventory(Player player)
    {
        if (Main.zenithWorld)
            Item.SetNameOverride(this.GetLocalizedValue("GFBName"));
    }

    public override void ModifyTooltips(List<TooltipLine> tooltips) => tooltips.FindAndReplace("[GFB]", this.GetLocalizedValue(Main.zenithWorld ? "TooltipGFB" : "TooltipNormal"));
}
