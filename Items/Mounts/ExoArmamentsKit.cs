using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Mounts
{
    public class ExoArmamentsKit : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Mounts";
        public override void SetDefaults()
        {
            Item.width = 50;
            Item.height = 44;
            Item.useAnimation = Item.useTime = 20;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.UseSound = SoundID.Item94;
            Item.noMelee = true;
            Item.mountType = ModContent.MountType<ExoTank>();

            Item.value = Item.sellPrice(gold: 5);
            Item.master = true;
        }
    }
}
