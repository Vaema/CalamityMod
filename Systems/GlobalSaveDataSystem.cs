using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Terraria;
using Terraria.ModLoader;

namespace CalamityMod.Systems;

public class GlobalSaveDataSystem : ModSystem
{
    internal static List<string> GlobalSaveDataKeys = [];

    /// <summary>
    /// The directory in which all files related to globall saved data are stored.
    /// </summary>
    internal static string GlobalSaveDataDirectoryPath;

    /// <summary>
    /// The path to the text file which contains all globally saved data keys.
    /// </summary>
    internal static string GlobalSaveDataKeysPath;

    /// <summary>
    /// The path to the README file explaining the purpose of the directory.
    /// </summary>
    internal static string READMEPath;

    public override void OnModLoad()
    {
        string savePathByOS = OperatingSystem.IsLinux() ? "~/.local/share/Terraria/tModloader/" : Main.SavePath;
        GlobalSaveDataDirectoryPath = savePathByOS + Path.DirectorySeparatorChar + "CalamityModGlobalSaveData";
        GlobalSaveDataKeysPath = savePathByOS + Path.DirectorySeparatorChar + "CalamityModGlobalSaveData" + Path.DirectorySeparatorChar + "GlobalSaveDataKeys.txt";
        READMEPath = savePathByOS + Path.DirectorySeparatorChar + "CalamityModGlobalSaveData" + Path.DirectorySeparatorChar + "README.txt";

        // Ensure that the save data keys text file exists. If not, create a new one.
        if (!File.Exists(GlobalSaveDataKeysPath))
        {
            // Create the directory if necessary.
            if (!Directory.Exists(GlobalSaveDataDirectoryPath)) 
                Directory.CreateDirectory(GlobalSaveDataDirectoryPath);

            // Create both the Global Save Data text file, as well as a README file explaining the directory's purpose.
            if (!File.Exists(READMEPath))
            {
                using StreamWriter readme = File.CreateText(READMEPath);
                readme.WriteLine("Hi! You're probably wondering what this directory is and why it exists if you've found it.");
                readme.WriteLine("This is how the Calamity Mod currently manages data that is saved globally across all worlds. " +
                    "You will find all keys managing globally saved data in the other text file within this folder. You are free to remove " +
                    "any of those keys if you wish to reapply the lock on whatever in-game content they unlock. If you have no interest in doing so " +
                    "then it is recommended you simply leave the file as is.");
            }

            File.CreateText(GlobalSaveDataKeysPath);
        }
        // If it does exist, read all keys from the text file and save them into the list.
        else
        {
            GlobalSaveDataKeys = File.ReadAllLines(GlobalSaveDataKeysPath).ToList();
        }
    }

    /// <summary>
    /// Checks whether or not a key has been saved into the GlobalSaveDataKeys text file.
    /// </summary>
    /// <returns>True if the key has been saved, false otherwise.</returns>
    public static bool IsKeyAlreadySaved(string key) => GlobalSaveDataKeys.Contains(key);

    /// <summary>
    /// Saves a specified key into the GlobalSaveDataKeys text file.
    /// </summary>
    public static void SaveKey(string key)
    {
        // If the key already exists, log it as a warning and don't run any code.
        if (GlobalSaveDataKeys.Contains(key))
        {
            CalamityMod.Log.Error($"WARNING! A global save data key \"{key}\" which has already been registered is attempting to be registered again. " +
                "Please report this to the Calamity Mod Team if you see this!");
            return;
        }

        File.AppendAllLines(GlobalSaveDataKeysPath, [key]);
        GlobalSaveDataKeys.Add(key);
    }
}
