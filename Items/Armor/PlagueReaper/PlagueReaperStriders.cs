using CalamityMod.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Armor.PlagueReaper;

[AutoloadEquip(EquipType.Legs)]
public class PlagueReaperStriders : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Armor.Hardmode";

    public static int RangedCritBoost = 8;
    public static float MoveSpeedBoost = 0.15f;
    public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(RangedCritBoost, MoveSpeedBoost.ToPercent());

    public override void SetDefaults()
    {
        Item.width = 18;
        Item.height = 18;
        Item.value = CalamityGlobalItem.RarityYellowBuyPrice;
        Item.rare = ItemRarityID.Yellow;
        Item.defense = 14;
    }

    public override void UpdateEquip(Player player)
    {
        player.GetCritChance<RangedDamageClass>() += RangedCritBoost;
        player.moveSpeed += MoveSpeedBoost;
    }

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient(ItemID.NecroGreaves).
            AddIngredient<PlagueCellCanister>(21).
            AddIngredient(ItemID.Nanites, 17).
            AddTile(TileID.MythrilAnvil).
            Register();
    }
}
