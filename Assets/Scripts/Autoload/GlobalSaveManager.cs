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
    static string FILENAME = "SaveData.json";
    private bool loadFromSave = false;
    public bool LoadFromSave
    {
        get { return loadFromSave; }
        set { loadFromSave = value; }
    }
    private void Awake()
    {
        Debug.Log("Save script initialized on load!");
        // Ensure that there is only one GlobalSaveManager
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        } else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    public void LoadSaveFile()
    {
        string path = Application.dataPath;
        string loadedData = LoadTextFromFile(path, FILENAME);
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
            Console.WriteLine(e);
        }
        return data;
    }

    public void CreateSaveFile()
    {
        string json = JsonUtility.ToJson(Data);
        string path = Application.dataPath;
        SaveTextToFile(path, FILENAME, json);
    }

    private void SaveTextToFile(string path, string fileName, string data)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        path = Path.Join(path, fileName);

        Console.WriteLine(path);

        try
        {
            File.WriteAllText(path, data);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}
