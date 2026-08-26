using CalamityMod.Buffs.Potions;
using CalamityMod.Items.Materials;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Potions;

public class ZergPotion : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Potions";
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 20;
        ItemID.Sets.DrinkParticleColors[Type] = new Color[3] {
            new(223, 135, 244),
            new(94, 74, 213),
            new(156, 217, 246)
        };
    }

    public override void SetDefaults()
    {
        Item.DefaultToFood(24, 34, ModContent.BuffType<Zerg>(), CalamityUtils.MinutesToFrames(7), true);
        Item.value = Item.sellPrice(silver: 2);
        Item.rare = ItemRarityID.LightRed;
    }

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient(ItemID.BottledWater).
            AddIngredient<PurifiedGel>(2).
            AddIngredient(ItemID.Fireblossom, 2).
            AddTile(TileID.AlchemyTable).
            AddConsumeIngredientCallback(Recipe.IngredientQuantityRules.Alchemy).
            Register();

        CreateRecipe().
            AddIngredient(ItemID.BottledWater).
            AddIngredient<BloodOrb>(5).
            AddIngredient<PurifiedGel>(2).
            AddTile(TileID.AlchemyTable).
            Register()
            .DisableDecraft();
    }
}
