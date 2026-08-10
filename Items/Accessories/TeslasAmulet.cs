using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables.SunkenSea;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Accessories;

public class TeslasAmulet : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Accessories";
    public override void SetStaticDefaults()
    {
        Main.RegisterItemAnimation(Type, new DrawAnimationVertical(5, 12));
        ItemID.Sets.AnimatesAsSoul[Type] = true;
    }

    public override void SetDefaults()
    {
        Item.width = 34;
        Item.height = 32;
        Item.rare = ItemRarityID.Orange;
        Item.value = CalamityGlobalItem.RarityOrangeBuyPrice;
        Item.accessory = true;
    }

    public override void UpdateAccessory(Player player, bool hideVisual)
    {
        player.Calamity().tesla = true;
        player.Calamity().teslaVisuals = !hideVisual;
    }

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient<AerialiteBar>(8).
            AddIngredient<SeaPrism>(8).
            AddIngredient<StormlionMandible>(4).
            AddTile(TileID.Anvils).
            Register();
    }
}
