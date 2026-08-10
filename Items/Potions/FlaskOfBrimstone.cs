using CalamityMod.Buffs.Potions;
using CalamityMod.Items.Materials;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Potions;

[LegacyName("CalamitasBrew")]
public class FlaskOfBrimstone : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Potions";
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 20;
        ItemID.Sets.DrinkParticleColors[Type] = new Color[3] {
            new Color(240, 95, 64),
            new Color(227, 52, 68),
            new Color(189, 112, 123)
        };
    }

    public override void SetDefaults()
    {
        Item.DefaultToFood(30, 34, ModContent.BuffType<WeaponImbueBrimstone>(), CalamityUtils.MinutesToFrames(20), true);
        Item.value = Item.sellPrice(silver: 5);
        Item.rare = ItemRarityID.LightRed;
    }

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient(ItemID.BottledWater).
            AddIngredient<AshesofCalamity>().
            AddTile(TileID.ImbuingStation).
            Register();
    }
}
