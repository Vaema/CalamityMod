using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.VanillaArmorChanges
{
    public class MonkT2ArmorSetChange : VanillaArmorChange
    {
        public override int? HeadPieceID => ItemID.MonkBrows;

        public override int? BodyPieceID => ItemID.MonkShirt;

        public override int? LegPieceID => ItemID.MonkPants;

        public override string ArmorSetName => "MonkTier2";

        public override void UpdateSetBonusText(ref string setBonusText)
        {
            setBonusText += $"\n{CalamityUtils.GetTextValue($"Vanilla.Armor.SetBonus.{ArmorSetName}")}";
        }

        public override void ApplyHeadPieceEffect(Player player)
        {
            player.GetAttackSpeed<MeleeDamageClass>() -= 0.2f;
            player.Calamity().rogueVelocity += 0.1f; // Replace melee speed with rogue velocity.
        }

        public override void ApplyBodyPieceEffect(Player player) 
        {
            player.GetDamage<SummonDamageClass>() -= 0.15f; // Decrease both damage boosts to 5%.
            player.GetDamage<MeleeDamageClass>() -= 0.2f;
            player.GetDamage<RogueDamageClass>() += 0.05f; // Replace melee damage with rogue damage.
        }

        public override void ApplyLegPieceEffect(Player player)
        {
            player.GetDamage<SummonDamageClass>() -= 0.05f; // Decrease summon damage boost to 5%.
            player.GetCritChance<MeleeDamageClass>() -= 15; // Replace melee crit chance with rogue and decrease to 5%.
            player.GetCritChance<RogueDamageClass>() += 5;
        }

        public override void ApplyArmorSetBonus(Player player)
        {
            player.GetDamage<SummonDamageClass>() += 0.15f; // Re-add 15% of the 20% lost summon damage, 10% of the 20% rogue damage, and the rest of the rogue crit.
            player.GetDamage<RogueDamageClass>() += 0.1f;
            player.GetCritChance<RogueDamageClass>() += 10;
            player.Calamity().rogueStealthMax += 0.9f; // Give rogue stealth.
            player.Calamity().wearingRogueArmor = true;
        }
    }
}
