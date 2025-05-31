using System;
using System.Diagnostics;
using System.Security.Cryptography;
using CalamityMod.NPCs;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.ModLoader;

namespace CalamityMod.DataStructures
{
    [ReinitializeDuringResizeArrays]
    public static class BuffDatasets
    {
        public static DebuffData[] DebuffDataset = BuffID.Sets.Factory.CreateNamedSet("DebuffData")
            .Description("Stores DebuffData for a particular debuff")
            .RegisterCustomSet<DebuffData>(null,
                BuffID.OnFire, DebuffData.OnFire,
                BuffID.OnFire3, DebuffData.Hellfire,
                BuffID.CursedInferno, DebuffData.CursedInferno,
                BuffID.ShadowFlame, DebuffData.Shadowflame,
                BuffID.Daybreak, DebuffData.Daybroken,
                BuffID.Frostburn, DebuffData.Frostburn,
                BuffID.Frostburn2, DebuffData.Frostbite,
                BuffID.Poisoned, DebuffData.Poisoned,
                BuffID.Venom, DebuffData.AcidVenom,
                BuffID.Electrified, DebuffData.Electrified
            );
    }
    public class DebuffData
    {
        /// <summary>
        /// Determines the behavior of the debuff.
        /// </summary>
        public enum DebuffBehavior
        {
            Default,
            Electric
        }
        #region DebuffData data
        /// <summary>
        /// Damage Per Second of this debuff on a player.
        /// </summary>
        public float PlayerDPS = 0;

        /// <summary>
        /// Damage Per Second of this debuff on a player
        /// </summary>
        public float EnemyLostRegen = 0;

        /// <summary>
        /// This is used to cancel Vanilla dot damage and allow using Calamity's own instead
        /// </summary>
        public int VanillaRegenToCancelOut = 0;

        /// <summary>
        /// minimum Damage tick size of this debuff
        /// </summary>
        public int MinimumDamageTickSize = 1;

        /// <summary>
        /// Damage tick size of this debuff, compared to total DPS
        /// MinimumDamageTickSize takes prioriity if this returns lower
        /// </summary>
        public float MultiplierDamageTickSize = 0.5f;

        /// <summary>
        /// How much this benefits from heat debuff amplifiers.
        /// Default is 0, which means it does not benefit.
        /// </summary>
        public float HeatDebuffScaling = 0;
        /// <summary>
        /// How much this benefits from sickness debuff amplifiers.
        /// Default is 0, which means it does not benefit.
        /// </summary>
        public float SicknessDebuffScaling = 0;
        /// <summary>
        /// How much this benefits from cold debuff amplifiers.
        /// Default is 0, which means it does not benefit.
        /// </summary>
        public float ColdDebuffScaling = 0;
        /// <summary>
        /// How much this benefits from water debuff amplifiers.
        /// Default is 0, which means it does not benefit.
        /// </summary>
        public float WaterDebuffScaling = 0;
        /// <summary>
        /// How much this benefits from electric debuff amplifiers.
        /// Default is 0, which means it does not benefit.
        /// </summary>
        public float ElectricDebuffScaling = 0;

        /// <summary>
        /// Whether or not this debuff should draw above NPCs.
        /// </summary>
        public bool DrawAboveNPC = true;

        /// <summary>
        /// Whether or not player gear can modify the debuff effects such as duration or damage.
        /// </summary>
        public bool GearCanModifyDebuff = true;

        /// <summary>
        /// How much alcohol this counts as.
        /// Default is 0, most alcohol is 1, and Everclear is 2
        /// </summary>
        public float AlcoholLevel = 0f;

        /// <summary>
        /// The UpdateOnPlayer code to run. Defaults to just applying DoT
        /// </summary>
        public UpdateOnPlayer PlayerUpdateMethod;

        /// <summary>
        /// The UpdateNPCLifeRegen code to run. Defaults to just applying DoT
        /// </summary>
        public UpdateNPCLifeRegen NPCLifeRegenMethod;

