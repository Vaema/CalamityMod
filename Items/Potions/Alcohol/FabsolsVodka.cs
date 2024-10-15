using CalamityMod.Buffs.Alcohol;
using CalamityMod.Items.Accessories.Vanity;
using CalamityMod.Items.Materials;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Potions.Alcohol
{
    public class FabsolsVodka : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Potions";

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 5;
            ItemID.Sets.ShimmerTransformToItem[Type] = ModContent.ItemType<CrystalHeartVodka>();
            ItemID.Sets.DrinkParticleColors[Type] = new Color[3] {
                new Color(239, 123, 202),
                new Color(187, 56, 158),
                new Color(165, 47, 255)
            };
        }

        public override void SetDefaults()
        {
            Item.DefaultToFood(30, 42, ModContent.BuffType<FabsolVodkaBuff>(), CalamityUtils.SecondsToFrames(900f), true);
            // Cirrus overcharges: 10% sell value instead of 20%
            Item.value = Item.sellPrice(silver: 30);
            Item.rare = ItemRarityID.LightRed;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ItemID.Ale).
                AddIngredient(ItemID.PixieDust, 10).
                AddIngredient(ItemID.CrystalShard, 5).
                AddIngredient(ItemID.UnicornHorn).
                AddTile(TileID.Kegs).
                Register();

            CreateRecipe().
                AddIngredient(ItemID.Ale).
                AddIngredient<BloodOrb>(15).
                AddIngredient(ItemID.CrystalShard).
                AddTile(TileID.AlchemyTable).
                Register()
                .DisableDecraft();
        }
    }
}
