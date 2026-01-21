using System;
using System.Collections.Generic;
using Terraria.ModLoader;

namespace CalamityMod.CalPlayer.Dashes
{
    public sealed class PlayerDashManager : ModSystem
    {
        internal static Dictionary<string, PlayerDashEffect> DashIdentificationTable = new();

        public static bool FindByID(string id, out PlayerDashEffect dashEffect)
        {
            return DashIdentificationTable.TryGetValue(id, out dashEffect);
        }

        // A new function that allows to add in new modded dashes for Calamity to recognize, it attempts
        // to make it as safe as possible.


        /// <summary>
        /// Attempts to add a dash effect into the dash manager. It should be executed in a Load() function.
        /// </summary>
        /// <param name="dashEffect">An instance of the desired dash effect to add.</param>

        public static void TryAddDash(PlayerDashEffect dashEffect)
        {
            //If DashIdentificationTable is not loaded or modder tries to load an abstract type of
            //PlayerDashEffect, then don't add the dash and stop the method.
            if (DashIdentificationTable == null || dashEffect.GetType().IsAbstract) return;

            string id = dashEffect.DashID;

            //This chunk of the code only executes if the dash isn't already added and
            //the dash id isn't empty.
            if (!DashIdentificationTable.ContainsKey(id) && !String.IsNullOrEmpty(id))
            {
                DashIdentificationTable[id] = dashEffect;
            }
        }

        public override void Unload()
        {
            DashIdentificationTable = null;
        }
    }
}
