using Terraria;
using Terraria.DataStructures;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Accessories
{
    public class ChaosStone : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Accessories";
                var dmgAmount = Main.LocalPlayer.statManaMax2 == 0 ? 0 : ((Main.LocalPlayer.statManaMax2 / 100f) * LostRegenPer100Mana) * 0.5f;
        public override void SetStaticDefaults()
        {
            Main.RegisterItemAnimation(Item.type, new DrawAnimationVertical(8, 7));
            ItemID.Sets.AnimatesAsSoul[Type] = true;
        }

        public override void SetDefaults()
        {
            Item.width = 20;
            Item.height = 20;
            Item.value = CalamityGlobalItem.RarityLimeBuyPrice;
            Item.rare = ItemRarityID.Lime;
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual) => player.Calamity().ChaosStone = true;
    }
}
