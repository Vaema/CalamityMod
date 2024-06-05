using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Placeables
{
    public class OrangeCoral : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Placeables";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 100;
        }

        public override void SetDefaults()
        {
            Item.width = 12;
            Item.height = 12;
            Item.rare = ItemRarityID.Blue;
            Item.maxStack = 9999;
            Item.useTurn = true;
            Item.autoReuse = true;
            Item.useAnimation = 15;
            Item.useTime = 10;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.consumable = true;
            Item.createTile = ModContent.TileType<Tiles.SunkenSea.OrangeCoral>();
        }

        //public override void AddRecipes()
        //{
        //    CreateRecipe(4).
        //        AddIngredient(ItemID.PinkPearl).
        //        AddTile(TileID.HeavyWorkBench).
        //        Register();
        //}
    }
}
