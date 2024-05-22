using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Accessories
{
    public class PermafrostsConcoction : ModItem, ILocalizedModType
    {
        // Boosted by Cross Necklace.
        internal static readonly int EncasedIFrames = 90;
        
        public new string LocalizationCategory => "Items.Accessories";
        public override void SetDefaults()
        {
            Item.width = 36;
            Item.height = 34;
            Item.accessory = true;
            Item.value = CalamityGlobalItem.RarityPinkBuyPrice;
            Item.rare = ItemRarityID.Pink;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.Calamity().permafrostsConcoction = true;
        }
    }
}
