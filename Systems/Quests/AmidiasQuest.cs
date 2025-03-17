namespace CalamityMod.Systems.Quests
{
    /// <summary>
    /// The different progression points of Amidias' Quest.
    /// </summary>
    public enum AmidiasQuestProgression
    {
        KilledGiantClam,
    }

    /// <summary>
    /// A ModSystem which controls, progresses, saves, and loads the data necessary for Amidias' Quest.
    /// </summary>
    public class AmidiasQuest : Quest<AmidiasQuestProgression> { }
}
