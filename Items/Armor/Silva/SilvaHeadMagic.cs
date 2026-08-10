using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables.Abyss;
using CalamityMod.Rarities;
using CalamityMod.Tiles.Furniture.CraftingStations;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;
using static CalamityMod.Items.Armor.Silva.SilvaArmor;

namespace CalamityMod.Items.Armor.Silva;

[AutoloadEquip(EquipType.Head)]
[LegacyName("SilvaMaskedCap")]
public class SilvaHeadMagic : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Armor.PostMoonLord";

    public static int MaxManaBoost = 100;
    public static float ManaCostReduction = 0.19f;
    public static float MagicDamageBoost = 0.18f;
    public static int MagicCritBoost = 10;
    public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(MaxManaBoost, ManaCostReduction.ToPercent(), MagicDamageBoost.ToPercent(), MagicCritBoost);

    // Set Bonus
    public static int BurstCooldown = CalamityUtils.SecondsToFrames(5);
    public static int BurstDamage = 800; // Damage + (projectile damage) * Damage Ratio
    public static double BurstDamageRatio = 0.6D;
    public static int BurstDamageSoftcap = 1400;

    public override void SetDefaults()
    {
        Item.width = 24;
        Item.height = 22;
        Item.value = CalamityGlobalItem.RarityDarkBlueBuyPrice;
        Item.defense = 24; // 90
        Item.rare = ModContent.RarityType<CosmicPurple>();
    }

    public override bool IsArmorSet(Item head, Item body, Item legs) => body.type == ModContent.ItemType<SilvaArmor>() && legs.type == ModContent.ItemType<SilvaLeggings>();

    public override void ArmorSetShadows(Player player) => player.armorEffectDrawShadow = true;

    public override void UpdateArmorSet(Player player)
    {
        var modPlayer = player.Calamity();
        modPlayer.silvaSet = true;
        modPlayer.silvaMage = true;
        player.setBonus = this.GetLocalization("SetBonus").Format(SetBonusRegenBoost.ToRegenPerSecond(), AccelerationBoost.ToPercent(), ReviveDuration.FramesToSeconds(), (ReviveCooldown / 60).FramesToSeconds());
    }

    public override void UpdateEquip(Player player)
    {
        player.statManaMax2 += MaxManaBoost;
        player.manaCost -= ManaCostReduction;
        player.GetDamage<MagicDamageClass>() += MagicDamageBoost;
        player.GetCritChance<MagicDamageClass>() += MagicCritBoost;
    }

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient<PlantyMush>(30).
            AddIngredient<EffulgentFeather>(8).
            AddIngredient<AscendantSpiritEssence>(2).
            AddTile<CosmicAnvil>().
            SortBeforeFirstRecipesOf(ModContent.ItemType<SilvaArmor>()).
            Register();
    }
}
