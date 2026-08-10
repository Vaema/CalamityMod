using Terraria;
using Terraria.ID;

namespace CalamityMod.Items.VanillaArmorChanges;

public class LeadArmorSetChange : VanillaArmorChange
{
    public override int? HeadPieceID => ItemID.LeadHelmet;

    public override int? BodyPieceID => ItemID.LeadChainmail;

    public override int? LegPieceID => ItemID.LeadGreaves;

    public override string ArmorSetName => "Lead";

    public override void UpdateSetBonusText(ref string setBonusText)
    {
        setBonusText += $"\n{CalamityUtils.GetTextValue($"Vanilla.Armor.SetBonus.{ArmorSetName}")}";
    }

    public override void ApplyArmorSetBonus(Player player)
    {
        player.noKnockback = true;
    }
}
