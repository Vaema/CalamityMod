using CalamityMod.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Armor.Astral;

[AutoloadEquip(EquipType.Body)]
public class AstralBreastplate : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Armor.Hardmode";

    public static int MaxManaBoost = 80;
    public static float AmmoReduction = 0.75f;
    public static float DamageBoost = 0.1f;
    public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(MaxManaBoost, DamageBoost.ToPercent(), (1f - AmmoReduction).ToPercent());

    public override void SetDefaults()
    {
        Item.width = 18;
        Item.height = 18;
        Item.value = CalamityGlobalItem.RarityCyanBuyPrice;
        Item.rare = ItemRarityID.Cyan;
        Item.defense = 25;
    }

    public override void UpdateEquip(Player player)
    {
        var modPlayer = player.Calamity();
        modPlayer.ammoCost *= AmmoReduction;
        player.statManaMax2 += MaxManaBoost;
        player.GetDamage<GenericDamageClass>() += DamageBoost;
    }

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient<AstralBar>(24).
            AddTile(TileID.LunarCraftingStation).
            Register();
    }
}
