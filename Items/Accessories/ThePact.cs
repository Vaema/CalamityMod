using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Buffs.StatBuffs;
using CalamityMod.Systems.Collections;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Accessories;

public class ThePact : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Accessories";

    public static float MaxLifeMult => 2;
    public static float ChanceToBeCrit => 0.25f;
    public static float CritDmgTaken => 2.25f;
    public static int BoostDuration => 900;
    public static int DebuffInflictionDuration => 60;
    public static float NaturalRegenBoost => 1.75f;
    public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(MaxLifeMult, ChanceToBeCrit.ToPercent(), CritDmgTaken);

    public override void SetStaticDefaults()
    {
        CalamityItemSets.ExtraDebuffTooltip_Enemy[Type] = [ModContent.BuffType<Laceration>()];
    }

    public override void SetDefaults()
    {
        Item.width = 46;
        Item.height = 64;
        Item.value = CalamityGlobalItem.RarityLightRedBuyPrice;
        Item.rare = ItemRarityID.LightRed;
        Item.accessory = true;
    }

    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        player.Calamity().CanBeCritByThePact = true;

        if (player.HasBuff<FulfilledContract>() && player.miscCounter % 3 == 0 && player.potionDelay > 0)
        {
            player.potionDelay -= 1;
            if (player.potionDelay < 0)
                player.potionDelay = 0;
            if (player.HasBuff(BuffID.PotionSickness))
            {
                for (var i = 0; i < player.buffType.Length; i++)
                {
                    if (player.buffType[i] == BuffID.PotionSickness)
                    {
                        player.buffTime[i] = player.potionDelay;
                    }
                }
            }
        }
    }
}
