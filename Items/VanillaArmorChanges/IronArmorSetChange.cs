using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.VanillaArmorChanges;

public class IronArmorSetChange : VanillaArmorChange
{
    public override int? HeadPieceID => ItemID.IronHelmet;

    public override int? BodyPieceID => ItemID.IronChainmail;

    public override int? LegPieceID => ItemID.IronGreaves;

    public override int[] AlternativeHeadPieceIDs => new int[] { ItemID.AncientIronHelmet };

    public override string ArmorSetName => "Iron";

    public const float KnockbackMultiplier = 1.5f;

    public override void UpdateSetBonusText(ref string setBonusText)
    {
        setBonusText += $"\n{CalamityUtils.GetText($"Vanilla.Armor.SetBonus.{ArmorSetName}").Format(KnockbackMultiplier)}";
    }

    public override void ApplyArmorSetBonus(Player player)
    {
        player.GetKnockback<GenericDamageClass>() *= KnockbackMultiplier;
    }
}
