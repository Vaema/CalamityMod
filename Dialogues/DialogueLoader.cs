using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using CalamityMod.UI.DialogueDisplay;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Dialogues;

internal record DialogueTextDataEntry(string FilePath, string DialogueKey, DialogueTextData Data);

internal class DialogueLoader : ModSystem
{
    private const string DialogueFilePrefix = "CalamityDialogue.";

    private static readonly Dictionary<string, DialogueTextData> _DialogueLookup = [];

    public override void Load() => _DialogueLookup.Clear();

    public override void Unload() => _DialogueLookup.Clear();

    public static bool TryGetDialogue(string dialogueKey, out DialogueTextData data)
    {
        return _DialogueLookup.TryGetValue(dialogueKey, out data);
    }

    public override void OnLocalizationsLoaded()
    {
        _DialogueLookup.Clear();

        foreach (var entry in GetDialogueTextDatas(Mod, GameCulture.DefaultCulture))
        {
            _DialogueLookup[entry.DialogueKey] = entry.Data;
        }

        var activeCulture = LanguageManager.Instance.ActiveCulture;
        foreach (var mod in ModLoader.Mods.Where(mod => mod != Mod))
        {
            foreach (var entry in GetDialogueTextDatas(mod, activeCulture))
            {
                if (!_DialogueLookup.ContainsKey(entry.DialogueKey))
                    continue;

                _DialogueLookup[entry.DialogueKey] = entry.Data;
            }
        }
    }

    private static IEnumerable<DialogueTextDataEntry> GetDialogueTextDatas(Mod mod, GameCulture targetCulture)
    {
        if (mod == null)
            yield break;

        if (mod.File == null)
            yield break;

        foreach (var file in mod.File)
        {
            if (!Path.GetExtension(file.Name).Equals(".json", StringComparison.InvariantCultureIgnoreCase))
                continue;

            if (!LocalizationLoader.TryGetCultureAndPrefixFromPath(file.Name, out var culture, out var prefix))
                continue;

            string fileName = Path.GetFileNameWithoutExtension(file.Name);
            string dialogueKey;
            if (fileName.StartsWith($"{prefix}_{DialogueFilePrefix}", StringComparison.InvariantCultureIgnoreCase))
                dialogueKey = fileName[$"{prefix}_{DialogueFilePrefix}".Length..];
            else if (fileName.StartsWith(DialogueFilePrefix, StringComparison.InvariantCultureIgnoreCase))
                dialogueKey = fileName[DialogueFilePrefix.Length..];
            else
                continue;

            // Explictly Allow Default Culture File.
            // This is for Translation mod that made for non-specified culture
            if (culture != targetCulture && culture != GameCulture.DefaultCulture)
                continue;

            DialogueTextData data = null;
            try
            {
                using var stream = new StreamReader(mod.File.GetStream(file), Encoding.UTF8);
                data = JsonSerializer.Deserialize<DialogueTextData>(stream.BaseStream);
            }
            catch (Exception e)
            {
                CalamityMod.Log.Error($"Error while reading DialogueTextData entry: {e}");
            }

            if (data != null)
            {
                CalamityMod.Log.Info($"Found Dialogue Item: '{mod.Name}', '{file.Name}', '{prefix}', '{culture.Name}'");
                yield return new DialogueTextDataEntry(file.Name, dialogueKey, data);
            }
        }
    }
}
