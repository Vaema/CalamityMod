using CalamityMod.Buffs.Potions;
using CalamityMod.Items.Materials;
using CalamityMod.Rarities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Potions;

public class CeaselessHungerPotion : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Potions";
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 20;
        ItemID.Sets.DrinkParticleColors[Type] = new Color[3] {
            new Color(12, 18, 28),
            new Color(110, 197, 212),
            new Color(158, 81, 153)
        };
    }

    public override void SetDefaults()
    {
        Item.DefaultToFood(22, 32, ModContent.BuffType<CeaselessHunger>(), CalamityUtils.SecondsToFrames(20), true);
        Item.value = Item.sellPrice(silver: 10);
        Item.rare = ModContent.RarityType<PureGreen>();
    }

    public override void AddRecipes()
    {
        CreateRecipe(4).
            AddIngredient(ItemID.BottledWater, 4).
            AddIngredient<DarkPlasma>().
            AddTile(TileID.AlchemyTable).
            AddConsumeIngredientCallback(Recipe.IngredientQuantityRules.Alchemy).
            Register();

        CreateRecipe(8).
            AddIngredient(ItemID.BottledWater, 8).
            AddIngredient<BloodOrb>(10).
            AddIngredient<DarkPlasma>().
            AddTile(TileID.AlchemyTable).
            Register()
            .DisableDecraft();
    }
}
