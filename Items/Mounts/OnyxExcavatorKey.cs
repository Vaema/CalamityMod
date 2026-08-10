using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Mounts;

public class OnyxExcavatorKey : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Mounts";
    public override void SetDefaults()
    {
        Item.width = 16;
        Item.height = 16;
        Item.useAnimation = Item.useTime = 20;
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.UseSound = SoundID.Item23;
        Item.noMelee = true;
        Item.mountType = ModContent.MountType<OnyxExcavator>();

        Item.value = Item.sellPrice(gold: 5);
        Item.rare = ItemRarityID.Yellow;
    }
}
