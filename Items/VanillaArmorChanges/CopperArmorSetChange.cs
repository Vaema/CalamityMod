using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.VanillaArmorChanges;

public class CopperArmorSetChange : VanillaArmorChange
{
    public override int? HeadPieceID => ItemID.CopperHelmet;

    public override int? BodyPieceID => ItemID.CopperChainmail;

    public override int? LegPieceID => ItemID.CopperGreaves;

    public override string ArmorSetName => "Copper";

    public const float SetBonusFlatDamage = 2.0f;

    public override void UpdateSetBonusText(ref string setBonusText)
    {
        setBonusText += $"\n{CalamityUtils.GetText($"Vanilla.Armor.SetBonus.{ArmorSetName}").Format(SetBonusFlatDamage.ToString("N0"))}";
    }

    public override void ApplyArmorSetBonus(Player player)
    {
        player.GetDamage<GenericDamageClass>().Flat += SetBonusFlatDamage;
    }
}
