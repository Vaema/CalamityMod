using CalamityMod.Buffs.Summon;
using CalamityMod.CalPlayer;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables.Abyss;
using CalamityMod.Items.Potions.Alcohol;
using CalamityMod.Projectiles.Summon;
using CalamityMod.Rarities;
using CalamityMod.Tiles.Furniture.CraftingStations;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static CalamityMod.Items.Armor.Silva.SilvaArmor;

namespace CalamityMod.Items.Armor.Silva
{
    [AutoloadEquip(EquipType.Head)]
    [LegacyName("SilvaHelmet")]
    public class SilvaHeadSummon : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Armor.PostMoonLord";
        internal static string SilvaCrystalEntitySourceContext => "SetBonus_Calamity_SilvaSummon";

        public static int MinionSlotBoost = 2;
        public static float SummonDamageBoost = 0.3f;
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(MinionSlotBoost, SummonDamageBoost.ToPercent());

        // Set Bonus
        public static int SetBonusMinionSlotBoost = 3;
        public static float SetBonusSummonDamageBoost = 0.4f;
        public static int CrystalDamage = 380;

        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 24;
            Item.value = CalamityGlobalItem.RarityDarkBlueBuyPrice;
            Item.defense = 18; // 84
            Item.rare = ModContent.RarityType<CosmicPurple>();
        }

        public override bool IsArmorSet(Item head, Item body, Item legs) => body.type == ModContent.ItemType<SilvaArmor>() && legs.type == ModContent.ItemType<SilvaLeggings>();

        public override void ArmorSetShadows(Player player) => player.armorEffectDrawShadow = true;

        public override void UpdateArmorSet(Player player)
        {
            var modPlayer = player.Calamity();
            modPlayer.silvaSet = true;
            modPlayer.silvaSummon = true;
            modPlayer.WearingPostMLSummonerSet = true;
            player.setBonus = this.GetLocalization("SetBonus").Format(SetBonusMinionSlotBoost, SetBonusSummonDamageBoost.ToPercent()) + "\n" + CalamityUtils.GetTextFromModItem<SilvaArmor>("CommonSetBonus").Format(SetBonusRegenBoost.ToRegenPerSecond(), AccelerationBoost.ToPercent(), ReviveDuration.FramesToSeconds(), (ReviveCooldown / 60).FramesToSeconds());
            player.maxMinions += SetBonusMinionSlotBoost;
            player.GetDamage<SummonDamageClass>() += SetBonusSummonDamageBoost;
        }

        public override void UpdateEquip(Player player)
        {
            player.maxMinions += MinionSlotBoost;
            player.GetDamage<SummonDamageClass>() += SummonDamageBoost;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<PlantyMush>(6).
                AddIngredient<EffulgentFeather>(5).
                AddIngredient<AscendantSpiritEssence>(2).
                AddTile<CosmicAnvil>().
                SortBeforeFirstRecipesOf(ModContent.ItemType<SilvaArmor>()).
                Register();
        }
    }
}
