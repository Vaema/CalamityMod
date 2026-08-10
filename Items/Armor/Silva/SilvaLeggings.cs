using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables.Abyss;
using CalamityMod.Rarities;
using CalamityMod.Tiles.Furniture.CraftingStations;
using Terraria;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Armor.Silva;

[AutoloadEquip(EquipType.Legs)]
public class SilvaLeggings : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Armor.PostMoonLord";

    public static float DamageBoost = 0.11f;
    public static int CritBoost = 11; // NOTE: Tooltip shares this number with damage % as they're equal
    public static float MoveSpeedBoost = 0.1f;
    public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(DamageBoost.ToPercent(), MoveSpeedBoost.ToPercent());

    public override void SetDefaults()
    {
        Item.width = 22;
        Item.height = 18;
        Item.value = CalamityGlobalItem.RarityDarkBlueBuyPrice;
        Item.defense = 28;
        Item.rare = ModContent.RarityType<CosmicPurple>();
    }

    public override void UpdateEquip(Player player)
    {
        player.GetDamage<GenericDamageClass>() += DamageBoost;
        player.GetCritChance<GenericDamageClass>() += CritBoost;
        player.moveSpeed += MoveSpeedBoost;
    }

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient<PlantyMush>(60).
            AddIngredient<EffulgentFeather>(10).
            AddIngredient<AscendantSpiritEssence>(3).
            AddTile<CosmicAnvil>().
            Register();
    }
}
