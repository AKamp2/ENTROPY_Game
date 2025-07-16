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
    public List<Objective> objectives = new List<Objective>();

    public class Objective
    {
        public Objective(string objectiveName, string objectiveDescription, bool isCompleted)
        {
            ObjectiveName = objectiveName;
            ObjectiveDescription = objectiveDescription;
            IsCompleted = isCompleted;
        }

        public string ObjectiveName { get; set; }
        public string ObjectiveDescription { get; set; }
        public bool IsCompleted { get; set; }
    }

    private void Start()
    {
        objectives.Add(new Objective("Medbay", "Reach the Medbay", false));
    }

    public void ToggleWristMonitor()
    {
        isActive = !isActive;
        this.gameObject.SetActive(isActive);
    }

    private void Update()
    {
        healthText.text = $"Health: {player.PlayerHealth}";
        currentObjectiveText.text = $"<color=orange>Current Objective: </color>\n\t{objectives[0].ObjectiveDescription}";
    }
}
