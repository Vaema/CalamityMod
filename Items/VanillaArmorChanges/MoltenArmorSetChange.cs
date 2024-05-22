using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.VanillaArmorChanges
{
    public class MoltenArmorSetChange : VanillaArmorChange
    {
        public override int? HeadPieceID => ItemID.MoltenHelmet;

        public override int? BodyPieceID => ItemID.MoltenBreastplate;

        public override int? LegPieceID => ItemID.MoltenGreaves;

        public override string ArmorSetName => "Molten";

        public override void UpdateSetBonusText(ref string setBonusText)
        {
            setBonusText = $"{CalamityUtils.GetTextValue($"Vanilla.Armor.SetBonus.{ArmorSetName}")}";
        }

        public override void ApplyArmorSetBonus(Player player)
        {
            // Nerf the set bonus damage slightly
            player.GetDamage<MeleeDamageClass>() -= 0.03f;

            player.fireWalk = true;
            player.lavaMax += 300;
            player.GetDamage<TrueMeleeDamageClass>() += 0.1f;
        }
    }
}
