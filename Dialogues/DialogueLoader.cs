using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using CalamityMod.UI.DialogueDisplay;
using MonoMod.Cil;
using MonoMod.RuntimeDetour;
using Terraria.Localization;
using Terraria.ModLoader;

namespace CalamityMod.Dialogues;

internal record DialogueTextDataEntry(
    string FilePath,
    string DialogueKey,
    DialogueTextData Data
    );

internal class DialogueLoader : ModSystem
{
    private const string DialogueFilePrefix = "CalamityDialogue.";

    private static readonly Dictionary<string, DialogueTextDataEntry> _DialogueLookup = [];

    private ILHook _ExtractLocalizationHook;

    public override void Load()
    {
        _DialogueLookup.Clear();

        var method = typeof(LocalizationLoader).GetMethod(nameof(LocalizationLoader.ExtractLocalizationFiles), BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static);
        if (method != null)
        {
            _ExtractLocalizationHook = new ILHook(method, ExtractDialogueFilesPatch);
            _ExtractLocalizationHook.Apply();
        }
    }

    public override void Unload()
    {
        _DialogueLookup.Clear();

        if (_ExtractLocalizationHook != null)
        {
            _ExtractLocalizationHook.Undo();
            _ExtractLocalizationHook.Dispose();
            _ExtractLocalizationHook = null;
        }
    }

    private static void ExtractDialogueFilesPatch(ILContext il)
    {
        var cursor = new ILCursor(il);

        int pathLdloc = -1;
        int modLdloc = -1;
        if (!cursor.TryGotoNext(MoveType.After,
            i => i.MatchLdloc(out modLdloc), // Mod mod
            i => i.MatchLdloc(out pathLdloc), // string path
            i => i.MatchCallOrCallvirt(out _), // GameCulture ActiveCulture
            i => i.MatchCallOrCallvirt(typeof(LocalizationLoader), nameof(LocalizationLoader.UpdateLocalizationFilesForMod))))
        {
            CalamityMod.Log.ILFailure("Force Extract Dialogue Files", $"Unable to locate {nameof(LocalizationLoader.UpdateLocalizationFilesForMod)} call");
        }

        if (modLdloc == -1)
        {
            CalamityMod.Log.ILFailure("Force Extract Dialogue Files", $"Unable to locate ldloc index for mod");
        }

        if (pathLdloc == -1)
        {
            CalamityMod.Log.ILFailure("Force Extract Dialogue Files", $"Unable to locate ldloc index for path");
        }

        cursor.EmitLdloc(modLdloc);
        cursor.EmitLdloc(pathLdloc);
        cursor.EmitDelegate((Mod mod, string basePath) =>
        {
            if (mod != CalamityMod.Instance)
                return;

            foreach (var entry in GetDialogueTextDatas(CalamityMod.Instance, GameCulture.DefaultCulture, skipDeserializeData: true))
            {
                try
                {
                    var destFilePath = Path.Combine(basePath, entry.FilePath);
                    var destDir = Path.GetDirectoryName(destFilePath);
                    if (!Directory.Exists(destDir)) Directory.CreateDirectory(destDir);

                    using var stream = CalamityMod.Instance.File.GetStream(entry.FilePath);
                    using var fileStream = File.OpenWrite(destFilePath);
                    using var writer = new StreamWriter(fileStream, Encoding.UTF8);
                    using var reader = new StreamReader(stream, Encoding.UTF8);
                    writer.Write(reader.ReadToEnd());
                }
                catch (Exception e)
                {
                    CalamityMod.Log.Error($"Error while exporting DialogueTextData entry: {e}");
                }
            }
        });
    }

    public static bool TryGetDialogue(string dialogueKey, out DialogueTextData data)
    {
        if (_DialogueLookup.TryGetValue(dialogueKey, out var entry))
        {
            data = entry.Data;
            return true;
        }

        data = null;
        return false;
    }

    public override void OnLocalizationsLoaded()
    {
        _DialogueLookup.Clear();

        foreach (var entry in GetDialogueTextDatas(Mod, GameCulture.DefaultCulture))
        {
            _DialogueLookup[entry.DialogueKey] = entry;
        }

        var activeCulture = LanguageManager.Instance.ActiveCulture;
        foreach (var mod in ModLoader.Mods.Where(mod => mod != Mod))
        {
            foreach (var entry in GetDialogueTextDatas(mod, activeCulture))
            {
                if (!_DialogueLookup.TryGetValue(entry.DialogueKey, out var oldEntry))
                {
                    CalamityMod.Log.Warn($"Dialogue Localization was detected but original Dialogue file does not exists. This will not be applied! : '{entry.FilePath}'");
                    continue;
                }

                if (entry.Data.Revision != oldEntry.Data.Revision)
                {
                    CalamityMod.Log.Warn($"Dialogue Localization was detected but revision mismatches. This will not be applied! : '{entry.FilePath}'");
                    continue;
                }

                _DialogueLookup[entry.DialogueKey] = entry;
            }
        }
    }

    private static IEnumerable<DialogueTextDataEntry> GetDialogueTextDatas(Mod mod, GameCulture targetCulture, bool skipDeserializeData = false)
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
            if (!skipDeserializeData)
            {
                try
                {
                    using var stream = new StreamReader(mod.File.GetStream(file), Encoding.UTF8);
                    data = JsonSerializer.Deserialize<DialogueTextData>(stream.BaseStream);
                }
                catch (Exception e)
                {
                    CalamityMod.Log.Error($"Error while reading DialogueTextData entry: {e}");
                }
            }

            if (data != null || skipDeserializeData)
            {
                yield return new DialogueTextDataEntry(file.Name, dialogueKey, data);
            }
        }
    }
}
