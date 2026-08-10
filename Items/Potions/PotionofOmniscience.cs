using CalamityMod.Buffs.Potions;
using CalamityMod.Items.Materials;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Potions;

public class PotionofOmniscience : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Potions";
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 20;
        ItemID.Sets.DrinkParticleColors[Type] = new Color[3] {
            new Color(159, 67, 199),
            new Color(176, 147, 243),
            new Color(84, 50, 185)
        };
    }

    public override void SetDefaults()
    {
        Item.DefaultToFood(28, 30, ModContent.BuffType<Omniscience>(), CalamityUtils.MinutesToFrames(15), true);
        Item.value = Item.sellPrice(silver: 2);
        Item.rare = ItemRarityID.Orange;
    }

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient(ItemID.HunterPotion).
            AddIngredient(ItemID.SpelunkerPotion).
            AddIngredient(ItemID.TrapsightPotion).
            AddTile(TileID.AlchemyTable).
            Register();

        CreateRecipe().
            AddIngredient(ItemID.BottledWater).
            AddIngredient<BloodOrb>(20).
            AddTile(TileID.AlchemyTable).
            Register()
            .DisableDecraft();
    }
}
