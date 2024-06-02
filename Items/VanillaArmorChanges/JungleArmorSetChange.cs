using System.Text;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.VanillaArmorChanges
{
    public class JungleArmorSetChange : VanillaArmorChange
    {
        public override int? HeadPieceID => ItemID.JungleHat;

        public override int? BodyPieceID => ItemID.JungleShirt;

        public override int? LegPieceID => ItemID.JunglePants;

        public override int[] AlternativeHeadPieceIDs => new int[] { ItemID.AncientCobaltHelmet };

        public override int[] AlternativeBodyPieceIDs => new int[] { ItemID.AncientCobaltBreastplate };

        public override int[] AlternativeLegPieceIDs => new int[] { ItemID.AncientCobaltLeggings };

        public override string ArmorSetName => "Jungle";

        public override void UpdateSetBonusText(ref string setBonusText)
        {
            setBonusText = $"{CalamityUtils.GetTextValue($"Vanilla.Armor.SetBonus.{ArmorSetName}")}";
        }

        public override void ApplyHeadPieceEffect(Player player)
        {
            player.statManaMax2 -= 20;
            player.GetCritChance<MagicDamageClass>() -= 3;
        }

        public override void ApplyLegPieceEffect(Player player) => player.GetCritChance<MagicDamageClass>() -= 3;

        public override void ApplyArmorSetBonus(Player player) => player.manaCost += 0.06f; // Reduces to -10% mana cost
    }
}
