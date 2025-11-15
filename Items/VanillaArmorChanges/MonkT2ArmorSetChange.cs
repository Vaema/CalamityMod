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

        public static float SetBonusRogueStealth = 0.9f;

        public override void UpdateSetBonusText(ref string setBonusText)
        {
            setBonusText = CalamityUtils.GetText($"Vanilla.Armor.SetBonus.{ArmorSetName}").Format(SetBonusRogueStealth.ToStealth()) + "\n" + setBonusText;
        }

        public override void ApplyHeadPieceEffect(Player player)
        {
            player.GetAttackSpeed<MeleeDamageClass>() -= 0.2f;
            player.Calamity().rogueVelocity += 0.15f; // Replace melee speed with rogue velocity.
        }

        public override void ApplyBodyPieceEffect(Player player) 
        {
            player.GetDamage<MeleeDamageClass>() -= 0.2f;
            player.GetDamage<RogueDamageClass>() += 0.15f; // Replace melee damage with rogue damage.
            player.GetDamage<SummonDamageClass>() -= 0.05f; // Small nerf to summon damage.
        }

        public override void ApplyLegPieceEffect(Player player)
        {
            player.GetCritChance<MeleeDamageClass>() -= 15;
            player.GetCritChance<RogueDamageClass>() += 15; // Replace melee crit chance with rogue crit chance.
        }

        public override void ApplyArmorSetBonus(Player player)
        {
            player.Calamity().rogueStealthMax += SetBonusRogueStealth; // Give rogue stealth.
            player.Calamity().wearingRogueArmor = true;
        }
    }
}
