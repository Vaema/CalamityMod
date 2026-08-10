using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables.FurnitureAcidwood;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Armor.Sulphurous;

[AutoloadEquip(EquipType.Legs)]
[LegacyName("SulfurLeggings")]
public class SulphurousLeggings : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Armor.PreHardmode";

    public static float RogueDamageBoost = 0.04f;
    public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(RogueDamageBoost.ToPercent());

    public override void SetDefaults()
    {
        Item.width = 22;
        Item.height = 16;
        Item.value = CalamityGlobalItem.RarityGreenBuyPrice;
        Item.defense = 5;
        Item.rare = ItemRarityID.Green;
    }

    public override void UpdateEquip(Player player) => player.GetDamage<ThrowingDamageClass>() += RogueDamageBoost;

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient<Acidwood>(15).
            AddIngredient<SulphuricScale>(15).
            AddTile(TileID.Anvils).
            Register();
    }
}
