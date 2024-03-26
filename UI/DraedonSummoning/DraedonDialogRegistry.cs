using System;
using System.Collections.Generic;

namespace CalamityMod.UI.DraedonSummoning
{
    public static class DraedonDialogRegistry
    {
        public const string CommunicationLocalizationBase = "Mods.CalamityMod.UI.Communication.";

        public static readonly DraedonDialogEntry WhoAreYou = CreateFromKey("WhoAreYou");

        public static readonly DraedonDialogEntry WhoAreYouPart2 = CreateFromKey("WhoAreYouPart2");

        public static readonly DraedonDialogEntry Origins = CreateFromKey("Origins");

        public static readonly DraedonDialogEntry Crusade = CreateFromKey("Crusade");

        public static readonly DraedonDialogEntry Goals = CreateFromKey("Goals");

        public static readonly DraedonDialogEntry ExoMechs = CreateFromKey("ExoMechs");

        public static readonly DraedonDialogEntry Exotech = CreateFromKey("Exotech");

        public static readonly DraedonDialogEntry MiracleMatter = CreateFromKey("MiracleMatter");

        public static readonly DraedonDialogEntry Forge = CreateFromKey("Forge");

        public static readonly DraedonDialogEntry Yharim = CreateFromKey("Yharim");

        public static readonly DraedonDialogEntry Aerie = CreateFromKey("Aerie");

        public static readonly DraedonDialogEntry Calamitas = CreateFromKey("Calamitas");

        public static readonly DraedonDialogEntry Dog = CreateFromKey("Dog");

        public static readonly DraedonDialogEntry Distortion = CreateFromKey("Distortion");

        public static readonly DraedonDialogEntry Mechs = CreateFromKey("Mechs");

        public static readonly DraedonDialogEntry Plague = CreateFromKey("Plague");

        public static readonly DraedonDialogEntry Astral = CreateFromKey("Astral");

        public static readonly DraedonDialogEntry Sulphur = CreateFromKey("Sulphur");

        internal static List<DraedonDialogEntry> DialogOptions = new()
        {
            // The first index is assumed to be the "who are you?" dialog by the UI. All other indices can be freely swapped around, however.
            WhoAreYou,
            WhoAreYouPart2,

            Origins,
            Crusade,
            Goals,
            MiracleMatter,

            ExoMechs,
            Exotech,
            Forge,
            Mechs,
            Plague,

            Calamitas,
            Yharim,
            Dog,
            Sulphur,
            Distortion,
            Astral,
            Aerie
        };

        internal static DraedonDialogEntry CreateFromKey(string key, Func<bool> condition = null) =>
            new($"{CommunicationLocalizationBase}{key}", condition);
    }
}
