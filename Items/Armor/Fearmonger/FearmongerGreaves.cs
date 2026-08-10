using CalamityMod.Items.Materials;
using CalamityMod.Rarities;
using CalamityMod.Tiles.Furniture.CraftingStations;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Armor.Fearmonger;

[AutoloadEquip(EquipType.Legs)]
public class FearmongerGreaves : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Armor.PostMoonLord";

    public static float DamageBoost = 0.1f;
    public static float MoveSpeedBoost = 0.2f;
    public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(DamageBoost.ToPercent(), MoveSpeedBoost.ToPercent());

    public override void SetDefaults()
    {
        Item.width = 18;
        Item.height = 18;
        Item.value = CalamityGlobalItem.RarityDarkBlueBuyPrice;
        Item.defense = 42;
        Item.rare = ModContent.RarityType<CosmicPurple>();
    }

    public override void UpdateEquip(Player player)
    {
        player.GetDamage<GenericDamageClass>() += DamageBoost;
        player.moveSpeed += MoveSpeedBoost;
    }

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient(ItemID.SpookyLeggings).
            AddIngredient<CosmiliteBar>(10).
            AddIngredient<AscendantSpiritEssence>(2).
            AddIngredient(ItemID.SoulofFright, 10).
            AddTile<CosmicAnvil>().
            Register();
    }
}
