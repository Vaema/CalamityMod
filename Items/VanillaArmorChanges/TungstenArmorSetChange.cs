using Terraria;
using Terraria.ID;

namespace CalamityMod.Items.VanillaArmorChanges;

public class TungstenArmorSetChange : VanillaArmorChange
{
    public override int? HeadPieceID => ItemID.TungstenHelmet;

    public override int? BodyPieceID => ItemID.TungstenChainmail;

    public override int? LegPieceID => ItemID.TungstenGreaves;

    public override string ArmorSetName => "Tungsten";

    public const float HookBoost = 0.5f;

    public override void UpdateSetBonusText(ref string setBonusText)
    {
        setBonusText += $"\n{CalamityUtils.GetText($"Vanilla.Armor.SetBonus.{ArmorSetName}").Format(HookBoost.ToPercent())}";
    }

    public override void ApplyArmorSetBonus(Player player)
    {
        player.Calamity().tungstenArmorHookBoost = true;
    }
}
