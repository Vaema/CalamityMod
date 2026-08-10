using CalamityMod.Buffs.Potions;
using CalamityMod.Items.Materials;
using CalamityMod.Items.Placeables.Abyss;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Potions;

public class SulphurskinPotion : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Potions";
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 20;
        ItemID.Sets.DrinkParticleColors[Type] = new Color[3] {
            new Color(133, 180, 49),
            new Color(80, 139, 81),
            new Color(117, 95, 133)
        };
    }

    public override void SetDefaults()
    {
        Item.DefaultToFood(22, 26, ModContent.BuffType<SulphurskinBuff>(), CalamityUtils.MinutesToFrames(4), true);
        Item.value = Item.sellPrice(silver: 2);
        Item.rare = ItemRarityID.Green;
    }

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient(ItemID.BottledWater).
            AddIngredient<SulphurousSand>().
            AddIngredient(ItemID.Waterleaf).
            AddTile(TileID.Bottles).
            Register();

        CreateRecipe().
            AddIngredient(ItemID.BottledWater).
            AddIngredient<BloodOrb>(5).
            AddTile(TileID.AlchemyTable).
            Register()
            .DisableDecraft();
    }
}
