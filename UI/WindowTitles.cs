using Terraria;
using Terraria.ModLoader;
using Terraria.Localization;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using ReLogic.OS;

namespace CalamityMod.UI
{
    public class WindowTitles : ModSystem
    {
        private static LocalizedText _calamityModifiedText;
        private static bool loaded = false; 
        public override void PostSetupContent()
        {
            // the other method involving some terraria intrinsic function didn't work, so i'm just ignoring it
            var vanillaTitles = Language.FindAll(new Regex("^GameTitle\\.")).ToList();
            var customTitles = Language.FindAll(new Regex("^Mods\\.CalamityMod\\.UI\\.WindowTitle\\.")).ToList();

            var allTitles = new List<LocalizedText>();
            allTitles.AddRange(vanillaTitles);
            allTitles.AddRange(customTitles);
            
            _calamityModifiedText ??= allTitles[Main.rand.Next(allTitles.Count)];
            
            // this is what vanilla terraria does to set it's title, so i'm replicating that here
            Platform.Get<IWindowService>().SetUnicodeTitle(Main.instance.Window, _calamityModifiedText.Value);
            Platform.Get<IWindowService>().SetIcon(Main.instance.Window);

            loaded = true;
        }

        public override void Unload()
        {
            Platform.Get<IWindowService>().SetUnicodeTitle(Main.instance.Window, Lang.GetRandomGameTitle());
            Platform.Get<IWindowService>().SetIcon(Main.instance.Window);

            _calamityModifiedText = null;
            loaded = false;
            base.Unload();
        }
    }
}
