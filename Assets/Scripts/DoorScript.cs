using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Jobs;

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


    private DoorManager doorManager;

    public bool dialogueComplete = false;
    private bool brokenBool = false;

    [SerializeField]
    private States states = States.Closed;

    [SerializeField]
    private float speed = 1.0f;
    [SerializeField]
    private float openSize = 8.0f;
    private float sinTime = 0.0f;
    private Vector3 openPos;
    private Vector3 closedPos;

    [SerializeField]
    private List<GameObject> buttons = new List<GameObject>();
    [SerializeField]
    private Transform doorPart;

    //private DialogueManager dialogueManager;

    //colors

    private Color redBase = new Color(0.75f, 0.20f, 0.16f);
    private Color redEmis = new Color(1.0f, 0.22f, 0.22f);
    private Color greenBase = new Color(0.0f, 1.0f, 0.1f);
    private Color greenEmis = new Color(0.46f, 1.0f, 0.59f);
    private Color yellowBase = new Color(1.0f, 0.99f, 0.37f);
    private Color yellowEmis = new Color(1.0f, 0.56f, 0.22f);

    public States DoorState
    {
        get { return states; }
    }

   
    // Start is called before the first frame update
    void Start()
    {
        GetChildButtons();

        closedPos = doorPart.position;
        Vector3 right = doorPart.forward * -1;
        openPos = closedPos + right * openSize;

        if (states == States.Open)
        {
            doorPart.position = openPos;
            SetButtonColor(redBase, redEmis);
        }

        if (states == States.Broken)
        {
            SetButtonColor(yellowBase, yellowEmis);
        }
        else if (states == States.Locked)
        {
            SetButtonColor(redBase, redEmis);
        }

        doorManager = FindObjectOfType<DoorManager>();

        //dialogueManager.StartDialogueSequence(0);
    }

    // Update is called once per frame
    void Update()
    {

        // handles different door interactions
        switch(states)
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
                        SetButtonColor(redBase, redEmis);
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
                    }
                    else
                    {
                        states = States.Closed;
                        SetButtonColor(greenBase, greenEmis);
                        sinTime = 0.0f;
                    }

                        break;
                }

            case States.Broken:
                {
                    if (!brokenBool && doorPart.position != openPos)
                    {
                        sinTime += Time.deltaTime * 1.5f;
                        sinTime = Mathf.Clamp(sinTime, 0, Mathf.PI);
                        // sin function
                        float t = 0.5f * Mathf.Sin(sinTime - Mathf.PI / 2f) + 0.5f;
                        doorPart.position = Vector3.Lerp(closedPos, openPos, t);

                        if (doorPart.position == openPos)
                        {
                            brokenBool = true;
                            sinTime = 0.0f;
                        }

                    }
                    else if (brokenBool && doorPart.position != closedPos)
                    {
                        sinTime += Time.deltaTime * 6.0f;
                        sinTime = Mathf.Clamp(sinTime, 0, Mathf.PI);
                        // sin function
                        float t = 0.5f * Mathf.Sin(sinTime - Mathf.PI / 2f) + 0.5f;
                        doorPart.position = Vector3.Lerp(openPos, closedPos, t);

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


    public void OnInteract(InputAction.CallbackContext context)
    {
        // if UI is active you can press button
        if(doorManager.DoorUI.activeInHierarchy && doorManager.CurrentSelectedDoor == transform.gameObject)
        {
            if (states == States.Open)
            {
                states = States.Closing;
            }
            else if (states == States.Closed)
            {
                states = States.Opening;
            }
            

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
        if(states == States.Locked)
        {
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

    private void SetButtonColor(Color baseColor, Color emisColor)
    {
        foreach(GameObject button in buttons)
        {
            Material m = button.GetComponent<Renderer>().material;
            m.SetColor("_Color", baseColor);
            m.SetColor("_EmissionColor", emisColor);
        }
    }
    
}
