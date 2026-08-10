using System;
using CalamityMod.Cooldowns;
using CalamityMod.Items.Materials;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Armor.PlagueReaper;

[AutoloadEquip(EquipType.Head)]
public class PlagueReaperMask : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Armor.Hardmode";
    public static readonly SoundStyle ActivationSound = new("CalamityMod/Sounds/Custom/AbilitySounds/PlagueReaperAbility");

    public static float RangedDamageBoost = 0.1f;
    public static int RangedCritBoost = 8;
    public static float AmmoReduction = 0.75f;
    public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(RangedDamageBoost.ToPercent(), RangedCritBoost, (1f - AmmoReduction).ToPercent());

    // Set Bonus
    public static float SetBonusFlightTimeBoost = 0.05f;
    public static float SetBonusPlaguedRangedDamageMult = 1.1f;
    public static float BlackoutRangedDamageBoost = 0.6f;
    public static int BlackoutRangedCritBoost = 20;
    public static int BlackoutDuration = CalamityUtils.SecondsToFrames(5);
    public static int BlackoutCooldown = CalamityUtils.SecondsToFrames(25);

    public override void SetDefaults()
    {
        Item.width = 18;
        Item.height = 18;
        Item.value = CalamityGlobalItem.RarityYellowBuyPrice;
        Item.rare = ItemRarityID.Yellow;
        Item.defense = 11; // 44
    }

    public override bool IsArmorSet(Item head, Item body, Item legs) => body.type == ModContent.ItemType<PlagueReaperVest>() && legs.type == ModContent.ItemType<PlagueReaperStriders>();

    public override void ArmorSetShadows(Player player) => player.armorEffectDrawOutlines = true;

    public override void UpdateArmorSet(Player player)
    {
        Color AbilityBriefColor = Color.Lerp(new Color(79, 100, 95), new Color(109, 209, 130), 0.5f + 0.5f * MathF.Sin(Main.GlobalTimeWrappedHourly * 3f));
        player.setBonus = this.GetLocalization("SetBonus").Format(SetBonusFlightTimeBoost.ToPercent(), SetBonusPlaguedRangedDamageMult, AbilityBriefColor.Hex3(), CalamityUtils.GetArmorSetBonusKey(), BlackoutRangedDamageBoost.ToPercent(), BlackoutRangedCritBoost, BlackoutDuration.FramesToSeconds(), BlackoutCooldown.FramesToSeconds());
        var modPlayer = player.Calamity();
        modPlayer.plagueReaper = true;

        var hasPlagueBlackoutCD = modPlayer.cooldowns.TryGetValue(PlagueBlackout.ID, out var cd);
        if (hasPlagueBlackoutCD && cd.timeLeft > BlackoutCooldown)
        {
            player.blind = true;
            player.headcovered = true;
            player.blackout = true;
            player.GetDamage<RangedDamageClass>() += BlackoutRangedDamageBoost;
            player.GetCritChance<RangedDamageClass>() += BlackoutRangedCritBoost;
        }
    }

    public override void UpdateEquip(Player player)
    {
        var modPlayer = player.Calamity();
        modPlayer.ammoCost *= AmmoReduction;
        player.GetDamage<RangedDamageClass>() += RangedDamageBoost;
        player.GetCritChance<RangedDamageClass>() += RangedCritBoost;
    }

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient(ItemID.NecroHelmet).
            AddIngredient<PlagueCellCanister>(15).
            AddIngredient(ItemID.Nanites, 11).
            AddTile(TileID.MythrilAnvil).
            Register();

        CreateRecipe().
            AddIngredient(ItemID.AncientNecroHelmet).
            AddIngredient<PlagueCellCanister>(15).
            AddIngredient(ItemID.Nanites, 11).
            AddTile(TileID.MythrilAnvil).
            Register();
    }
}
