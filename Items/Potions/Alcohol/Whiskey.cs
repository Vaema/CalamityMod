using System;
using CalamityMod.Buffs.Alcohol;
using CalamityMod.Items.Accessories;
using CalamityMod.Items.Materials;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.Potions.Alcohol
{
    public class Whiskey : ModItem, ILocalizedModType, IAlcoholItem
    {
        public new string LocalizationCategory => "Items.Potions";

        public static float MaxDamageCeiling = 0.15f;
        public static float MinDamageFloor = -0.15f;
        public static float TimeToDischarge = 600;
        public static float TimeToRecharge = 300;
        public static float IVDripDamageCeiling = 0.3f;
        public static float IVDripDamageFloor = -0.3f;

        public override LocalizedText Tooltip => base.Tooltip.WithFormatArgs(MinDamageFloor.ToPercent(), MaxDamageCeiling.ToPercent(), (TimeToDischarge/60).ToString("0.##"), (TimeToRecharge/60).ToString("0.##"));
        public LocalizedText DripEffectText => Language.GetText("Mods.CalamityMod.Items.Potions.Whiskey.DripEffect").WithFormatArgs(MinDamageFloor.ToPercent(), MaxDamageCeiling.ToPercent(), (TimeToDischarge / 60).ToString("0.##"), (TimeToRecharge / 60).ToString("0.##"), MaxDamageCeiling.ToPercent(), MinDamageFloor.ToPercent());
        public AlcoholType AlcoholVariant => AlcoholType.Whiskey;

        public Action<Player, float> IVDripAlcoholEffect => ApplyWhiskeyEffect;

        private static void ApplyWhiskeyEffect(Player player, float intensity)
        {
            // out of order 20 defense
        }
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 20;
            ItemID.Sets.DrinkParticleColors[Type] = new Color[3] {
                new Color(227, 148, 11),
                new Color(235, 177, 5),
                new Color(250, 190, 12)
            };
        }

        public override void SetDefaults()
        {
            Item.DefaultToFood(18, 32, ModContent.BuffType<WhiskeyBuff>(), CalamityUtils.MinutesToFrames(6), true);

            Item.value = Item.sellPrice(silver: 2);
            Item.rare = ItemRarityID.Blue;
        }
        public override void AddRecipes()
        {
            CreateRecipe(10).
                AddIngredient(ItemID.Bottle, 10).
                AddIngredient(ItemID.SpicyPepper).
                AddTile(TileID.Kegs).
                Register();
        }
    }
}
