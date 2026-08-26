using CalamityMod.Buffs.Potions;
using CalamityMod.Items.Materials;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Potions;

[LegacyName("HolyWrathPotion", "ProfanedRagePotion")]
public class FlaskOfHolyFlames : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Potions";
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 20;
        ItemID.Sets.DrinkParticleColors[Type] = new Color[3] {
            new(252, 23, 23),
            new(199, 14, 51),
            new(143, 36, 72)
        };
    }

    public override void SetDefaults()
    {
        Item.DefaultToFood(44, 36, ModContent.BuffType<WeaponImbueHolyFlames>(), CalamityUtils.MinutesToFrames(20), true);
        Item.value = Item.sellPrice(silver: 5);
        Item.rare = ItemRarityID.Purple;
    }

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient(ItemID.BottledWater).
            AddIngredient<UnholyEssence>().
            AddTile(TileID.ImbuingStation).
            Register();
    }
}
