using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WristMonitor : MonoBehaviour
{
    bool isActive = false;
    [SerializeField] ZeroGravity player;
    [SerializeField] TextMeshProUGUI healthText;
    [SerializeField] TextMeshProUGUI currentObjectiveText;
    [SerializeField] GameObject wristMonitor;
    public List<Objective> mainObjectives = new List<Objective>();
    public List<Objective> completedObjectives = new List<Objective>();

    public class Objective
    {
        public Objective(string objectiveName, string objectiveDescription, string subObjective, bool isCompleted)
        {
            ObjectiveName = objectiveName;
            ObjectiveDescription = objectiveDescription;
            SubObjective = subObjective;
            IsCompleted = isCompleted;
        }

        public string ObjectiveName { get; set; }
        public string ObjectiveDescription { get; set; }
        public string SubObjective { get; set; }
        public bool IsCompleted { get; set; }
    }

    private void Start()
    {
        //mainObjectives.Add(new Objective("Empty", "<color=orange>Current Objective: </color>\n\tReach the Medbay", "<size=8><color=orange>Sub Objective: </color>\n\tReconnect ALAN</size>", false));
        mainObjectives.Add(new Objective("Medbay", "<color=orange>Current Objective: </color>\n\tReach the Medbay", "<size=8><color=orange>Sub Objective: </color>\n\tReconnect ALAN</size>", false));
        mainObjectives.Add(new Objective("Dining Room", "<color=orange>Current Objective: </color>\n\tReach the Dining Room", "<size=8><color=orange>Sub Objective: </color>\n\tReconnect ALAN</size>", false));
        mainObjectives.Add(new Objective("Server Room", "<color=orange>Current Objective: </color>\n\tReach the Server Room", "<size=8><color=orange>Sub Objective: </color>\n\tOverride Manual Lockdown</size>", false));
        mainObjectives.Add(new Objective("Facilities Room", "<color=orange>Current Objective: </color>\n\tReach the Facilities Room", "", false));
    }

    public void ToggleWristMonitor()
    {
        if (!wristMonitor.activeSelf)
        {
            isActive = !isActive;
            this.gameObject.SetActive(isActive);
        }
    }

    private void Update()
    {
        healthText.text = $"Health: {player.PlayerHealth}";
        if(mainObjectives.Count > 0)
        {
            currentObjectiveText.text = $"{mainObjectives[0].ObjectiveDescription}\n{mainObjectives[0].SubObjective}";
        }
    }

    public void CompleteObjective()
    {
        if (mainObjectives.Count > 0)
        {
            mainObjectives[0].IsCompleted = true;
            CheckObjectives();
            Debug.Log(mainObjectives[0].ObjectiveDescription);
        }
    }

    void CheckObjectives()
    {
        if (mainObjectives[0].IsCompleted) 
        {
            completedObjectives.Add(mainObjectives[0]);
            mainObjectives.Remove(mainObjectives[0]);
        }
    }
}
