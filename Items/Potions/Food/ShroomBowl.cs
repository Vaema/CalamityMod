using CalamityMod.Items.Critters;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Potions.Food
{
    public class ShroomBowl : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Potions";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 5;
            Main.RegisterItemAnimation(Type, new DrawAnimationVertical(int.MaxValue, 3));
            ItemID.Sets.FoodParticleColors[Type] = [
                new Color(248, 195, 96),
                new Color(115, 57, 15),
                new Color(230, 124, 45)
            ];
            ItemID.Sets.IsFood[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.DefaultToFood(30, 24, BuffID.WellFed2, CalamityUtils.MinutesToFrames(10), true);
            Item.value = Item.sellPrice(silver: 10);
            Item.rare = ItemRarityID.Blue;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient(ItemID.Bowl).
                AddIngredient<ShroombleItem>().
                AddTile(TileID.CookingPots).
                DisableDecraft().
                Register();
        }
    }
}
