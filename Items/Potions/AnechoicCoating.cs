using CalamityMod.Buffs.Potions;
using CalamityMod.Items.Materials;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Potions;

public class AnechoicCoating : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Potions";
    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 20;
        ItemID.Sets.DrinkParticleColors[Type] = new Color[2] {
            new(118, 182, 199),
            new(150, 227, 230)
        };
    }

    public override void SetDefaults()
    {
        // How do you even use this?
        Item.DefaultToFood(22, 26, ModContent.BuffType<AnechoicCoatingBuff>(), CalamityUtils.MinutesToFrames(4));
        Item.UseSound = SoundID.Item3;
        Item.value = Item.sellPrice(silver: 2);
        Item.rare = ItemRarityID.Blue;
    }

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient(ItemID.BottledWater).
            AddIngredient<BloodOrb>(10).
            AddTile(TileID.AlchemyTable).
            Register()
            .DisableDecraft();
    }
}
