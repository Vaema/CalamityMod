using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using Terraria.ModLoader;

namespace CalamityMod.UI
{
    [Autoload(Side = ModSide.Client)]
    public sealed class InvasionProgressUIManager : ModSystem
    {
        internal static readonly List<InvasionProgressUI> gUIs = [];

        public static int TotalGUIsActive => gUIs.Count(gui => gui.IsActive);
        public static bool AnyGUIsActive => TotalGUIsActive > 0;
        public static InvasionProgressUI GetActiveGUI => gUIs.FirstOrDefault(gui => gui.IsActive);
        public static void UpdateAndDraw(SpriteBatch spriteBatch)
        {
            if (AnyGUIsActive)
            {
                if (GetActiveGUI is null)
                    return;
                GetActiveGUI.Draw(spriteBatch);
            }
        }

        public override void Unload()
        {
            gUIs?.Clear();
        }
    }
}
