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
    public List<Objective> mainObjectives = new List<Objective>();

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
        mainObjectives.Add(new Objective("Medbay", "Reach the Medbay", "Reconnect ALAN", false));
        mainObjectives.Add(new Objective("Server Room", "Reach the Server Room", "Override Manual Lockdown", false));
        mainObjectives.Add(new Objective("Server Room", "Reach the Faciliteis Room","", false));
    }

    public void ToggleWristMonitor()
    {
        isActive = !isActive;
        this.gameObject.SetActive(isActive);
    }

    private void Update()
    {
        healthText.text = $"Health: {player.PlayerHealth}";
        currentObjectiveText.text = $"<color=orange>Current Objective: </color>\n\t{mainObjectives[0].ObjectiveDescription}\n<size=8><color=orange>Sub Objective: </color>\n\t{mainObjectives[0].SubObjective}</size>";
    }
}
