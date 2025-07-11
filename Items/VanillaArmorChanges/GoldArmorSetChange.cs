using System;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.VanillaArmorChanges
{
    public class GoldArmorSetChange : VanillaArmorChange
    {
        public override int? HeadPieceID => ItemID.GoldHelmet;

        public override int? BodyPieceID => ItemID.GoldChainmail;

        public override int? LegPieceID => ItemID.GoldGreaves;

        public override int[] AlternativeHeadPieceIDs => new int[] { ItemID.AncientGoldHelmet };

        public override string ArmorSetName => "Gold";

        public const float GoldDropChanceFromEnemies = 0.02f;
        public const int GoldFromBosses = 3;

        public override void UpdateSetBonusText(ref string setBonusText)
        {
            setBonusText += $"\n{CalamityUtils.GetText($"Vanilla.Armor.SetBonus.{ArmorSetName}").Format(GoldDropChanceFromEnemies.ToPercent(), GoldFromBosses)}";
        }

        public override void ApplyArmorSetBonus(Player player)
        {
            player.Calamity().goldArmorGoldDrops = true;
        }
    }
}
