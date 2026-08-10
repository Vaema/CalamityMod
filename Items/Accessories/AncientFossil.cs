using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Accessories;

public class AncientFossil : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Accessories";

    public static float MiningSpeedBoost = 0.1f;
    public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(MiningSpeedBoost.ToPercent());

    public override void SetDefaults()
    {
        Item.width = 26;
        Item.height = 26;
        Item.value = CalamityGlobalItem.RarityBlueBuyPrice;
        Item.rare = ItemRarityID.Blue;
        Item.accessory = true;
    }

    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        player.pickSpeed -= MiningSpeedBoost;
        player.Calamity().aFossil = true;
        player.Calamity().fallingBlockProtection = true;
    }

    public override void AddRecipes()
    {
        CreateRecipe().
            AddRecipeGroup("AnySiltBlock", 100).
            AddTile(TileID.Furnaces).
            Register();
    }
}
