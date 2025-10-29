using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static CheckpointManager;

public class AirBreachTrigger : MonoBehaviour, ISaveable
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] private VentScript vent1;
    [SerializeField] private VentScript vent2;
    [SerializeField] private VentScript vent3;
    private bool triggered = false;

    void Start()
    {
        // continue from save
        if (GlobalSaveManager.LoadFromSave) GlobalSaveManager.LoadSavable(this, false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            GetComponent<Collider>().enabled = false;
            StartCoroutine(TriggerVents());

        }
    }

    private IEnumerator TriggerVents()
    {
        vent1.TurnOn();
        yield return new WaitForSeconds(3f);
        vent2.TurnOn();
        yield return new WaitForSeconds(0.5f);
        vent3.TurnOn();
    }

    public void LoadSaveFile(string fileName)
    {
        // this will load data from the file to a variable we will use to change this objects data
        string path = Application.persistentDataPath;
        string loadedData = GlobalSaveManager.LoadTextFromFile(path, fileName);
        if (loadedData != null && loadedData != "")
        {
            if (loadedData == "True")
            {
                GetComponent<Collider>().enabled = false;
                triggered = true;
                vent1.TurnOn();
                vent2.TurnOn();
                vent3.TurnOn();
            }
        }
    }

    public void CreateSaveFile(string fileName)
    {
        // this will create a file backing up the data we give it
        string path = Application.persistentDataPath;
        GlobalSaveManager.SaveTextToFile(path, fileName, triggered.ToString());
    }
}
