using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CalamityMod.Items.Accessories;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.CalPlayer
{
    // This should be a thing for reason:
    // - Draw call invoke faster than Player.Update
    // - Therefore draw call often gets default value for shield charge or such, results in flickering bug
    // So we calculate those value as same rate as Player.Update
    public struct CalamityPlayerDrawingParameters
    {
        public float ProfanedShieldStrength;
        public Color ProfanedShieldColor;

        public float RoverShieldCharge;
        public float SpongeShieldCharge;
        public float LunicShieldCharge;
    }

    public partial class CalamityPlayer : ModPlayer
    {
        public CalamityPlayerDrawingParameters drawingParameters;

        private void UpdateDrawingParameters()
        {
            bool isVanityOnly = pSoulShieldVisible && !pSoulArtifact;
            bool shouldNotDraw = andromedaState >= AndromedaPlayerState.LargeRobot; //I am not dealing with drawing that :taxevasion:
            bool shieldExists = isVanityOnly || pSoulShieldDurability > 0;
            if (pSoulShieldVisible && !shouldNotDraw && shieldExists)
            {
                ProfanedSoulCrystal.DetermineTransformationEligibility(Player);
                var psState = (int)ProfanedSoulCrystal.GetPscStateFor(Player, profanedCrystalAnim >= 0);
                var psc = profanedCrystalBuffs || (profanedCrystalAnim >= 0 && psState >= (int)ProfanedSoulCrystal.ProfanedSoulCrystalState.Buffs);

                float visualShieldStrength = 1f;
                if (!isVanityOnly)
                {
                    float max = psc ? ProfanedSoulCrystal.ShieldDurabilityMax : ProfanedSoulArtifact.ShieldDurabilityMax;
                    float shieldDurabilityRatio = pSoulShieldDurability / max;
                    visualShieldStrength = MathF.Pow(shieldDurabilityRatio, 0.5f);
                }

                Color shieldColor = ProfanedSoulCrystal.GetColorForPsc(psState, Main.dayTime);
                if (psState >= (int)(ProfanedSoulCrystal.ProfanedSoulCrystalState.Buffs))
                {
                    bool tester = ProfanedSoulCrystal.contributorNames.Any(name => name.Equals(Player.name));
                    shieldColor = tester ? CalamityUtils.ColorSwap(new Color(255, 166, 0), new Color(25, 250, 25) * 0.8f, 6f) :
                    ProfanedSoulCrystal.GetLerpedColorForPsc(this);
                }

                drawingParameters.ProfanedShieldColor = shieldColor;
                drawingParameters.ProfanedShieldStrength = visualShieldStrength;
            }
            else
            {
                drawingParameters.ProfanedShieldStrength = -1.0f;
                drawingParameters.ProfanedShieldColor = Color.White;
            }
        }
    }
}
