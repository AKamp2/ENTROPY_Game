using NUnit.Framework;
using System.Collections;
using System.Xml;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class WristMonitor : MonoBehaviour
{
    // Variables
    bool isActive = false;
    public bool IsActive
    {
        get { return isActive; }
        set { isActive = value; }
    }
    [SerializeField] ZeroGravity player;
    [SerializeField] Slider healthSlider;
    [SerializeField] TextMeshProUGUI currentObjectiveText;
    [SerializeField] GameObject wristMonitor;
    // checks the wrist monitor object and manipulates the state of having the wrist monitor accordingly
    public bool HasWristMonitor
    {
        get { return !wristMonitor.activeSelf; }
        set {  wristMonitor.SetActive(!value); }
    }
    [SerializeField] TextMeshProUGUI stimText;
    public List<Objective> mainObjectives = new List<Objective>();
    public List<Objective> completedObjectives = new List<Objective>();
    public RectTransform targetRectTransform;
    public Vector3 targetPosition;
    public float lerpDuration;

    Vector3 startPosition;
    public float duration;
    Vector3 currentPosition;

    [SerializeField] private GameObject[] _displayTexts;
    [SerializeField] ObjectiveUpdate objectiveUpdator;

    

    private bool tutorialShowing = true;
    [SerializeField]
    private DormHallEvent dormHallScript;


    /// <summary>
    /// Public class used to display vital information to the player of how they must proceed
    /// </summary>
    [Serializable]
    public class Objective
    {
        public Objective(string _objectiveName, string _objectiveDescription, string _subObjective, bool _isCompleted)
        {
            objectiveName = _objectiveName;
            objectiveDescription = _objectiveDescription;
            subObjective = _subObjective;
            isCompleted = _isCompleted;
        }
        [SerializeField]
        private string objectiveName;
        public string ObjectiveName
        {
            get { return objectiveName; }
            set { objectiveName = value; }
        }
        [SerializeField]
        private string objectiveDescription;
        public string ObjectiveDescription
        {
            get { return objectiveDescription; }
            set { objectiveDescription = value; }
        }
        [SerializeField]
        private string subObjective;
        public string SubObjective
        {
            get { return subObjective; }
            set { subObjective = value; }
        }
        [SerializeField]
        private bool isCompleted;
        public bool IsCompleted
        {
            get { return isCompleted; }
            set { isCompleted = value; }
        }
    }

    /// <summary>
    /// Creates the objectives for the wristwatch to display
    /// </summary>
    private void Start()
    {
        //mainObjectives.Add(new Objective("Empty", "<color=orange>Current Objective: </color>\nEMPTY", "<size=8><color=orange>Sub Objective: </color>\n\tReconnect ALAN</size>", false));
        mainObjectives.Add(new Objective("Empty", "<size=14><color=orange>Current Objective: </size></color><size=12>\nConnect ALAN to the nearest terminal</size>\n", "<size=10><color=orange>Sub Objective: </color>\n</size>", false));
        mainObjectives.Add(new Objective("Medbay", "<size=14><color=orange>Current Objective: </size></color><size=12>\n  Reach the Medbay</size> \n", "<size=10><color=orange>Sub Objective: </color></size>\n  <size=8>Reconnect ALAN</size>", false));
        mainObjectives.Add(new Objective("Dining Room", "<size=14><color=orange>Current Objective: </size></color><size=12>\nRefill your stims</size> \n", "<size=10><color=orange>Sub Objective: </color></size>\n  <size=8>Heal yourself</size>", false));
        mainObjectives.Add(new Objective("Dining Room", "<size=14><color=orange>Current Objective: </size></color><size=12>\nReach the Dining room</size> \n", "<size=10><color=orange>Sub Objective: </color></size>\n  <size=8></size>", false));
        mainObjectives.Add(new Objective("Server Room", "<size=14><color=orange>Current Objective: </size></color><size=12>\n  Reach the Server Room</size> \n", "<size=10><color=orange>Sub Objective: </color></size>\n  <size=8>Override Manual Lockdown</size>", false));
        mainObjectives.Add(new Objective("Facilities Room", "<size=14><color=orange>Current Objective: </size></color><size=12>\n  Reach the Facilities Room</size> \n", "", false));
        if (targetRectTransform == null) {
            Debug.LogError("TargetRectTransform not assigned");
            return;
        }
        startPosition = targetRectTransform.anchoredPosition3D;
        duration = 0f;
        //this.enabled = false;
        
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

            if(tutorialShowing && dormHallScript != null)
            {
                tutorialShowing = false;
                dormHallScript.FadeOutMonitorTutorial();
            }
        }
        else if (context.canceled)
        {
            isActive = false;
            StartLerp();
            
        }
    }

    public void StartLerp()
    {
        startPosition = new Vector3(-300,0,0);
        duration = 0f;
    }

    

    /// <summary>
    /// Updates health text as well as the current objective. Also handles Lerp logic 
    /// </summary>
    private void FixedUpdate()
    {
        healthSlider.value = player.PlayerHealth;
        stimText.text = $"{player.NumStims}/3";
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
            objectiveUpdator.TextFade();

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

    public void SwitchDisplay(int displayIndex)
    {
        for(int i = 0; i < _displayTexts.Length; i++)
        {
            _displayTexts[i].SetActive(i== displayIndex);
        }
    }

    
}

