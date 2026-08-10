using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.VanillaArmorChanges;

public class GladiatorArmorSetChange : VanillaArmorChange
{
    public override int? HeadPieceID => ItemID.GladiatorHelmet;

    public override int? BodyPieceID => ItemID.GladiatorBreastplate;

    public override int? LegPieceID => ItemID.GladiatorLeggings;

    public override string ArmorSetName => "Gladiator";

    public const int HelmetRogueDamageBoostPercent = 8;
    public const int ChestplateRogueCritBoostPercent = 3;
    public const int LeggingRogueVelocityBoostPercent = 10;
    public const float SetBonusRogueStealth = 0.6f;

    public override void UpdateSetBonusText(ref string setBonusText)
    {
        setBonusText += $"\n{CalamityUtils.GetText($"Vanilla.Armor.SetBonus.{ArmorSetName}").Format(SetBonusRogueStealth.ToStealth())}";
    }

    public override void ApplyHeadPieceEffect(Player player)
    {
        player.GetDamage<ThrowingDamageClass>() += HelmetRogueDamageBoostPercent * 0.01f;
    }

    public override void ApplyBodyPieceEffect(Player player)
    {
        player.GetCritChance<ThrowingDamageClass>() += ChestplateRogueCritBoostPercent;
    }

    public override void ApplyLegPieceEffect(Player player)
    {
        player.Calamity().rogueVelocity += LeggingRogueVelocityBoostPercent * 0.01f;
    }

    public override void ApplyArmorSetBonus(Player player)
    {
        player.Calamity().rogueStealthMax += SetBonusRogueStealth;
        player.Calamity().wearingRogueArmor = true;
    }
}
