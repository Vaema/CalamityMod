using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.VanillaArmorChanges
{
    public class GoldArmorSetChange : VanillaArmorChange
    {
        public override int? HeadPieceID => ItemID.GoldHelmet;

        public override int? BodyPieceID => ItemID.GoldChainmail;

        public override int? LegPieceID => ItemID.GoldGreaves;

        public override int[] AlternativeHeadPieceIDs => new int[] { ItemID.AncientGoldHelmet };

        public override string ArmorSetName => "Gold";

        public const float GoldDropChanceFromEnemies = 0.02f;
        public const int GoldFromBosses = 3;
        public const float SetBonusCritPerGoldCoin = 0.2f; // 5 gold coins = +1% crit chance
        public const float MaximumCritBonus = 5f;

        public override void UpdateSetBonusText(ref string setBonusText)
        {
            setBonusText += $"\n{CalamityUtils.GetText($"Vanilla.Armor.SetBonus.{ArmorSetName}").Format(GoldDropChanceFromEnemies.ToPercent(), GoldFromBosses, SetBonusCritPerGoldCoin, MaximumCritBonus)}";
        }

        public override void ApplyArmorSetBonus(Player player)
        {
            player.Calamity().goldArmorGoldDrops = true;
            float critFromGold;

            // Give the crit chance from gold in inventory.
            // If you have any platinum, this guarantees the max boost.
            if (player.InventoryHas(ItemID.PlatinumCoin))
                critFromGold = MaximumCritBonus;
            else
            {
                // 13FEB2024: Ozzatron: this function doesn't cap at its second argument, it just stops counting if it exceeds that number
                // so this can give up to 100 gold coins
                int goldCoins = player.CountItem(ItemID.GoldCoin, 90);
                critFromGold = Math.Min(goldCoins * SetBonusCritPerGoldCoin, MaximumCritBonus);
            }

            player.GetCritChance<GenericDamageClass>() += critFromGold;
        }
    }
}
