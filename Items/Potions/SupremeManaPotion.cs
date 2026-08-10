using CalamityMod.Items.Materials;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Potions;

public class SupremeManaPotion : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Potions";
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 30;
        ItemID.Sets.DrinkParticleColors[Type] = new Color[3] {
            new Color(0, 255, 250),
            new Color(26, 117, 177),
            new Color(160, 82, 144)
        };
    }

    public override void SetDefaults()
    {
        Item.DefaultToFood(34, 38, 0, 0, true);
        Item.healMana = 400;
        Item.value = Item.sellPrice(silver: 10);
        Item.rare = ItemRarityID.Purple;
    }

    public override void AddRecipes()
    {
        CreateRecipe(15).
            AddIngredient(ItemID.SuperManaPotion, 15).
            AddIngredient<Necroplasm>().
            AddTile(TileID.Bottles).
            Register()
            .DisableDecraft();
    }
}
