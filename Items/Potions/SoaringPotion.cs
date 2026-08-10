using CalamityMod.Buffs.Potions;
using CalamityMod.Items.Fishing.SunkenSeaCatches;
using CalamityMod.Items.Materials;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Potions;

public class SoaringPotion : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Potions";

    public static float FlightTimeBoost = 0.1f;
    public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(FlightTimeBoost.ToPercent());

    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 20;
        ItemID.Sets.DrinkParticleColors[Type] = new Color[3] {
            new Color(85, 181, 217),
            new Color(190, 237, 232),
            new Color(81, 114, 173)
        };
    }

    public override void SetDefaults()
    {
        Item.DefaultToFood(30, 30, ModContent.BuffType<Soaring>(), CalamityUtils.MinutesToFrames(8), true);
        Item.value = Item.sellPrice(silver: 2);
        Item.rare = ItemRarityID.LightRed;
    }

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient(ItemID.BottledWater).
            AddIngredient<SunkenSailfish>().
            AddIngredient(ItemID.SoulofFlight).
            AddTile(TileID.AlchemyTable).
            AddConsumeIngredientCallback(Recipe.IngredientQuantityRules.Alchemy).
            Register();

        CreateRecipe().
            AddIngredient(ItemID.BottledWater).
            AddIngredient<BloodOrb>(15).
            AddIngredient(ItemID.SoulofFlight).
            AddTile(TileID.AlchemyTable).
            Register()
            .DisableDecraft();
    }
}
