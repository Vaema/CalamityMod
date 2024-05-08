using System.Text;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.VanillaArmorChanges
{
    public class PlatinumArmorSetChange : VanillaArmorChange
    {
        public override int? HeadPieceID => ItemID.PlatinumHelmet;

        public override int? BodyPieceID => ItemID.PlatinumChainmail;

        public override int? LegPieceID => ItemID.PlatinumGreaves;

        public override string ArmorSetName => "Platinum";

        public const float HeadDamage = 0.06f;
        public const float ChestCrit = 5f;
        public const float LegsMoveSpeed = 0.1f;
        public const float SetBonusDamagePerDefense = 0.001f; // 10 defense = +1% damage
        public const float SetBonusCritPerDefense = 0.1f; // 10 defense = +1% crit chance
        public const int SetBonusDefenseCap = 40;

        public override void ApplyHeadPieceEffect(Player player) => player.GetDamage<GenericDamageClass>() += HeadDamage;

        public override void ApplyBodyPieceEffect(Player player) => player.GetCritChance<GenericDamageClass>() += ChestCrit;

        public override void ApplyLegPieceEffect(Player player) => player.moveSpeed += LegsMoveSpeed;

        public override void UpdateSetBonusText(ref string setBonusText)
        {
            setBonusText += $"\n{CalamityUtils.GetTextValue($"Vanilla.Armor.SetBonus.{ArmorSetName}")}";
        }

        public override void ApplyArmorSetBonus(Player player)
        {
            // 07MAY2024: Ozzatron: Platinum armor doesn't count its own defense for its set bonus
            int defenseBesidesThisArmor = player.statDefense - (4 + 6 + 4 + 4);
            if (defenseBesidesThisArmor <= 0)
                return;

            if (defenseBesidesThisArmor > SetBonusDefenseCap)
                defenseBesidesThisArmor = SetBonusDefenseCap;
            player.GetDamage<GenericDamageClass>() += defenseBesidesThisArmor * SetBonusDamagePerDefense;
            player.GetCritChance<GenericDamageClass>() += defenseBesidesThisArmor * SetBonusCritPerDefense;
        }
    }
}
