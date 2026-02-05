using System.Collections.Generic;
using CalamityMod.CalPlayer;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Items.PermanentBoosters
{
    [LegacyName("MLGRune2")]
    public class CelestialOnion : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Misc";
        public override void SetDefaults()
        {
            Item.width = 28;
            Item.height = 28;
            Item.consumable = true;
            Item.maxStack = Item.CommonMaxStack;
            Item.useAnimation = Item.useTime = 30;
            Item.UseSound = SoundID.Item4;
            Item.useStyle = ItemUseStyleID.HoldUp;
            Item.value = Item.sellPrice(gold: 2);
            Item.rare = ItemRarityID.Red;
        }

        public static bool HasConsumedBefore(Player player)
        {
            return (Main.masterMode && player.extraAccessory) || player.Calamity().extraAccessoryML;
        }

        public override bool CanUseItem(Player player)
        {
            if (HasConsumedBefore(player))
            {
                if (player.whoAmI == Main.myPlayer)
                {
                    string key = "Mods.CalamityMod.Misc.CelestialOnionText";
                    Color messageColor = Color.LightSlateGray;
                    Main.NewText(Language.GetTextValue(key), messageColor);
                }
                return false;
            }

            return true;
        }

        public override bool? UseItem(Player player)
        {
            CalamityPlayer modPlayer = player.Calamity();
            // In Master Mode, will enable Demon Heart's accessory slot if for whatever reason you don't have that yet
            if (Main.masterMode)
            {
                if (player.itemAnimation > 0 && !player.extraAccessory && player.itemTime == 0)
                {
                    player.itemTime = Item.useTime;
                    player.extraAccessory = true;
                }
            }

            else if (player.itemAnimation > 0 && !modPlayer.extraAccessoryML && player.itemTime == 0)
            {
                player.itemTime = Item.useTime;
                modPlayer.extraAccessoryML = true;
            }
            return true;
        }

        public override void ModifyTooltips(List<TooltipLine> list)
        {
            if (HasConsumedBefore(Main.LocalPlayer))
                list.AddConsumedTooltip();
        }
    }
}
