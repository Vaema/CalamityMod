using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Accessories;

[LegacyName("GravistarSabaton")]
public class InterstellarStompers : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Accessories";

    public static readonly int PassthroughDamage = 150;
    public static readonly int SlamDamage = 300;
    public static readonly int PassthroughIFrames = 5;

    public override void SetDefaults()
    {
        Item.width = 20;
        Item.height = 22;
        Item.value = CalamityGlobalItem.RarityLimeBuyPrice;
        Item.accessory = true;
        Item.rare = ItemRarityID.Lime;
        Item.expert = true;
    }

    public override void ModifyTooltips(List<TooltipLine> list) => list.IntegrateDynamicHotkey(Item);

    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        player.noFallDmg = true;
        player.moveSpeed += 0.06f;
        player.jumpSpeedBoost += 1f;
        player.Calamity().gSabaton = true;
    }
}
