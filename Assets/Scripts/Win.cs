using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Win : MonoBehaviour
{
    private bool winCondition = false;
    [SerializeField]
    private DoorScript winDoor;

    private void Start()
    {
        winCondition = false;
    }
    public bool WinCondition
    {
        get { return winCondition; }
        set { winCondition = value; }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Was triggerd by: " + other.name);

        if (other.CompareTag("Player"))
        {
            winCondition = true;
        }
    }

    private void Update()
    {
        //if (winDoor.DoorState == DoorScript.States.Closed && winCondition != true)
        //{
        //    winCondition = true;
        //}

    }

}
