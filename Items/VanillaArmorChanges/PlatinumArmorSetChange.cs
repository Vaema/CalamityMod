using Terraria;
using Terraria.ID;

namespace CalamityMod.Items.VanillaArmorChanges;

public class PlatinumArmorSetChange : VanillaArmorChange
{
    public override int? HeadPieceID => ItemID.PlatinumHelmet;

    public override int? BodyPieceID => ItemID.PlatinumChainmail;

    public override int? LegPieceID => ItemID.PlatinumGreaves;

    public override string ArmorSetName => "Platinum";

    public const float SetBonusDR = 0.1f;

    public override void UpdateSetBonusText(ref string setBonusText)
    {
        setBonusText += $"\n{CalamityUtils.GetText($"Vanilla.Armor.SetBonus.{ArmorSetName}").Format(SetBonusDR.ToPercent())}";
    }

    public override void ApplyArmorSetBonus(Player player)
    {
        player.endurance += SetBonusDR;
    }
}
