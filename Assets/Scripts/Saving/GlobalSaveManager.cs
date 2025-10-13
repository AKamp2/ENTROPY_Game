using UnityEngine;
using System.IO;
using System;

// this contains info about the game, such as the current level and its state
// this is the file the will handle saving and loading
public static class GlobalSaveManager
{
    public static bool loadFromSave = false;
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
    //void OverwriteTempFiles();
    //{
    //    if (SaveFileExists())
    //    {
    //        LoadPersistantSaveFile();
    //        CreateTempSaveFile();
    //    }
    //}
    //bool SaveFileExists(string fileName);
    //{
    //    return File.Exists(Path.Join(Application.persistentDataPath, fileName));
    //}
}
