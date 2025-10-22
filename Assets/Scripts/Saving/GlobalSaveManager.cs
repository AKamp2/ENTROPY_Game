using UnityEngine;
using System.IO;
using System;
using System.Linq;

// this contains info about the game, such as the current level and its state
// this is the file the will handle saving and loading
public static class GlobalSaveManager
{
    public static bool LoadFromSave = false;
    public static bool SavedWithTerminal = false;
    public static void SaveGame(bool permanent)
    {
        ISaveable[] saveables = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<ISaveable>().ToArray();
        foreach (var saveable in saveables)
        {
            // permanent save file
            if (permanent) saveable.CreateSaveFile(GenerateFileName(saveable, true));
            // temporary save file - always make one of these
            saveable.CreateSaveFile(GenerateFileName(saveable, false));
        }
    }
    //public static void LoadGame(bool permanent)
    //{
    //    ISaveable[] saveables = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<ISaveable>().ToArray();
    //    foreach (var saveable in saveables)
    //    {
    //        LoadSavable(saveable, permanent);
    //    }
    //}
    public static void LoadSavable(ISaveable saveable, bool permanent)
    {
        // permanent save file
        if (permanent)
        {
            saveable.LoadSaveFile(GenerateFileName(saveable, true));
        }
        // temporary save file
        else
        {
            saveable.LoadSaveFile(GenerateFileName(saveable, false));
        }
    }
    private static string GenerateFileName(ISaveable saveable, bool permanent)
    {
        string typeName = saveable.GetType().Name;
        if (permanent) return $"{typeName}_Save.json";
        return $"{typeName}_Temp.json";
    }
    public static void SaveTextToFile(string path, string fileName, string data)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        path = Path.Join(path, fileName);

        // Debug.Log(path);

        try
        {
            File.WriteAllText(path, data);
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
    }
    public static string LoadTextFromFile(string path, string fileName)
    {
        string data = null;
        path = Path.Join(path, fileName);

        if (!File.Exists(path)) return null;
        try
        {
            data = File.ReadAllText(path);
        }
        catch (Exception e)
        {
            Debug.Log(e);
        }
        return data;
    }

    // TODO:this is broken and needs to search for files NOT saveables
    public static void OverwriteTempFiles()
    {
        ISaveable[] saveables = UnityEngine.Object.FindObjectsByType<MonoBehaviour>(FindObjectsSortMode.None).OfType<ISaveable>().ToArray();
        foreach (var saveable in saveables)
        {
            // load the text from the perm file and save it to the temp file
            string tempFile = GenerateFileName(saveable, false);
            string permFile = GenerateFileName(saveable, true);
            string fileText = LoadTextFromFile(Application.persistentDataPath, permFile);
            SaveTextToFile(Application.persistentDataPath, tempFile, fileText);
        }
    }
    //public static bool SaveFileExists(string fileName)
    //{
    //    return File.Exists(Path.Join(Application.persistentDataPath, fileName));
    //}
}
