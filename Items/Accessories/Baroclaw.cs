using CalamityMod.CalPlayer;
using CalamityMod.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Accessories;

public class Baroclaw : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Accessories";

    public static int ThornsDamage => CalamityUtils.ScaleWithDifficulty(150);

    public override void SetDefaults()
    {
        Item.width = 40;
        Item.height = 26;
        Item.value = CalamityGlobalItem.RarityLimeBuyPrice;
        Item.rare = ItemRarityID.Lime;
        Item.accessory = true;
    }

    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        CalamityPlayer modPlayer = player.Calamity();
        modPlayer.baroclaw = true;
    }
    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient<CrawCarapace>().
            AddIngredient<DepthCells>(20).
            AddTile(TileID.MythrilAnvil).
            Register();
    }
}
