using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Accessories
{
    public class OldDie : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Accessories";
        public override void SetDefaults()
        {
            Item.width = 24;
            Item.height = 26;
            Item.rare = ItemRarityID.Orange;
            Item.value = Item.buyPrice(gold: 40); // Sold by Bandit
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual) => player.Calamity().calamityBonusLuck += 0.2f;
    }
}
