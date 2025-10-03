using UnityEngine;
using System.IO;
using System;

// this contains info about the game, such as the current level and its state
// this is the file the will handle saving and loading
public class GlobalSaveManager : MonoBehaviour
{
    // reference to self
    public static GlobalSaveManager Instance { get; private set; }
    public SaveData Data;
    static string TEMP_SAVE_FILENAME = "TempSaveData.json";
    static string SAVE_FILE_1 = "SaveData1.json";
    private bool loadFromSave = false;
    public bool LoadFromSave
    {
        get { return loadFromSave; }
        set { loadFromSave = value; }
    }
    private void Awake()
    {
        // Debug.Log("Save script initialized on load!");
        // Ensure that there is only one GlobalSaveManager
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        } else
        {
            // Overwrite temp file on game launch
            OverwriteTempFile();
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }
    public void OverwriteTempFile()
    {
        if (SaveFileExists())
        {
            LoadPersistantSaveFile();
            CreateTempSaveFile();
        }
    }
    public static bool SaveFileExists()
    {
        return File.Exists(Path.Join(Application.persistentDataPath, SAVE_FILE_1));
    }
    public void LoadTempSaveFile()
    {
        string path = Application.persistentDataPath;
        string loadedData = LoadTextFromFile(path, TEMP_SAVE_FILENAME);
        Data = JsonUtility.FromJson<SaveData>(loadedData);
    }
    public void LoadPersistantSaveFile()
    {
        string path = Application.persistentDataPath;
        string loadedData = LoadTextFromFile(path, SAVE_FILE_1);
        Data = JsonUtility.FromJson<SaveData>(loadedData);
    }

    private string LoadTextFromFile(string path, string fileName)
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

    public void CreateTempSaveFile()
    {
        string json = JsonUtility.ToJson(Data);
        // Debug.Log("this is the Save Data as a string:");
        // Debug.Log(json);
        string path = Application.persistentDataPath;
        SaveTextToFile(path, TEMP_SAVE_FILENAME, json);
    }
    // this is a save file that will be loaded when the game is loaded
    public void CreatePersistantSaveFile()
    {
        string json = JsonUtility.ToJson(Data);
        // Debug.Log("this is the Save Data as a string:");
        // Debug.Log(json);
        string path = Application.persistentDataPath;
        SaveTextToFile(path, SAVE_FILE_1, json);
    }

    private void SaveTextToFile(string path, string fileName, string data)
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
}
