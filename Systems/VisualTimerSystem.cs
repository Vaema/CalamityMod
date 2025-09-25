using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Terraria.ModLoader;

namespace CalamityMod.Systems
{
    public class VisualTimerSystem : ModSystem
    {
        /// <summary>
        /// Dummy global variable that increments by one every frame. Good for animated visual effects. Not synced server side.
        /// </summary>
        public static float GlobalVisualTimer = 0f;

        public override void PostUpdateEverything()
        {
            GlobalVisualTimer++;
        }
    }
}
