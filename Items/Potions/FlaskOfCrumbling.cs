using CalamityMod.Buffs.Potions;
using CalamityMod.Items.Materials;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Potions;

[LegacyName("CrumblingPotion")]
public class FlaskOfCrumbling : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Potions";
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 20;
        ItemID.Sets.DrinkParticleColors[Type] = new Color[3] {
            new(243, 205, 45),
            new(192, 97, 38),
            new(225, 162, 58)
        };
    }

    public override void SetDefaults()
    {
        Item.DefaultToFood(32, 28, ModContent.BuffType<WeaponImbueCrumbling>(), CalamityUtils.MinutesToFrames(20), true);
        Item.value = Item.sellPrice(silver: 5);
        Item.rare = ItemRarityID.LightRed;
    }

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient(ItemID.BottledWater).
            AddIngredient<EssenceofSunlight>(2).
            AddTile(TileID.ImbuingStation).
            Register();
    }
}
