using CalamityMod.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Armor.Statigel;

[AutoloadEquip(EquipType.Legs)]
public class StatigelGreaves : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Armor.PreHardmode";

    public static float DamageBoost = 0.05f;
    public static float MoveSpeedBoost = 0.05f; // NOTE: Tooltip shares this number with damage % as they're equal
    public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(DamageBoost.ToPercent());

    public override void SetDefaults()
    {
        Item.width = 18;
        Item.height = 18;
        Item.value = CalamityGlobalItem.RarityLightRedBuyPrice;
        Item.rare = ItemRarityID.LightRed;
        Item.defense = 8;
    }

    public override void UpdateEquip(Player player)
    {
        player.GetDamage<GenericDamageClass>() += DamageBoost;
        player.moveSpeed += MoveSpeedBoost;
    }

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient<PurifiedGel>(10).
            AddIngredient<BlightedGel>(10).
            AddTile(TileID.Solidifier).
            Register();
    }
}
