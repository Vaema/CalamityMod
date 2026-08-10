using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.VanillaArmorChanges;

public class MonkT3ArmorSetChange : VanillaArmorChange
{
    public override int? HeadPieceID => ItemID.MonkAltHead;

    public override int? BodyPieceID => ItemID.MonkAltShirt;

    public override int? LegPieceID => ItemID.MonkAltPants;

    public override string ArmorSetName => "MonkTier3";

    public static float SetBonusRogueStealth = 1.1f;

    public override void UpdateSetBonusText(ref string setBonusText)
    {
        setBonusText = CalamityUtils.GetText($"Vanilla.Armor.SetBonus.{ArmorSetName}").Format(SetBonusRogueStealth.ToStealth()) + "\n" + setBonusText;
    }

    public override void ApplyHeadPieceEffect(Player player)
    {
        player.GetDamage<MeleeDamageClass>() -= 0.2f;
        player.GetDamage<RogueDamageClass>() += 0.2f; // Replace melee damage with rogue damage.
    }

    public override void ApplyBodyPieceEffect(Player player) 
    {
        player.GetAttackSpeed<MeleeDamageClass>() -= 0.2f; 
        player.Calamity().rogueVelocity += 0.2f; // Replace melee speed with rogue velocity.
        player.GetCritChance<MeleeDamageClass>() -= 5;
        player.GetCritChance<RogueDamageClass>() += 5; // Replace melee crit with rogue crit.
    }

    public override void ApplyLegPieceEffect(Player player)
    {
        player.GetCritChance<MeleeDamageClass>() -= 20;
        player.GetCritChance<RogueDamageClass>() += 20; // Replace melee crit with rogue crit.
    }

    public override void ApplyArmorSetBonus(Player player)
    {
        player.Calamity().rogueStealthMax += SetBonusRogueStealth; // Give rogue stealth.
        player.Calamity().wearingRogueArmor = true;
    }
}
