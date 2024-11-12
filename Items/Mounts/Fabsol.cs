using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Mounts
{
    public class Fabsol : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Mounts";
        public override void SetDefaults()
        {
            Item.width = 16;
            Item.height = 16;
            Item.useAnimation = Item.useTime = 20;
            Item.useStyle = ItemUseStyleID.Swing;
            Item.UseSound = SoundID.Item3;
            Item.noMelee = true;
            Item.mountType = ModContent.MountType<AlicornMount>();

            Item.value = Item.sellPrice(gold: 5);
            Item.rare = ItemRarityID.Yellow;
            Item.Calamity().devItem = true;
        }
    }
}
