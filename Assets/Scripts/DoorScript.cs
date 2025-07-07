using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Jobs;
using UnityEngine.Rendering.Universal;

public class DoorScript : MonoBehaviour
{

    public enum States
    {
        Locked,
        Closed,
        Closing,
        Open,
        Opening,
        Broken
    }

    public enum PermissionLevel
    {
        Basic = 0,
        Captain = 1
    }

    [SerializeField]
    private DoorManager doorManager;

    public bool dialogueComplete = false;
    private bool brokenBool = false;

    [SerializeField]
    private States states = States.Closed;
    [SerializeField]
    private bool hasPermissionlevel = false;
    [SerializeField]
    private PermissionLevel level;


    [SerializeField]
    private float speed = 1.0f;

    [SerializeField]
    private float brokenOpenSpeed = 1.5f;
    [SerializeField] 
    private float brokenCloseSpeed = 6.0f;

    [SerializeField]
    private float openSize = 8.0f;
    private float sinTime = 0.0f;
    private Vector3 openPos;
    private Vector3 closedPos;

    //bool to track when doors are closing. Used for collision detection 
    [SerializeField]
    private bool isClosing = false;

    [SerializeField]
    private List<GameObject> buttons = new List<GameObject>();
    [SerializeField]
    private Transform doorPart;

    private bool inRange = false;
    [SerializeField]
    private BoxCollider doorTrigger;
    [SerializeField]
    private DecalProjector decal;

    public AudioSource audioSource;
    public EnvironmentAudio audioManager;

    //private DialogueManager dialogueManager;

    //colors

    private Color redBase = new Color(0.75f, 0.20f, 0.16f);
    private Color redEmis = new Color(1.0f, 0.22f, 0.22f);
    private Color greenBase = new Color(0.0f, 1.0f, 0.1f);
    private Color greenEmis = new Color(0.46f, 1.0f, 0.59f);
    private Color yellowBase = new Color(1.0f, 0.99f, 0.37f);
    private Color yellowEmis = new Color(1.0f, 0.56f, 0.22f);

    //public property for is closing bool
    public bool IsClosing
    {
        get { return isClosing; }
    }

    public States DoorState
    {
        get { return states; }
        set { states = value; }
    }

    public bool HasPermissionLevel
    {
        get { return hasPermissionlevel; }
    }

    public PermissionLevel Permission
    {
        get { return level; }
    }

    public bool InRange
    {
        get { return inRange; }
        set { inRange = value; }
    }
   
    // Start is called before the first frame update
    void Start()
    {

        doorManager = FindFirstObjectByType<DoorManager>();

        GetChildButtons();

        closedPos = doorPart.position;
        Vector3 right = doorPart.forward * -1;
        openPos = closedPos + right * openSize;
        isClosing = false;

        //default unlock
        decal.material = doorManager.UnlockedMaterial;

        if (states == States.Open)
        {
            doorPart.position = openPos;
            SetButtonColor(greenBase, greenEmis);
        }

        if (states == States.Closed)
        {
            SetButtonColor(greenBase, greenEmis);
        }


        if (states == States.Broken)
        {
            SetButtonColor(yellowBase, yellowEmis);
            decal.material = doorManager.WarningMaterial;
        }
        
        if (states == States.Locked)
        {
            SetButtonColor(redBase, redEmis);
            decal.material = doorManager.LockedMaterial;
        }


        //dialogueManager.StartDialogueSequence(0);
    }

