using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Accessories
{
    public class GladiatorsLocket : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Accessories";
        public override void SetDefaults()
        {
            Item.width = 48;
            Item.height = 54;
            Item.value = CalamityGlobalItem.RarityOrangeBuyPrice;
            Item.rare = ItemRarityID.Orange;
            Item.accessory = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            float statPower = (float)Math.Round(0.2f * Utils.GetLerpValue(1, 0.5f, ((float)player.statLife / (float)player.statLifeMax2), true), 2);
            player.Calamity().gladiatorSword = true;
            player.GetDamage<GenericDamageClass>() += statPower;
            player.moveSpeed += statPower;
        }
    }
}
