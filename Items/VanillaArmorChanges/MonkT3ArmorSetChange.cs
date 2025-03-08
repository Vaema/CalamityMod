using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.VanillaArmorChanges
{
    public class MonkT3ArmorSetChange : VanillaArmorChange
    {
        public override int? HeadPieceID => ItemID.MonkAltHead;

        public override int? BodyPieceID => ItemID.MonkAltShirt;

        public override int? LegPieceID => ItemID.MonkAltPants;

        public override string ArmorSetName => "MonkTier3";

        public override void UpdateSetBonusText(ref string setBonusText)
        {
            setBonusText += $"\n{CalamityUtils.GetTextValue($"Vanilla.Armor.SetBonus.{ArmorSetName}")}";
        }

        public override void ApplyHeadPieceEffect(Player player)
        {
            player.GetDamage<SummonDamageClass>() -= 0.1f; // Decrease both damage boosts to 10%.
            player.GetDamage<MeleeDamageClass>() -= 0.2f;
            player.GetDamage<RogueDamageClass>() += 0.1f; // Replace melee damage with rogue damage.
        }

        public override void ApplyBodyPieceEffect(Player player) 
        {
            player.GetDamage<SummonDamageClass>() -= 0.1f; // Decrease summon damage boost to 10%.
            player.GetAttackSpeed<MeleeDamageClass>() -= 0.2f; // Replace melee speed with rogue velocity, and decrease to 15%.
            player.Calamity().rogueVelocity += 0.15f;
            player.GetCritChance<MeleeDamageClass>() -= 5; // Replace melee crit with rogue crit.
            player.GetCritChance<RogueDamageClass>() += 5;
        }

        public override void ApplyLegPieceEffect(Player player)
        {
            player.GetDamage<SummonDamageClass>() -= 0.1f; // Decrease summon damage boost to 10%.
            player.GetCritChance<MeleeDamageClass>() -= 20; // Replace melee crit chance with rogue, and decrease to 10%.
            player.GetCritChance<RogueDamageClass>() += 10;
        }

        public override void ApplyArmorSetBonus(Player player)
        {
            player.GetDamage<SummonDamageClass>() += 0.2f; // Re-add 20% of the 30% lost summon damage, and the rest of the rogue damage.
            player.GetDamage<RogueDamageClass>() += 0.1f;
            player.Calamity().rogueStealthMax += 1f; // Give rogue stealth.
            player.Calamity().wearingRogueArmor = true;
        }
    }
}
