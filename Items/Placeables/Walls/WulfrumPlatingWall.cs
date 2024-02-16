using Terraria.ModLoader;
using Terraria.ID;
using CalamityMod.Items.Placeables.FurnitureWulfrum;
using WallTiles = CalamityMod.Walls;

namespace CalamityMod.Items.Placeables.Walls
{
    public class WulfrumPlatingWall : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 400;
        }

        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 32;
            Item.maxStack = 9999;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useAnimation = 15;
            Item.useTime = 7;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.consumable = true;
            Item.createWall = ModContent.WallType<WallTiles.WulfrumPlatingWall>();
        }

        public override void AddRecipes()
        {
            CreateRecipe(4).
                AddIngredient<WulfrumPlating>().
                AddTile(TileID.WorkBenches).
                Register();
        }
    }
}
