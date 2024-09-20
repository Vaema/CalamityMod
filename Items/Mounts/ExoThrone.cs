using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Mounts
{
    public class ExoThrone : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Mounts";
        public override void SetDefaults()
        {
            Item.width = 32;
            Item.height = 34;
            Item.useAnimation = Item.useTime = 20;
            Item.useStyle = ItemUseStyleID.Shoot;
            Item.UseSound = SoundID.Item94;
            Item.noMelee = true;
            Item.mountType = ModContent.MountType<DraedonGamerChairMount>();

            Item.value = Item.sellPrice(gold: 5);
            Item.rare = ItemRarityID.Yellow;
        }

        public override void ModifyTooltips(List<TooltipLine> list) => list.IntegrateHotkey(CalamityKeybinds.ExoChairSlowdownHotkey);
    }
}
