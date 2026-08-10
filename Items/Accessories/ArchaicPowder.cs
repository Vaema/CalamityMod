using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables.Crags;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Accessories;

public class ArchaicPowder : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Accessories";

    public static float MiningSpeedBoost = 0.25f;
    public static float TrapDamageReduction = 0.65f;
    public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(MiningSpeedBoost.ToPercent(), TrapDamageReduction.ToPercent());

    public override void SetDefaults()
    {
        Item.width = 56;
        Item.height = 34;
        Item.value = CalamityGlobalItem.RarityOrangeBuyPrice;
        Item.rare = ItemRarityID.Orange;
        Item.accessory = true;
    }

    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        player.pickSpeed -= MiningSpeedBoost;
        player.Calamity().fallingBlockProtection = true;
        player.Calamity().archaicPowder = true;
    }

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient<AncientFossil>().
            AddIngredient(ItemID.AncientChisel).
            AddIngredient<AncientBoneDust>(3).
            AddIngredient<ScorchedBone>(10).
            AddIngredient(ItemID.Bone, 15).
            AddTile(TileID.Anvils).
            Register();
    }
}