        public DebuffData()
        {
            PlayerUpdateMethod = DefaultUpdateOnPlayer;
            NPCLifeRegenMethod = BaseUpdateNPCLifeRegen;
        }
        /// <summary>
        /// Allows using keys to determine preset behavior
        /// "electric" causes debuffs to scale 4x when moving
        /// Uses default behavior if no known key is inputed
        /// </summary>
        /// <param name="key"></param>
        public DebuffData(DebuffBehavior key)
        {

            PlayerUpdateMethod = DefaultUpdateOnPlayer;
            if (key == DebuffBehavior.Electric)
                NPCLifeRegenMethod = ElectricDebuffNPCLifeRegen;
            else
                NPCLifeRegenMethod = BaseUpdateNPCLifeRegen;
        }

        /// <summary>
        /// This is the code that should be run when updating the buff on a player.
        /// Use for gameplay effects, not drawing.
        /// </summary>
        /// <param name="player"></param>
        public delegate void UpdateOnPlayer(Player player, int buffType, ref int buffIndex, ref int damage);

        /// <summary>
        /// This is the code that should be run when updating life regen on NPC
        /// Use for gameplay effects, not drawing.
        /// </summary>
        /// <param name="npc"></param>
        public delegate void UpdateNPCLifeRegen(NPC npc, int buffType, ref int buffIndex, ref int damage);

        /// <summary>
        /// The default debuff DoT functionality
        /// </summary>
        public void DefaultUpdateOnPlayer(Player player, int buffType, ref int buffIndex, ref int damage)
        {
            
        }

        /// <summary>
        /// The default debuff DoT functionality
        /// </summary>
        public void BaseUpdateNPCLifeRegen(NPC npc, int buffType, ref int buffIndex, ref int damage)
        {
            var cnpc = npc.Calamity();
            double totalDPS = EnemyLostRegen;
            double totalScaling =
                HeatDebuffScaling + ColdDebuffScaling + SicknessDebuffScaling + WaterDebuffScaling + ElectricDebuffScaling == 0
                ?
                1 + (
                    (cnpc.ActiveHeatDebuffMultiplier - 1) * HeatDebuffScaling +
                    (cnpc.ActiveColdDebuffMultiplier - 1) * ColdDebuffScaling +
                    (cnpc.ActiveSicknessDebuffMultiplier - 1) * SicknessDebuffScaling +
                    (cnpc.ActiveWaterDebuffMultiplier - 1) * WaterDebuffScaling +
                    (cnpc.ActiveElectricDebuffMultiplier - 1) * ElectricDebuffScaling
                )
                :
                1 + (cnpc.TypelessDebuffMultiplier-1);
            totalDPS *= totalScaling;
            var totalDPSAdjusted = totalDPS-VanillaRegenToCancelOut;
            npc.Calamity().ApplyDPSDebuff((int)(totalDPSAdjusted), (int)Math.Min(totalDPS*MultiplierDamageTickSize,MinimumDamageTickSize), ref npc.lifeRegen, ref damage);
        }
        public void ElectricDebuffNPCLifeRegen(NPC npc, int buffType, ref int buffIndex, ref int damage)
        {
            var cnpc = npc.Calamity();
            double totalDPS = EnemyLostRegen;
            double totalScaling =
                HeatDebuffScaling + ColdDebuffScaling + SicknessDebuffScaling + WaterDebuffScaling + ElectricDebuffScaling == 0
                ?
                1 + (
                    (cnpc.ActiveHeatDebuffMultiplier - 1) * HeatDebuffScaling +
                    (cnpc.ActiveColdDebuffMultiplier - 1) * ColdDebuffScaling +
                    (cnpc.ActiveSicknessDebuffMultiplier - 1) * SicknessDebuffScaling +
                    (cnpc.ActiveWaterDebuffMultiplier - 1) * WaterDebuffScaling +
                    (cnpc.ActiveElectricDebuffMultiplier - 1) * ElectricDebuffScaling
                )
                :
                1 + (cnpc.TypelessDebuffMultiplier - 1);
            totalDPS *= totalScaling;
            totalDPS *= (npc.velocity.X == 0 ? 1 : 4);
            totalDPS -= VanillaRegenToCancelOut * (npc.velocity.X == 0 ? 1 : 5); //Vanilla Electrified is 5x when moving, not 4x
            npc.Calamity().ApplyDPSDebuff((int)(totalDPS), (int)Math.Min(totalDPS * MultiplierDamageTickSize, MinimumDamageTickSize), ref npc.lifeRegen, ref damage);
        }
        #region Special Regen Functions
        public static void DaybrokenRegen(NPC npc, int buffType, ref int buffIndex, ref int damage)
        { 
            // 18OCT2023: Ozzatron: im not gonna sugarcoat it
            // vanilla debuff damage from Daybreak impales scales linearly up to 8 for 800 DPS
            // instead of allowing this entire 800 DPS to be multiplied by heat weakness + heat DoT bonuses,
            // each Daybreak spear beyond the first is only affected 25% as much by weaknesses or resistances.
            // This also stops Daybreak's DPS from being utterly shafted by heat resistance.
            // As no other weapon can stack Daybroken, this has no effect on other weapons (they count as "1 Daybreak spear")
            var cnpc = npc.Calamity();
            int numImpaledSpears = 0;
            for (int k = 0; k < Main.maxProjectiles; k++)
            {
                if (Main.projectile[k].active && Main.projectile[k].type == ProjectileID.Daybreak && Main.projectile[k].ai[0] == 1f && Main.projectile[k].ai[1] == npc.whoAmI)
                    numImpaledSpears++;
            }

            // If there are no Daybreak impaled spears, Daybroken has 1x potency (it was applied some other way)
            float daybrokenMultiplier = numImpaledSpears <= 1 ? 1f : (1f + 0.25f * (numImpaledSpears - 1));

            int baseDaybreakDoTValue = (int)(daybrokenMultiplier * 2 * 100 * (cnpc.ActiveHeatDebuffMultiplier - CalamityGlobalNPC.BaseDoTDamageMult));
            npc.lifeRegen -= baseDaybreakDoTValue;
            if (damage < baseDaybreakDoTValue / 4)
                damage = baseDaybreakDoTValue / 4;
        }

        
        #endregion
        #endregion

