using CalamityMod.Items.Materials;
using CalamityMod.Rarities;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Potions;

public class OmegaHealingPotion : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Potions";
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 30;
        ItemID.Sets.DrinkParticleColors[Type] = new Color[3] {
            new Color(255, 31, 25),
            new Color(162, 28, 25),
            new Color(159, 10, 111)
        };
    }

    public override void SetDefaults()
    {
        Item.DefaultToHealingPotion(24, 32, 300);
        Item.value = Item.sellPrice(gold: 1);
        Item.rare = ModContent.RarityType<CosmicPurple>();
    }

    public override void AddRecipes()
    {
        CreateRecipe(20).
            AddIngredient<SupremeHealingPotion>(20).
            AddIngredient<AscendantSpiritEssence>().
            AddTile(TileID.Bottles).
            Register()
            .DisableDecraft();
    }
}
