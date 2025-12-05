using Terraria;
using Terraria.ID;

namespace CalamityMod.Items.VanillaArmorChanges
{
    public class RainArmorSetChange : VanillaArmorChange
    {
        public override int? HeadPieceID => ItemID.RainHat;
        public override int? BodyPieceID => ItemID.RainCoat;
        public override int? LegPieceID => null;

        public override string ArmorSetName => "Rain";

        public override void UpdateSetBonusText(ref string setBonusText)
        {
            setBonusText = $"{CalamityUtils.GetTextValue($"Vanilla.Armor.SetBonus.{ArmorSetName}")}";
        }

        public override void ApplyArmorSetBonus(Player player)
        {
            player.Calamity().rainSet = true;
            player.autoJump = true;
            player.jumpSpeedBoost += 1.2f; // 24%
        }
    }
}
