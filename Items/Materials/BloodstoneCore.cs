using CalamityMod.Rarities;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Materials
{
    public class BloodstoneCore : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Materials";
        public override void SetStaticDefaults()
        {
            Item.ResearchUnlockCount = 25;
            ItemID.Sets.SortingPriorityMaterials[Type] = 113;
        }

        public override void SetDefaults()
        {
            Item.width = 15;
            Item.height = 12;
            Item.maxStack = Item.CommonMaxStack;
            Item.value = Item.sellPrice(gold: 2);
            Item.rare = ModContent.RarityType<Turquoise>();
        }

        public override void AddRecipes()
        {
            CreateRecipe(2).
                AddIngredient<Bloodstone>(5).
                AddIngredient<BloodOrb>().
                AddIngredient<Necroplasm>().
                AddTile(TileID.MythrilAnvil).
                Register();
        }
    }
}
