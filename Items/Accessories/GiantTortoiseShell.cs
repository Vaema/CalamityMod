using CalamityMod.CalPlayer;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Accessories
{
    public class GiantTortoiseShell : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Accessories";
        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 24;
            Item.defense = 15;
            Item.value = CalamityGlobalItem.RarityLightRedBuyPrice;
            Item.rare = ItemRarityID.LightRed;
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            CalamityPlayer modPlayer = player.Calamity();
            player.noKnockback = true;
            modPlayer.tortShell = true;
        }
        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<GiantShell>().
                AddIngredient(ItemID.TurtleShell).
                AddTile(TileID.Anvils).
                Register();
        }
    }
}