    // Update is called once per frame
    void Update()
    {

        // automatic door function
        if (doorTrigger != null) AutomaticDoor();

        // handles different door interactions
        switch (states)
        {
            case States.Opening:
                {
                    if (doorPart.position != openPos)
                    {
                        sinTime += Time.deltaTime * speed;
                        sinTime = Mathf.Clamp(sinTime, 0, Mathf.PI);
                        // sin function
                        float t = 0.5f * Mathf.Sin(sinTime - Mathf.PI / 2f) + 0.5f;
                        doorPart.position = Vector3.Lerp(closedPos, openPos, t);
                    }
                    else
                    {
                        states = States.Open;
                        SetButtonColor(greenBase, greenEmis);
                        decal.material = doorManager.UnlockedMaterial;
                        sinTime = 0.0f;
                    }


                    break;
                }

            case States.Closing:
                {
                    if (doorPart.position != closedPos)
                    {
                        sinTime += Time.deltaTime * speed;
                        sinTime = Mathf.Clamp(sinTime, 0, Mathf.PI);
                        // sin function
                        float t = 0.5f * Mathf.Sin(sinTime - Mathf.PI / 2f) + 0.5f;
                        doorPart.position = Vector3.Lerp(openPos, closedPos, t);

                        //set isClosing as false
                        isClosing = true;
                    }
                    else
                    {
                        states = States.Closed;
                        SetButtonColor(greenBase, greenEmis);
                        decal.material = doorManager.UnlockedMaterial;
                        sinTime = 0.0f;

                        //set isClosing as false
                        isClosing = false;
                    }

                    break;
                }

            case States.Broken:
                {
                    if (!brokenBool && doorPart.position != openPos)
                    {
                        sinTime += Time.deltaTime * brokenOpenSpeed;
                        sinTime = Mathf.Clamp(sinTime, 0, Mathf.PI);
                        // sin function
                        float t = 0.5f * Mathf.Sin(sinTime - Mathf.PI / 2f) + 0.5f;
                        doorPart.position = Vector3.Lerp(closedPos, openPos, t);

                        isClosing = false;

                        if (doorPart.position == openPos)
                        {
                            brokenBool = true;
                            sinTime = 0.0f;
                        }

                    }
                    else if (brokenBool && doorPart.position != closedPos)
                    {
                        sinTime += Time.deltaTime * brokenCloseSpeed;
                        sinTime = Mathf.Clamp(sinTime, 0, Mathf.PI);
                        // sin function
                        float t = 0.5f * Mathf.Sin(sinTime - Mathf.PI / 2f) + 0.5f;
                        doorPart.position = Vector3.Lerp(openPos, closedPos, t);

                        isClosing = true;

                        if (doorPart.position == closedPos)
                        {
                            brokenBool = false;
                            sinTime = 0.0f;
                        }
                    }

                    break;

                }

        }


    }

    public void UseDoor()
    {
        if (states == States.Open)
        {
            states = States.Closing;
        }
        else if (states == States.Closed)
        {
            audioManager.playDoorOpenAudio(speed, this);
            states = States.Opening;
        }

    }

    public void SetVisualStatus(States state)
    {
        if (state == States.Closed)
        {
            SetButtonColor(greenBase, greenEmis);
            decal.material = doorManager.UnlockedMaterial;
        }
    }

    private void DialogueEnd(int sequenceIndex)
    {
        // Only open door if the first dialogue sequence is completed
        if (sequenceIndex == 0)
        {
            Debug.Log($"Dialogue {sequenceIndex} completed, door can be opened.");
            dialogueComplete = true;
        }
    }

    public void PuzzleComplete()
    {
        if (states == States.Locked)
        {
            states = States.Opening;
        }
    }

    public void LockDoor()
    {
        states = States.Locked;
    }

    public void UnlockDoor()
    {
        if (states == States.Locked)
        {
            audioManager.playDoorOpenAudio(speed, this);
            states = States.Opening;
        }
    }

    private void GetChildButtons()
    {
        foreach (Transform child in transform)
        {
            if (child.gameObject.tag == "DoorButton")
            {
                buttons.Add(child.gameObject);
            }
        }
    }

    private void AutomaticDoor()
    {
        if (inRange)
        {
            if (states != States.Open && states != States.Opening)
            {
                UseDoor();
            }

        }
        else
        {
            if (states != States.Closed && states != States.Closed)
            {
                UseDoor();
            }
        }
    }

    private void SetButtonColor(Color baseColor, Color emisColor)
    {
        foreach(GameObject button in buttons)
        {
            Material m = button.GetComponent<Renderer>().material;
            m.SetColor("_BaseColor", baseColor);
            m.SetColor("_EmissionColor", emisColor);
        }
    }
    
}
