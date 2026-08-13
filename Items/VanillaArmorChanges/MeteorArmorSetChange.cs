using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.VanillaArmorChanges;

public class MeteorArmorSetChange : VanillaArmorChange
{
    public override int? HeadPieceID => ItemID.MeteorHelmet;

    public override int? BodyPieceID => ItemID.MeteorSuit;

    public override int? LegPieceID => ItemID.MeteorLeggings;

    public override string ArmorSetName => "Meteor";

    public override void UpdateSetBonusText(ref string setBonusText)
    {
        setBonusText = $"{CalamityUtils.GetTextValue($"Vanilla.Armor.SetBonus.{ArmorSetName}")}";
    }

    public override void ApplyArmorSetBonus(Player player)
    {
        player.Calamity().meteorSet = true;
    }
}
