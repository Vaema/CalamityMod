using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Accessories
{
    public class SandCloak : ModItem, ILocalizedModType
    {
        public static float KnockbackStrength = 0.3f;

        public new string LocalizationCategory => "Items.Accessories";
        public override void SetDefaults()
        {
            Item.width = 30;
            Item.height = 44;
            Item.value = CalamityGlobalItem.RarityGreenBuyPrice;
            Item.rare = ItemRarityID.Green;
            Item.accessory = true;
            Item.defense = 2;
        }

        public override void ModifyTooltips(List<TooltipLine> list) => list.IntegrateHotkey(CalamityKeybinds.SandCloakHotkey);

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            player.moveSpeed += 0.05f;
            player.Calamity().sandCloak = true;
        }
    }
}
