using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Materials
{
    public class AnodizedWulfrumMetal : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Materials";

        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 25;
        }

        public override void SetDefaults()
        {
            Item.width = 18;
            Item.height = 18;
            Item.maxStack = Item.CommonMaxStack;
            Item.value = Item.sellPrice(copper: 10);
            Item.rare = ItemRarityID.Blue;
            Item.ammo = Item.type;
        }
        public override void AddRecipes()
        {
            CreateRecipe(1).
                AddIngredient<StormlionMandible>().
                AddIngredient<WulfrumMetalScrap>().
                AddTile(TileID.WorkBenches).
                Register();
        }
        //Added to avoid making the recipes really repetitive and nonsensical. It also makes the new metal color make sense. Adds a bit of flavor to the Wulfrum Recipes
        //Uses Stormlion Manible because in order to anodize a metal you must use a current, the other option was a Power Cell but I feel Stormlion Mandible needed more uses
        //More uses are planned for other blocks and furniture for wulfrum that use this new metal color and gives a bit more depth to the set
    }
}
