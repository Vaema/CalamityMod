using CalamityMod.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Armor.Umbraphile;

[AutoloadEquip(EquipType.Body)]
public class UmbraphileRegalia : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Armor.Hardmode";

    public static float RogueDamageBoost = 0.1f;
    public static int RogueCritBoost = 10; // NOTE: Tooltip shares this number with damage % as they're equal
    public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(RogueDamageBoost.ToPercent());

    public override void SetDefaults()
    {
        Item.width = 38;
        Item.height = 24;
        Item.value = CalamityGlobalItem.RarityLimeBuyPrice;
        Item.rare = ItemRarityID.Lime;
        Item.defense = 18;
    }

    public override void UpdateEquip(Player player)
    {
        player.GetDamage<ThrowingDamageClass>() += RogueDamageBoost;
        player.GetCritChance<ThrowingDamageClass>() += RogueCritBoost;
    }

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient<SolarVeil>(18).
            AddIngredient(ItemID.HallowedBar, 15).
            AddTile(TileID.MythrilAnvil).
            SortBeforeFirstRecipesOf(ModContent.ItemType<UmbraphileBoots>()).
            Register();
    }
}
