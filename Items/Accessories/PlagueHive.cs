using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.CalPlayer;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Accessories
{
    public class PlagueHive : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Accessories";
        public override void SetDefaults()
        {
            Item.width = 42;
            Item.height = 48;
            Item.value = CalamityGlobalItem.RarityRedBuyPrice;
            Item.rare = ItemRarityID.Red;
            Item.accessory = true;
        }

        public override void AddRecipes()
        {
            CreateRecipe().
                AddIngredient<AlchemicalFlask>().
                AddIngredient(ItemID.HoneyComb).
                AddTile(TileID.LunarCraftingStation).
                Register();
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            CalamityPlayer modPlayer = player.Calamity();
            modPlayer.uberBees = true;
            modPlayer.alchFlask = true;
            player.buffImmune[ModContent.BuffType<Plague>()] = true;
        }
    }
}