        #region Vanilla debuff stats
        public static DebuffData OnFire = new DebuffData()
        {
            EnemyLostRegen = 12,
            VanillaRegenToCancelOut = 12,
            HeatDebuffScaling = 1
        };
        public static DebuffData Hellfire = new DebuffData()
        {
            EnemyLostRegen = 30,
            VanillaRegenToCancelOut = 30,
            HeatDebuffScaling = 1
        };
        public static DebuffData CursedInferno = new DebuffData()
        {
            EnemyLostRegen = 48,
            VanillaRegenToCancelOut = 48,
            HeatDebuffScaling = 1
        };
        public static DebuffData Shadowflame = new DebuffData()
        {
            EnemyLostRegen = 60,
            VanillaRegenToCancelOut = 60,
            HeatDebuffScaling = 1
        };
        public static DebuffData Daybroken = new DebuffData()
        {
            // These first three are not actually used in the Daybroken logic due to it being copied from prior systems
            // Ideally, they would be used for consistency, but no effects besides DoT currently interface with these.
            EnemyLostRegen = 200,
            VanillaRegenToCancelOut = 200,
            HeatDebuffScaling = 1,
            NPCLifeRegenMethod = DaybrokenRegen
        };
        public static DebuffData Frostburn = new DebuffData()
        {
            EnemyLostRegen = 16,
            VanillaRegenToCancelOut = 16,
            ColdDebuffScaling = 1
        };
        public static DebuffData Frostbite = new DebuffData()
        {
            EnemyLostRegen = 50,
            VanillaRegenToCancelOut = 50,
            ColdDebuffScaling = 1
        };
        public static DebuffData Poisoned = new DebuffData()
        {
            EnemyLostRegen = 12,
            VanillaRegenToCancelOut = 12,
            SicknessDebuffScaling = 1
        };
        public static DebuffData AcidVenom = new DebuffData()
        {
            EnemyLostRegen = 60,
            VanillaRegenToCancelOut = 60,
            SicknessDebuffScaling = 1
        };
        public static DebuffData Electrified = new DebuffData(DebuffBehavior.Electric)
        {
            EnemyLostRegen = 21,
            VanillaRegenToCancelOut = 8,
            ElectricDebuffScaling = 1
        };
        #endregion
    }
}
