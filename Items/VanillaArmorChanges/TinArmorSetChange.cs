using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.VanillaArmorChanges;

public class TinArmorSetChange : VanillaArmorChange
{
    public override int? HeadPieceID => ItemID.TinHelmet;

    public override int? BodyPieceID => ItemID.TinChainmail;

    public override int? LegPieceID => ItemID.TinGreaves;

    public override string ArmorSetName => "Tin";

    public const float SetBonusCrit = 10f;

    public override void UpdateSetBonusText(ref string setBonusText)
    {
        setBonusText += $"\n{CalamityUtils.GetText($"Vanilla.Armor.SetBonus.{ArmorSetName}").Format(SetBonusCrit)}";
    }

    public override void ApplyArmorSetBonus(Player player)
    {
        player.GetCritChance<GenericDamageClass>() += SetBonusCrit;
    }
}
