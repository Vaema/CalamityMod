using CalamityMod.Systems;
using Luminance.Core.MenuInfoUI;
using System.Collections.Generic;
using Terraria.ModLoader;

namespace CalamityMod.ModSupport.LuminanceSupport
{
    [JITWhenModsEnabled("Luminance")]
    [ExtendsFromMod("Luminance")]
    public sealed class CalamityUIManagers : InfoUIManager
    {
        public override IEnumerable<WorldInfoIcon> GetWorldInfoIcons()
        {
            yield return new WorldInfoIcon("CalamityMod/UI/ModeIndicator/ModeIndicator_Death",
            "Mods.CalamityMod.UI.DeathEnabled",
            worldData =>
            {
                if (!worldData.TryGetHeaderData<WorldSelectionDifficultySystem>(out var tagData))

                    return false;

                return tagData.ContainsKey("DeathMode") && tagData.GetBool("DeathMode");
            },
            50);
            yield return new WorldInfoIcon("CalamityMod/UI/ModeIndicator/ModeIndicator_Rev",
            "Mods.CalamityMod.UI.RevengeanceEnabled",
            worldData =>
            {
                if (!worldData.TryGetHeaderData<WorldSelectionDifficultySystem>(out var tagData))

                    return false;

                return tagData.ContainsKey("RevengeanceMode") && tagData.GetBool("RevengeanceMode") && !(tagData.ContainsKey("DeathMode") && tagData.GetBool("DeathMode"));
            },
            50);
        }
    }
}
