using System;
using System.Collections.Generic;
using System.Linq;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace CalamityMod.Systems.Quests;

/// <summary>
/// An <see langword="abstract"/> <see cref="ModSystem"/> that implements the foundation to make quests in Terraria<br/>
/// and making their data be saved and loaded accordingly.
/// </summary>
/// <typeparam name="TEnum">A <see cref="Enum"/> which contains all the progression points of the quest</typeparam>
public abstract class Quest<TEnum> : ModSystem where TEnum : Enum
{
    #region Data Saving & Loading

    public override void Unload() => QuestProgression = null;

    public override void ClearWorld()
    {
        // Resets all the properties when loading a world without a TagCompound.

        QuestStarted = false;
        QuestFinished = false;
        QuestProgression = ResetQuestProgression();
    }

    public override void LoadWorldData(TagCompound tag)
    {
        QuestStarted = tag.ContainsKey("questStarted");
        QuestFinished = tag.ContainsKey("questFinished");

        QuestProgression = ResetQuestProgression();

        // Although this method may seem weird, it's endorsed by tML on their guides for save & loading dictionaries.
        if (tag.TryGet<List<bool>>("questProgression", out var value))
            QuestProgression = QuestProgression.Keys.Zip(value, (k, v) => new { Key = k, Value = v }).ToDictionary(x => x.Key, x => x.Value);
    }

    public override void SaveWorldData(TagCompound tag)
    {
        // If any of them have modified values from the default value, we save them on the TagCompound.

        if (QuestStarted)
            tag.Add("questStarted", QuestStarted);

        if (QuestFinished)
            tag.Add("questFinished", QuestFinished);

        if (QuestProgression.ContainsValue(true))
            tag.Add("questProgression", QuestProgression.Values.ToList());
    }

    #endregion

    /// <summary>
    /// Whether or not this quest has started.
    /// </summary>
    public static bool QuestStarted { get; private set; }

    /// <summary>
    /// Whether or not this quest has finished.
    /// </summary>
    public static bool QuestFinished { get; private set; }

    /// <summary>
    /// A dictionary that saves a boolean for each progression point, to state which one was completed and which one was not.
    /// </summary>
    public static Dictionary<TEnum, bool> QuestProgression { get; private set; } = ResetQuestProgression();

    /// <summary>
    /// An event that is triggered when this quest starts.
    /// </summary>
    public static event Action OnQuestStarted;

    /// <summary>
    /// An event that is triggered when this quest finishes.
    /// </summary>
    public static event Action OnQuestFinish;

    /// <summary>
    /// An event that is triggered when this quest completes a new progression point.
    /// </summary>
    public static event Action<TEnum> OnQuestProgression;

    /// <summary>
    /// A method that makes this quest start and trigger its corresponding event.<br/>
    /// Does nothing if the quest has already started.
    /// </summary>
    public static void StartQuest()
    {
        if (!QuestStarted)
        {
            QuestStarted = true;
            OnQuestStarted?.Invoke();
        }
    }

    /// <summary>
    /// A method that makes this quest finishes and trigger its corresponding event.<br/>
    /// Does nothing if the quest has already finished.
    /// </summary>
    public static void FinishQuest()
    {
        if (!QuestFinished)
        {
            QuestFinished = true;
            OnQuestFinish?.Invoke();
        }
    }

    /// <summary>
    /// A method that makes a progression point be counted as completed.<br/>
    /// Does nothing if the progression point has already been completed.
    /// </summary>
    /// <param name="progressionPoint">The progression point that has been completed.</param>
    public static void ProgressQuest(TEnum progressionPoint)
    {
        if (!QuestProgression[progressionPoint])
        {
            QuestProgression[progressionPoint] = true;
            OnQuestProgression?.Invoke(progressionPoint);
        }
    }

    /// <summary>
    /// A quick method to resets the progression of Amidias' Quest.<br/>
    /// Used for when changing worlds or initializing <see cref="QuestProgression"/>.
    /// </summary>
    private static Dictionary<TEnum, bool> ResetQuestProgression()
    {
        Dictionary<TEnum, bool> questProgression = new();
        var progressionPoints = Enum.GetValues(typeof(TEnum));
        foreach (TEnum progressionPoint in progressionPoints)
            questProgression.Add(progressionPoint, false);
        return questProgression;
    }
}
