using System;
using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.Items.Materials;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Armor.Brimflame
{
    [AutoloadEquip(EquipType.Head)]
    [LegacyName("BrimflameScowl")]
    public class BrimflameCowl : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Armor.Hardmode";
        public static readonly SoundStyle ActivationSound = new("CalamityMod/Sounds/Custom/AbilitySounds/BrimflameAbility");

        public static int MaxManaBoost = 80;
        public static float ManaCostReduction = 0.1f;
        public static float MagicDamageBoost = 0.1f;
        public static int MagicCritBoost = 10; // NOTE: Tooltip shares this number with damage % as they're equal
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(MaxManaBoost, ManaCostReduction.ToPercent(), MagicDamageBoost.ToPercent());

        // Set Bonus
        public static float SetBonusMagicDamageBoost = 0.08f;
        public static int SetBonusMagicCritBoost = 8; // NOTE: Tooltip shares this number with damage % as they're equal
        public static float FrenzyMagicDamageBoost = 0.4f;
        public static int FrenzyDuration = CalamityUtils.SecondsToFrames(10);
        public static int FrenzyCooldown = CalamityUtils.SecondsToFrames(30);

        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 18;
            Item.value = CalamityGlobalItem.RarityLimeBuyPrice;
            Item.rare = ItemRarityID.Lime;
            Item.defense = 12;
        }

        public override void UpdateEquip(Player player)
        {
            player.GetDamage<MagicDamageClass>() += MagicDamageBoost;
            player.GetCritChance<MagicDamageClass>() += MagicCritBoost;
            player.statManaMax2 += MaxManaBoost;
            player.manaCost -= ManaCostReduction;
            player.buffImmune[ModContent.BuffType<BrimstoneFlames>()] = true;
            player.buffImmune[BuffID.OnFire] = true;
            player.buffImmune[BuffID.Frostburn] = true;
        }

        public override bool IsArmorSet(Item head, Item body, Item legs) => body.type == ModContent.ItemType<BrimflameRobes>() && legs.type == ModContent.ItemType<BrimflameBoots>();

        public override void ArmorSetShadows(Player player) => player.armorEffectDrawShadowSubtle = true;

        public override void UpdateArmorSet(Player player)
        {
            var modPlayer = player.Calamity();
            modPlayer.brimflameSet = true;
            player.GetDamage<MagicDamageClass>() += SetBonusMagicDamageBoost;
            player.GetCritChance<MagicDamageClass>() += SetBonusMagicCritBoost;
            Color AbilityBriefColor = Color.Lerp(new Color(250, 202, 140), new Color(227, 79, 79), 0.5f + 0.5f * MathF.Sin(Main.GlobalTimeWrappedHourly * 3f));
            player.setBonus = this.GetLocalization("SetBonus").Format(SetBonusMagicDamageBoost.ToPercent(), AbilityBriefColor.Hex3(), CalamityUtils.GetArmorSetBonusKey(), FrenzyDuration.FramesToSeconds(), FrenzyMagicDamageBoost.ToPercent(), FrenzyCooldown.FramesToSeconds());
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<UnholyCore>(8).
                AddIngredient<AshesofCalamity>(4).
                AddTile(TileID.MythrilAnvil).
                SortBeforeFirstRecipesOf(ModContent.ItemType<BrimflameBoots>()).
                Register();
        }
    }
}
