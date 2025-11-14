using System.Collections;
using UnityEngine;

public class BodyScareEvent : MonoBehaviour
{
    private ZeroGravity player;
    [SerializeField]
    private DoorScript bodyDoor;
    [SerializeField]
    private DoorScript[] doorsToUnlock;
    [SerializeField]
    private Collider[] barsToGrab;
    [SerializeField]
    private GameObject body;
    [SerializeField]
    private Rigidbody bodyRb;

    [SerializeField]
    private GameObject tempCollider;
    private DialogueManager dialogueManager;

    public bool waitForGrabbingBar = false;

    public Light[] escapePodLights;

    public AnimationCurve lightCurve;

    public float intensityMultiplier;

    public GameObject bodyPos;

    public GameplayBeatAudio audioManager;

    private AmbientController ambientController;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        dialogueManager = FindFirstObjectByType<DialogueManager>();
        ambientController = FindFirstObjectByType<AmbientController>();
        player = FindFirstObjectByType<ZeroGravity>();

        foreach(Light light in escapePodLights)
        {
            light.intensity = 0;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(waitForGrabbingBar)
        {
           foreach(Collider bar in barsToGrab)
            {
                if(player.GrabbedBar != null)
                {
                    //Debug.Log(player.GrabbedBar.name);
                }
                
                if (bar == player.GrabbedBar)
                {
                    //Debug.Log("Grabbed bar detected");
                    waitForGrabbingBar = false;
                    StartCoroutine(PlayBodyScare());
                }
            }
        }
    }

    public void SetOffBarCheck()
    {
        waitForGrabbingBar = true;
    }

    public IEnumerator PlayBodyScare()
    {

        
        

        //bodyRb.AddForce(new Vector3(-.5f, -1, 0) * 30f, ForceMode.Impulse);
        

        bodyDoor.SetState(DoorScript.States.JoltOpen);

        

        //put any body movement Logic here;


        yield return new WaitUntil(() => bodyDoor.aboutToJolt == true);

        body.SetActive(true);
        body.transform.position = bodyPos.transform.position;
        bodyRb.isKinematic = false;
        //turn off the collider that keeps the player from moving through the door prematurely.
        tempCollider.SetActive(false);

        //bodyRb.AddForce(new Vector3(0f, -1f, 0f) * .5f, ForceMode.Impulse);

        //foreach (Rigidbody rb in bodyRb.GetComponentsInChildren<Rigidbody>())
        //{
        //    rb.AddForce(new Vector3(0f, -1f, 0f) * 5f, ForceMode.Impulse);
        //}

        //yield return new WaitUntil(() => bodyDoor.showingBody);

        yield return new WaitForSeconds(1.5f);
        ActivateLights();

        

       

        //ambientController.Progress();


    }

    public void ActivateLights()
    {
        StartCoroutine(TurnOnLights(3));
    }

    public IEnumerator TurnOnLights(float duration)
    {
        float elapsed = 0f;
        audioManager.playBodyStinger();

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float intensity = lightCurve.Evaluate(t) * intensityMultiplier;

            foreach (Light light in escapePodLights)
            {
                light.intensity = intensity;
            }

            yield return null;
        }

        
        // Ensure final value is set
        float finalIntensity = lightCurve.Evaluate(1f) * intensityMultiplier;
        foreach (Light light in escapePodLights)
        {
            light.intensity = finalIntensity;
        }

        foreach (DoorScript door in doorsToUnlock)
        {
            door.SetState(DoorScript.States.Closed);
        }

        yield return new WaitForSeconds(5.5f);

        dialogueManager.StartDialogueSequence(8, 0f);

        yield return new WaitForSeconds(5f);
    }
}
