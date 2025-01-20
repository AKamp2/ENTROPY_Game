using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorManager : MonoBehaviour
{


    [SerializeField]
    private DoorScript[] doors;

    public GameObject DoorUI = null;

    private GameObject currentSelectedDoor = null;

    public GameObject CurrentSelectedDoor
    {
        get { return currentSelectedDoor; }
        set { currentSelectedDoor = value; }
    }




    // Start is called before the first frame update
    void Start()
    {
        doors = transform.Find("DoorGroup").GetComponentsInChildren<DoorScript>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }


}
