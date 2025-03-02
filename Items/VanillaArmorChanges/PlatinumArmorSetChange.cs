using System;
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
        public const float ChestCrit = 3f;
        public const float LegsMoveSpeed = 0.1f;
        public const float SetBonusDamagePerDefense = 0.1f; // 10 defense = +1% damage, this gets divided by 100
        public const int SetBonusDefenseCap = 50;

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
            // CIT 1MAR2025: Platinum armor should not count its set bonus defense here, because it hasn't been applied yet
            int defenseBesidesThisArmor = player.statDefense - (5 + 6 + 5);
            if (defenseBesidesThisArmor <= 0)
                return;

            if (defenseBesidesThisArmor > SetBonusDefenseCap)
                defenseBesidesThisArmor = SetBonusDefenseCap;
            player.GetDamage<GenericDamageClass>() += (float)Math.Floor(defenseBesidesThisArmor * SetBonusDamagePerDefense) * 0.01f;
        }
    }
}
