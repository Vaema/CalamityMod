using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Mounts;

public class TundraLeash : ModItem, ILocalizedModType
{
    public new string LocalizationCategory => "Items.Mounts";
    public override void SetDefaults()
    {
        Item.width = 16;
        Item.height = 16;
        Item.useAnimation = Item.useTime = 20;
        Item.useStyle = ItemUseStyleID.HoldUp;
        Item.UseSound = SoundID.NPCHit56;
        Item.noMelee = true;
        Item.mountType = ModContent.MountType<RimehoundMount>();

        Item.value = Item.sellPrice(gold: 5);
        Item.rare = ItemRarityID.Yellow;
    }
}
