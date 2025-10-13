using UnityEngine;

public interface ISaveable
{
    static string TEMP_SAVE_FILENAME = "TempSaveData.json";
    static string SAVE_FILE_1 = "SaveData1.json";

    void LoadSaveFile(string fileName);
    void CreateSaveFile(string fileName);
}
