using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables.Abyss;
using CalamityMod.Rarities;
using CalamityMod.Tiles.Furniture.CraftingStations;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Armor.Silva
{
    [AutoloadEquip(EquipType.Body)]
    public class SilvaArmor : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Armor.PostMoonLord";
        public static readonly SoundStyle ActivationSound = new("CalamityMod/Sounds/Custom/AbilitySounds/SilvaActivation");
        public static readonly SoundStyle DispelSound = new("CalamityMod/Sounds/Custom/AbilitySounds/SilvaDispel");

        public static float DamageBoost = 0.15f;
        public static int CritBoost = 12;
        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(DamageBoost.ToPercent(), CritBoost);

        // Common Set Bonus
        public static int SetBonusRegenBoost = 6;
        public static float AccelerationBoost = 0.05f;
        public static int ReviveDuration = CalamityUtils.SecondsToFrames(5);
        public static int ReviveCooldown = CalamityUtils.SecondsToFrames(5 * 60);

        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 24;
            Item.value = CalamityGlobalItem.RarityDarkBlueBuyPrice;
            Item.defense = 38;
            Item.rare = ModContent.RarityType<CosmicPurple>();
        }

        public override void UpdateEquip(Player player)
        {
            player.GetDamage<GenericDamageClass>() += DamageBoost;
            player.GetCritChance<GenericDamageClass>() += CritBoost;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<PlantyMush>(12).
                AddIngredient<EffulgentFeather>(10).
                AddIngredient<AscendantSpiritEssence>(3).
                AddTile<CosmicAnvil>().
                Register();
        }
    }
}
