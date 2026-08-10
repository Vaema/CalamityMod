using CalamityMod.Items.Accessories;
using CalamityMod.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Armor.LunicCorps;

[AutoloadEquip(EquipType.Body)]
public class LunicCorpsVest : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Armor.Hardmode";

    public static float RangedDamageBoost = 0.15f;
    public static int RangedCritBoost = 15; // NOTE: Tooltip shares this number with damage % as they're equal
    public static float AmmoReduction = 0.75f;
    public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(RangedDamageBoost.ToPercent(), (1f - AmmoReduction).ToPercent());

    public override void SetDefaults()
    {
        Item.width = 18;
        Item.height = 18;
        Item.value = CalamityGlobalItem.RarityCyanBuyPrice;
        Item.defense = 24;
        Item.rare = ItemRarityID.Cyan;
        Item.Calamity().donorItem = true;
    }

    public override void UpdateEquip(Player player)
    {
        var modPlayer = player.Calamity();
        modPlayer.ammoCost *= AmmoReduction;
        player.GetDamage<RangedDamageClass>() += RangedDamageBoost;
        player.GetCritChance<RangedDamageClass>() += RangedCritBoost;
    }

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient<RoverDrive>().
            AddIngredient<AstralBar>(11).
            AddIngredient(ItemID.ChlorophyteBar, 11).
            AddTile(TileID.LunarCraftingStation).
            SortBeforeFirstRecipesOf(ModContent.ItemType<LunicCorpsBoots>()).
            Register();
    }
}
