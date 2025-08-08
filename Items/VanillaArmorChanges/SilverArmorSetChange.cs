using System.Text;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.Items.VanillaArmorChanges
{
    public class SilverArmorSetChange : VanillaArmorChange
    {
        public override int? HeadPieceID => ItemID.SilverHelmet;

        public override int? BodyPieceID => ItemID.SilverChainmail;

        public override int? LegPieceID => ItemID.SilverGreaves;

        public override string ArmorSetName => "Silver";

        public const double SetBonusMinimumDamageToHeal = 20.0;
        public const int SetBonusHealTime = 120;
        public const int SetBonusHealAmount = 10;

        public override void UpdateSetBonusText(ref string setBonusText)
        {
            setBonusText += $"\n{CalamityUtils.GetText($"Vanilla.Armor.SetBonus.{ArmorSetName}").Format(SetBonusHealTime.FramesToSeconds(), SetBonusMinimumDamageToHeal.ToString("N0"), SetBonusHealAmount)}";
        }

        public override void ApplyArmorSetBonus(Player player)
        {
            player.Calamity().silverMedkit = true;
        }

        internal static void OnHealEffects(Entity entity)
        {
            Vector2 dustCenter = entity.Center;

            int numDust = 36;
            for (int i = 0; i < numDust; ++i)
            {
                float theta = MathHelper.TwoPi * (i / 36f);
                Vector2 dustVel = 3.5f * Vector2.One.RotatedBy(theta);
                Dust d = Dust.NewDustPerfect(dustCenter, DustID.SilverCoin, dustVel, Scale: 1.4f);
                d.noGravity = true;
                d.noLight = false;
            }
        }
    }
}
