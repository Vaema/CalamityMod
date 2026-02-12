using System;
using System.Linq;
using CalamityMod.Items.Accessories;
using CalamityMod.Items.Armor.LunicCorps;
using CalamityMod.Packets;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.CalPlayer
{
    // This should be a thing for reason:
    // - Draw call invoke faster than Player.Update on different thread
    // - Therefore draw call often gets default value for shield charge or such, results in flickering bug
    // So we calculate those value as same rate as Player.Update and does NOT reset between updates
    public struct CalamityPlayerDrawingParameters
    {
        // Profaned Shield (Profaned Soul Artifact / Profaned Soul Crystal)
        public float ProfanedShieldCharge;
        public Color ProfanedShieldColor;

        // The Sponge Shield
        public float SpongeShieldCharge;

        // RoverDrive Shield
        public float RoverShieldCharge;
        
        // Lunic Corps
        public float LunicShieldCharge;

        public static bool operator ==(CalamityPlayerDrawingParameters left, CalamityPlayerDrawingParameters right)
        {
            if (left.RoverShieldCharge != right.RoverShieldCharge) return false;
            if (left.LunicShieldCharge != right.LunicShieldCharge) return false;
            if (left.ProfanedShieldCharge != right.ProfanedShieldCharge) return false;
            if (left.ProfanedShieldColor != right.ProfanedShieldColor) return false;
            if (left.SpongeShieldCharge != right.SpongeShieldCharge) return false;
            return true;
        }

        public static bool operator !=(CalamityPlayerDrawingParameters left, CalamityPlayerDrawingParameters right)
        {
            return !(left == right);
        }

        public override readonly bool Equals(object obj) => base.Equals(obj);
        public override readonly int GetHashCode() => base.GetHashCode();
    }

    public partial class CalamityPlayer : ModPlayer
    {
        public CalamityPlayerDrawingParameters drawingParameters;
        private CalamityPlayerDrawingParameters drawingParameters_LastNetSyncValue;
        private int drawingParameters_NetSyncCountdown = 0;

        private void UpdateDrawingParameters()
        {
            // Non-Owner should receive update via network!
            if (Player.whoAmI == Main.myPlayer)
            {
                UpdateDrawParameter_RoverDriveShield();
                UpdateDrawParameter_LunicCorps();
                UpdateDrawParameter_ProfanedShield();
                UpdateDrawParameter_TheSponge();

                if (Main.netMode == NetmodeID.MultiplayerClient)
                {
                    if (drawingParameters_NetSyncCountdown > 0)
                    {
                        drawingParameters_NetSyncCountdown--;
                        return;
                    }

                    if (drawingParameters != drawingParameters_LastNetSyncValue)
                    {
                        drawingParameters_NetSyncCountdown = 8;
                        drawingParameters_LastNetSyncValue = drawingParameters;
                        SyncPlayerDrawParameterPacket.Send(this);
                    }
                }
            }
        }

        #region RoverDrive Shield
        private void UpdateDrawParameter_RoverDriveShield()
        {
            bool isVanityOnly = roverDriveShieldVisible && !roverDrive;
            bool shieldExists = isVanityOnly || RoverDriveShieldDurability > 0;
            if (roverDriveShieldVisible && shieldExists)
            {
                float visualShieldStrength = 1f;
                if (!isVanityOnly)
                {
                    float shieldDurabilityRatio = RoverDriveShieldDurability / (float)RoverDrive.ShieldDurabilityMax;
                    visualShieldStrength = MathF.Pow(shieldDurabilityRatio, 0.5f);
                }

                drawingParameters.RoverShieldCharge = visualShieldStrength;
            }
            else
            {
                drawingParameters.RoverShieldCharge = -1.0f;
            }
        }
        #endregion

        #region LunicCorps Shield
        private void UpdateDrawParameter_LunicCorps()
        {
            if (LunicCorpsShieldDurability > 0)
            {
                float shieldDurabilityRatio = LunicCorpsShieldDurability / (float)LunicCorpsHelmet.ShieldDurabilityMax;
                float visualShieldStrength = MathF.Pow(shieldDurabilityRatio, 0.5f);

                drawingParameters.LunicShieldCharge = visualShieldStrength;
            }
            else
            {
                drawingParameters.LunicShieldCharge = -1.0f;
            }
        }
        #endregion

        #region Profaned Shield (Profaned Soul Artifact / Profaned Soul Crystal)
        private void UpdateDrawParameter_ProfanedShield()
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
                drawingParameters.ProfanedShieldCharge = visualShieldStrength;
            }
            else
            {
                drawingParameters.ProfanedShieldCharge = -1.0f;
                drawingParameters.ProfanedShieldColor = Color.White;
            }
        }
        #endregion

        #region Sponge Shield
        private void UpdateDrawParameter_TheSponge()
        {
            bool isVanityOnly = spongeShieldVisible && !sponge;
            bool shieldExists = isVanityOnly || SpongeShieldDurability > 0;
            if (spongeShieldVisible && shieldExists)
            {
                float visualShieldStrength = 1f;
                if (!isVanityOnly)
                {
                    float shieldDurabilityRatio = SpongeShieldDurability / (float)TheSponge.ShieldDurabilityMax;
                    visualShieldStrength = MathF.Pow(shieldDurabilityRatio, 0.5f);
                }

                drawingParameters.SpongeShieldCharge = visualShieldStrength;
            }
            else
            {
                drawingParameters.SpongeShieldCharge = -1.0f;
            }
        }
        #endregion
    }
}
