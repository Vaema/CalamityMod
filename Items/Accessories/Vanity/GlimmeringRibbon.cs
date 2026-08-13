using CalamityMod.Items.BaseItems;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Accessories.Vanity;

internal class GlimmeringRibbon : TransformationAccessory, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Accessories";

    public override (EquipType, string, string)[] EquipSlots =>
    [
        (EquipType.Head, "Charlotte", null),
        (EquipType.Body, "Charlotte", null),
        (EquipType.Legs, "Charlotte", null),
    ];

    public override void SetDefaults()
    {
        Item.width = 52;
        Item.height = 46;
        Item.accessory = true;
        Item.vanity = true;
        Item.rare = ItemRarityID.Blue;
        Item.value = CalamityGlobalItem.RarityBlueBuyPrice;
        Item.Calamity().devItem = true;
    }

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient(ItemID.Silk, 10).
            AddIngredient(ItemID.FallenStar, 3).
            AddTile(TileID.Loom).
            Register();
    }
}
