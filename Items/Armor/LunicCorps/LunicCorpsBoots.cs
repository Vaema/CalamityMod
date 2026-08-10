using CalamityMod.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Armor.LunicCorps;

[AutoloadEquip(EquipType.Legs)]
public class LunicCorpsBoots : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Armor.Hardmode";

    public static int RangedCritBoost = 7;
    public static float MoveSpeedAccelerationBoost = 0.15f;
    public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(RangedCritBoost, MoveSpeedAccelerationBoost.ToPercent());

    public override void SetDefaults()
    {
        Item.width = 18;
        Item.height = 18;
        Item.value = CalamityGlobalItem.RarityCyanBuyPrice;
        Item.defense = 18;
        Item.rare = ItemRarityID.Cyan;
        Item.Calamity().donorItem = true;
    }

    public override void UpdateEquip(Player player)
    {
        player.Calamity().lunicCorpsLegs = true;
        player.GetCritChance<RangedDamageClass>() += RangedCritBoost;
        player.moveSpeed += MoveSpeedAccelerationBoost;
    }

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient<AstralBar>(8).
            AddIngredient(ItemID.ChlorophyteBar, 8).
            AddTile(TileID.LunarCraftingStation).
            Register();
    }
}
