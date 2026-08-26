using CalamityMod.Items.Materials;
using CalamityMod.Rarities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Potions;

public class SupremeHealingPotion : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Potions";
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 30;
        ItemID.Sets.DrinkParticleColors[Type] = new Color[3] {
            new(255, 31, 25),
            new(217, 19, 15),
            new(255, 0, 221)
        };
    }

    public override void SetDefaults()
    {
        Item.DefaultToHealingPotion(26, 38, 250);
        Item.value = Item.sellPrice(silver: 60);
        Item.rare = ModContent.RarityType<Turquoise>();
    }

    public override void AddRecipes()
    {
        CreateRecipe(4).
            AddIngredient(ItemID.SuperHealingPotion, 4).
            AddIngredient<Bloodstone>(3).
            AddTile(TileID.Bottles).
            Register()
            .DisableDecraft();
    }
}
