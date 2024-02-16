using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.VanillaArmorChanges
{
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

        // Reducing every piece's damage boost from 9% to 8%
        private void ApplyAnyPieceEffect(Player player) => player.GetDamage<MagicDamageClass>() -= 0.01f;

        public override void ApplyHeadPieceEffect(Player player) => ApplyAnyPieceEffect(player);

        public override void ApplyBodyPieceEffect(Player player) => ApplyAnyPieceEffect(player);

        public override void ApplyLegPieceEffect(Player player) => ApplyAnyPieceEffect(player);

        public override void ApplyArmorSetBonus(Player player)
        {
            player.Calamity().meteorSet = true;
        }
    }
}
