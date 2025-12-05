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
        private static readonly List<InvasionProgressUI> gUIs = [];
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

        public override void OnModLoad()
        {
            // Look through every type in the mod, and check if it's derived from InvasionProgressUI. If it is, create a copy and save it in the static list.
            ReflectionHelper.IterateCalamityTypes<InvasionProgressUI>(action: type =>
            {
                gUIs.Add(Activator.CreateInstance(type) as InvasionProgressUI);
            });
        }

        public override void Unload()
        {
            gUIs?.Clear();
        }
    }
}
