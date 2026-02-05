using CalamityMod.Buffs.DamageOverTime;
using CalamityMod.CalPlayer;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.Accessories
{
    public class BloodPact : ModItem, ILocalizedModType
    {
        public new string LocalizationCategory => "Items.Accessories";
        public override void SetDefaults()
        {
            Item.width = 26;
            Item.height = 26;
            Item.rare = ItemRarityID.Yellow;
            Item.value = CalamityGlobalItem.RarityYellowBuyPrice;
            Item.accessory = true;
            Item.expert = true;
        }

        public override void UpdateAccessory(Player player, bool hideVisual)
        {
            CalamityPlayer modPlayer = player.Calamity();
            modPlayer.bloodPact = true; // This applies the +25% health boost.
            modPlayer.healingPotionMultiplier += 0.33f;
            //Grants immunity to most Bleeding debuffs
            player.buffImmune[BuffID.Bleeding] = true;
            player.buffImmune[ModContent.BuffType<BurningBlood>()] = true;
            player.buffImmune[ModContent.BuffType<HeavyBleeding>()] = true; 
        }
    }
}
