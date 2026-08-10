using CalamityMod.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Armor.Umbraphile;

[AutoloadEquip(EquipType.Legs)]
public class UmbraphileBoots : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Armor.Hardmode";

    public static float RogueDamageBoost = 0.09f;
    public static int RogueCritBoost = 6;
    public static float MoveSpeedBoost = 0.2f;
    public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(RogueDamageBoost.ToPercent(), RogueCritBoost, MoveSpeedBoost.ToPercent());

    public override void SetDefaults()
    {
        Item.width = 22;
        Item.height = 18;
        Item.value = CalamityGlobalItem.RarityLimeBuyPrice;
        Item.rare = ItemRarityID.Lime;
        Item.defense = 14;
    }

    public override void UpdateEquip(Player player)
    {
        player.GetDamage<ThrowingDamageClass>() += RogueDamageBoost;
        player.GetCritChance<ThrowingDamageClass>() += RogueCritBoost;
        player.moveSpeed += MoveSpeedBoost;
    }

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient<SolarVeil>(14).
            AddIngredient(ItemID.HallowedBar, 11).
            AddTile(TileID.MythrilAnvil).
            Register();
    }
}
