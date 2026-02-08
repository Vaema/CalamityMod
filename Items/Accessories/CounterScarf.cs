using CalamityMod.CalPlayer;
using CalamityMod.CalPlayer.Dashes;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Accessories
{
    [AutoloadEquip(EquipType.Neck)]
    public class CounterScarf : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Accessories";
        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 38;
            Item.value = Item.buyPrice(gold: 20); // Sold by Clothier
            Item.rare = ItemRarityID.Orange;
            Item.accessory = true;
        }
        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            CalamityPlayer modPlayer = player.Calamity();
            modPlayer.dodgeScarf = true;
            modPlayer.DashID = CounterScarfDash.ID;
            player.dashType = 0;
        }
    }
}
