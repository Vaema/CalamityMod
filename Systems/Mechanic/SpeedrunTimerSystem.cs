using System;
using System.Diagnostics;
using CalamityMod.CalPlayer;
using Terraria.ModLoader;

namespace CalamityMod.Systems
{
    internal sealed class SpeedrunTimerSystem : ModSystem
    {
        public static TimeSpan Elapsed => _SpeedrunTimer.Elapsed;

        private static Stopwatch _SpeedrunTimer = new();

        private const string FormatStringForDay = @"hh\:mm\:ss\.ff";
        private const string FormatString = @"d\:hh\:mm\:ss\.ff";

        public static void Restart()
        {
            _SpeedrunTimer.Restart();
        }

        public static string GetTimerText(CalamityPlayer player)
        {
            string formatStr = @"hh\:mm\:ss\.ff";
            string formatStrDays = @"d\:hh\:mm\:ss\.ff";
            TimeSpan totalTime = Elapsed.Add(player.previousSessionTotal);
            return totalTime.ToString(totalTime.Days > 0 ? formatStrDays : formatStr);
        }

        public static string GetSplitText(CalamityPlayer player)
        {
            TimeSpan split = player.lastSplit;
            return split.ToString(split.Days > 0 ? FormatStringForDay : FormatString);
        }

        public override void PreSaveAndQuit() => _SpeedrunTimer?.Stop();
    }
}
