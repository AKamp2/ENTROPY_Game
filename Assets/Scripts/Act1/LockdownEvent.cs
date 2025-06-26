using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class LockdownEvent : MonoBehaviour
{
    [SerializeField]
    private GameObject playerObject;
    private ZeroGravity player;
    [SerializeField]
    private Collider playerCollider;

    [SerializeField]
    private BoxCollider DoorTrigger;
    [SerializeField]
    private GameObject lever;
    [SerializeField]
    private GameObject wrist;
    [SerializeField]
    private DoorScript[] doors;
    [SerializeField]
    private DoorScript brokenDoor;

    [SerializeField]
    private DoorScript bodyDoor;


    [SerializeField]
    private Material leverMaterial;

    // lockdown bools
    private bool isActive;
    private bool canPull;

    // wrist monitor
    private bool canGrab;
    private bool isGrabbable;

    public GameplayBeatAudio audio;

    public bool CanPull
    {
        get { return canPull; }
        set { canPull = value; }
    }

    public bool IsActive
    {
        get { return isActive; }
    }

    public bool CanGrab
    {
        get { return canGrab; }
        set { canGrab = value; }
    }

    public bool IsGrabbable
    {
        get { return isGrabbable; }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = playerObject.GetComponent<ZeroGravity>();

        DoorTrigger.enabled = false;
        // checks if player is currently hovering over lever
        canPull = false;
        // checks if system is able to be turned on
        isActive = true;

        canGrab = false;
        isGrabbable = true;

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OpenDoors()
    {
        
        foreach (DoorScript door in doors)
        {
            StartCoroutine(OpenDoorWithDelay(door));
        }
        StartCoroutine(WaitForBodyVisible());
    }

    //adding slight delay to door to prevent phasing.
    private IEnumerator OpenDoorWithDelay(DoorScript door)
    {
        float randomDelay = Random.Range(0f, 0.2f); // Adjust range if needed
        yield return new WaitForSeconds(randomDelay);
        door.UseDoor();
    }

    private IEnumerator WaitForBodyVisible()
    {
        yield return new WaitForSeconds(3f);
        audio.playBodyStinger();

    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (canPull && isActive)
        {
            // open the broken door first
            brokenDoor.DoorState = DoorScript.States.Opening;
            lever.GetComponent<Renderer>().material = leverMaterial;

            DoorTrigger.enabled = true;
            isActive = false;

            // begin lighting and audio queues
            StartCoroutine(PlayLockdownFX());
            
        }

        if (canGrab && IsGrabbable)
        {
            player.AccessPermissions[0] = true;

            isGrabbable = false;

            wrist.SetActive(false);
        }
    }

    private IEnumerator PlayLockdownFX()
    {
        audio.playPowerCut();
        yield return new WaitForSeconds(6f);

        audio.playPowerOn();
    }

}
