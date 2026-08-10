using CalamityMod.Buffs.Potions;
using CalamityMod.Items.Fishing.BrimstoneCragCatches;
using CalamityMod.Items.Materials;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Potions;

public class ShadowPotion : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Potions";

    public static float StealthRegenBoost = 0.08f;
    public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(StealthRegenBoost.ToPercent());

    public override void SetStaticDefaults()
    {
        Item.ResearchUnlockCount = 20;
        ItemID.Sets.DrinkParticleColors[Type] = new Color[3] {
            new Color(45, 45, 45),
            new Color(0, 0, 0),
            new Color(95, 0, 36)
        };
    }

    public override void SetDefaults()
    {
        Item.DefaultToFood(20, 28, ModContent.BuffType<ShadowBuff>(), CalamityUtils.MinutesToFrames(8), true);
        Item.value = Item.sellPrice(silver: 2);
        Item.rare = ItemRarityID.Orange;
    }

    public override void AddRecipes()
    {
        CreateRecipe().
            AddIngredient(ItemID.BottledWater).
            AddIngredient<Shadowfish>().
            AddIngredient(ItemID.Blinkroot).
            AddTile(TileID.Bottles).
            Register();

        CreateRecipe().
            AddIngredient(ItemID.BottledWater).
            AddIngredient<BloodOrb>(10).
            AddTile(TileID.AlchemyTable).
            Register()
            .DisableDecraft();
    }
}
