using CalamityMod.Items.Materials;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Mounts
{
    public class Brimrose : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Mounts";
        public override void SetDefaults()
        {
            Item.width = 64;
            Item.height = 64;
            Item.useAnimation = Item.useTime = 20;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.UseSound = SoundID.Item3;
            Item.noMelee = true;
            Item.mountType = ModContent.MountType<BrimroseChair>();

            Item.value = Item.sellPrice(gold: 5);
            Item.rare = ItemRarityID.Yellow;
            Item.Calamity().devItem = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<UnholyCore>(10).
                AddIngredient<Bloodstone>(20).
                AddTile(TileID.MythrilAnvil).
                Register();
        }
    }
}
