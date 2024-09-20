using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Dyes.HairDye
{
    public class StealthHairDye : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Dyes";
        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 26;
            Item.useAnimation = Item.useTime = 17;
            Item.UseSound = SoundID.Item3;
            Item.useStyle = ItemUseStyleID.DrinkLiquid;
            Item.useTurn = true;
            Item.consumable = true;
            Item.maxStack = 9999;

            Item.value = Item.buyPrice(gold: 5); // Sold by Stylist
            Item.rare = ItemRarityID.Green;
        }
    }
}
