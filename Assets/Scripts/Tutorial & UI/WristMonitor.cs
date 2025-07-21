using NUnit.Framework;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class WristMonitor : MonoBehaviour
{
    // Variables
    bool isActive = false;
    [SerializeField] ZeroGravity player;
    [SerializeField] TextMeshProUGUI healthText;
    [SerializeField] TextMeshProUGUI currentObjectiveText;
    [SerializeField] GameObject wristMonitor;
    public List<Objective> mainObjectives = new List<Objective>();
    public List<Objective> completedObjectives = new List<Objective>();
    public RectTransform targetRectTransform;
    public Vector3 targetPosition;
    public float lerpDuration;

    Vector3 startPosition;
    public float duration;
    Vector3 currentPosition;

    /// <summary>
    /// Public class used to display vital information to the player of how they must proceed
    /// </summary>
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

    /// <summary>
    /// Creates the objectives for the wristwatch to display
    /// </summary>
    private void Start()
    {
        //mainObjectives.Add(new Objective("Empty", "<color=orange>Current Objective: </color>\n\tReach the Medbay", "<size=8><color=orange>Sub Objective: </color>\n\tReconnect ALAN</size>", false));
        mainObjectives.Add(new Objective("Medbay", "<color=orange>Current Objective: </color>\n\tReach the Medbay", "<size=8><color=orange>Sub Objective: </color>\n\tReconnect ALAN</size>", false));
        mainObjectives.Add(new Objective("Dining Room", "<color=orange>Current Objective: </color>\n\tReach the Dining Room", "<size=8><color=orange>Sub Objective: </color>\n\tReconnect ALAN</size>", false));
        mainObjectives.Add(new Objective("Server Room", "<color=orange>Current Objective: </color>\n\tReach the Server Room", "<size=8><color=orange>Sub Objective: </color>\n\tOverride Manual Lockdown</size>", false));
        mainObjectives.Add(new Objective("Facilities Room", "<color=orange>Current Objective: </color>\n\tReach the Facilities Room", "", false));
        if (targetRectTransform == null) {
            Debug.LogError("TargetRectTransform not assigned");
            return;
        }
        startPosition = targetRectTransform.anchoredPosition3D;
        duration = 0f;
    }



    /// <summary>
    /// Public method called by the zero gravity controller to turn the monitor on and off
    /// </summary>
    public void ToggleWristMonitor(InputAction.CallbackContext context)
    {
        if (context.performed && !wristMonitor.activeSelf)
        {
            isActive = true;
            this.gameObject.SetActive(true);
            StartLerp();
        }
        else if (context.canceled)
        {
            isActive = false;
            StartLerp();
            
        }
    }

    public void StartLerp()
    {
        startPosition = new Vector3(-300,110,0);
        duration = 0f;
    }

    

    /// <summary>
    /// Updates health text as well as the current objective. Also handles Lerp logic 
    /// </summary>
    private void FixedUpdate()
    {
        healthText.text = $"Health: {player.PlayerHealth}";
        if(mainObjectives.Count > 0)
        {
            currentObjectiveText.text = $"{mainObjectives[0].ObjectiveDescription}\n{mainObjectives[0].SubObjective}";
        }
        currentPosition = targetRectTransform.anchoredPosition3D;
        if(isActive && duration < lerpDuration)
        {
            duration += Time.deltaTime;
            float t = duration / lerpDuration;

            targetRectTransform.anchoredPosition3D = Vector3.Lerp(startPosition, targetPosition, t);
        }
        if (!isActive && duration < lerpDuration)
        {
            duration += Time.deltaTime;
            float t = duration / lerpDuration;

            targetRectTransform.anchoredPosition3D = Vector3.Lerp(currentPosition, startPosition, t);

            if (targetRectTransform.anchoredPosition3D == startPosition) {
                this.gameObject.SetActive(false);
            }
        }
    }

    /// <summary>
    /// Public method called outside of script (by objective triggers) to complete the first objective in the list
    /// </summary>
    public void CompleteObjective()
    {
        if (mainObjectives.Count > 0)
        {
            mainObjectives[0].IsCompleted = true;
            CheckObjectives();
            //Debug.Log(mainObjectives[0].ObjectiveDescription);
        }
    }

    /// <summary>
    /// Cleans up list by removing completed objectives and adding them to a seperate list
    /// </summary>
    void CheckObjectives()
    {
        if (mainObjectives[0].IsCompleted) 
        {
            completedObjectives.Add(mainObjectives[0]);
            mainObjectives.Remove(mainObjectives[0]);
        }
    }
}
