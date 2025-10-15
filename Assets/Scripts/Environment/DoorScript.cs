using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Jobs;
using UnityEngine.ProBuilder;
using UnityEngine.Rendering.Universal;
public class DoorScript : MonoBehaviour
{
    [Serializable]
    public enum States
    {
        Locked,
        Closed,
        Closing,
        Open,
        Opening,
        Broken,
        BrokenShort,
        JoltOpen
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
    private float openDuration = 1.8f;
    [SerializeField]
    private float closeDuration = 2.3f;


    [SerializeField]
    private float brokenOpenDuration = 2.7f;
    [SerializeField]
    private float brokenCloseDuration = 1f;
    [SerializeField]
    private float brokenDoorPause = 2f;

    [SerializeField]
    private float openSize = 8.0f;
    //private float sinTime = 0.0f;
    private Vector3 openPos;
    private Vector3 closedPos;
    private Vector3 shortPos;
    private Vector3 midPos;
    private Vector3 bodyPos;


    [SerializeField]
    private GameObject shortReference;
    [SerializeField]
    private GameObject midReference;
    [SerializeField]
    private GameObject bodyOpenReference;




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
    [SerializeField]
    private bool showSparks;

    public AudioSource startAudioSource;
    public AudioSource middleAudioSource;
    public AudioSource endAudioSource;

    public bool playDoorAlarm = false;

    public EnvironmentAudio audioManager;
    public Sparks[] sparks;

    private bool isShortBreakOver = false;

    public bool showingBody = false;
    public bool aboutToJolt = false;

    [Header("Hologram")]
    [SerializeField]
    private Texture2D[] textLabels;
    [SerializeField]
    public MeshRenderer[] hologramGroup;
    [SerializeField]
    public Coroutine fadeRoutine;
    public bool hologramActive = false;
    public float lightOff = 0.001f;
    public float lightOn = 0.015f;


    [Header("Sound Effects")]
    [SerializeField]
    private AudioClip doorOpenStart;
    [SerializeField]
    private AudioClip doorOpenMiddle;
    [SerializeField]
    private AudioClip doorOpenEnd;
    [SerializeField]
    private AudioClip doorCloseStart;
    [SerializeField]
    private AudioClip doorCloseMiddle;
    [SerializeField]
    private AudioClip doorCloseEnd;
    [SerializeField]
    private AudioClip doorBrokenStart;
    [SerializeField]
    private AudioClip doorBrokenEnd;
    [SerializeField]
    private AudioClip doorBrokenSlam;
    [SerializeField]
    private AudioClip doorBrokenSlamShort;
    [SerializeField]
    private AudioClip doorBrokenJolt;
    [SerializeField]
    private AudioClip doorStuck;
    [SerializeField]
    private AudioClip doorAlarm;



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
        set
        {
            states = value;
            // track door updates for saving purposes
            doorManager.StoreDoorStates();
        }
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

    private void Awake()
    {
        doorManager = FindFirstObjectByType<DoorManager>();
    }

    // Start is called before the first frame update
    void Start()
    {
        GetChildButtons();

        closedPos = doorPart.position;
        Vector3 right = doorPart.forward * -1;
        openPos = closedPos + right * openSize;
        isClosing = false;

        //default unlock
        decal.material = doorManager.UnlockedMaterial;
        UnlockHologram();
        ApplyTextSign();

        if (states == States.Open)
        {
            doorPart.position = openPos;
            SetButtonColor(greenBase, greenEmis);
        }

        if (states == States.Closed)
        {
            doorPart.position = closedPos;
            SetButtonColor(greenBase, greenEmis);
        }


        if (states == States.Broken)
        {
            SetButtonColor(yellowBase, yellowEmis);
            decal.material = doorManager.WarningMaterial;
            BrokenHologram();
            StartCoroutine(HandleBrokenDoorLoop());
        }

        if (states == States.Locked)
        {
            doorPart.position = closedPos;
            SetButtonColor(redBase, redEmis);
            decal.material = doorManager.LockedMaterial;
            LockHologram();
        }

        if (states == States.BrokenShort)
        {
            SetButtonColor(yellowBase, yellowEmis);
            decal.material = doorManager.WarningMaterial;
            StartCoroutine(HandleBrokenDoorShort());
        }

        foreach (Sparks spark in sparks)
        {
            if (spark != null)
            {
                spark.ToggleSparks(showSparks);
            }

        }

        if (midReference != null)
        {
            midPos = midReference.transform.position;
        }

        if (shortReference != null)
        {
            shortPos = shortReference.transform.position;
        }

        if (bodyOpenReference != null)
        {
            bodyPos = bodyOpenReference.transform.position;
        }


        //dialogueManager.StartDialogueSequence(0);
    }

    // Update is called once per frame
    void Update()
    {
        // automatic door function
        if (doorTrigger != null) AutomaticDoor();

    }

    private IEnumerator MoveDoor(Vector3 fromPos, Vector3 toPos, float duration, System.Action onComplete)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = 0.5f * Mathf.Sin((elapsed / duration) * Mathf.PI - Mathf.PI / 2f) + 0.5f;
            doorPart.position = Vector3.Lerp(fromPos, toPos, t);
            elapsed += Time.deltaTime;
            yield return null;
        }

        doorPart.position = toPos;
        onComplete?.Invoke();
    }

    public void UseDoor()
    {
        if (states == States.Open)
        {
            StartCoroutine(CloseDoor());

        }
        else if (states == States.Closed)
        {
            StartCoroutine(OpenDoor());
        }
    }

    private IEnumerator OpenDoor()
    {
        if (playDoorAlarm)
        {
            endAudioSource.clip = doorAlarm;
            endAudioSource.Play();
            yield return new WaitForSeconds(3f);
        }
        StartCoroutine(FadeOutAndStop(endAudioSource, 0.3f));


        startAudioSource.clip = doorOpenStart;
        startAudioSource.Play();

        DoorState = States.Opening;
        SetButtonColor(greenBase, greenEmis);
        decal.material = doorManager.UnlockedMaterial;

        StartCoroutine(FadeOutAndStop(startAudioSource, 0.3f));
        middleAudioSource.clip = doorOpenMiddle;
        middleAudioSource.Play();

        yield return MoveDoor(closedPos, openPos, openDuration, () =>
        {
            DoorState = States.Open;
            //sinTime = 0f;
        });

        StartCoroutine(FadeOutAndStop(middleAudioSource, 0.3f));

        endAudioSource.clip = doorOpenEnd;
        endAudioSource.Play();
    }

    private IEnumerator CloseDoor()
    {
        StartCoroutine(FadeOutAndStop(endAudioSource, 0.3f));

        startAudioSource.clip = doorCloseStart;
        startAudioSource.Play();

        DoorState = States.Closing;
        SetButtonColor(greenBase, greenEmis);
        decal.material = doorManager.UnlockedMaterial;
        isClosing = true;

        StartCoroutine(FadeOutAndStop(startAudioSource, 0.3f));
        middleAudioSource.clip = doorCloseMiddle;
        middleAudioSource.Play();

        yield return MoveDoor(openPos, closedPos, closeDuration, () =>
        {
            DoorState = States.Closed;
            //sinTime = 0f;
            isClosing = false;
        });

        StartCoroutine(FadeOutAndStop(middleAudioSource, 0.3f));

        endAudioSource.clip = doorCloseEnd;
        endAudioSource.Play();
    }

    private IEnumerator HandleBrokenDoorLoop()
    {
        while (states == States.Broken)
        {
            // Play opening start sound
            startAudioSource.clip = doorBrokenStart;
            startAudioSource.Play();

            // Wait for door to fully open
            StartFade(1.0f, lightOff, 0.1f);
            yield return MoveDoor(closedPos, openPos, brokenOpenDuration, null);


            // Pause before slamming shut
            yield return new WaitForSeconds(brokenDoorPause);

            // Play slam SFX
            middleAudioSource.clip = doorBrokenSlam;
            middleAudioSource.Play();

            isClosing = true;

            //hologram will be turned on while closing, best solution for now
            StartFade(0.0f, lightOn, 0.5f);

            // Wait for door to fully close
            yield return MoveDoor(openPos, closedPos, brokenCloseDuration, () => isClosing = false);
           
        }
    }

    private IEnumerator HandleBrokenDoorShort()
    {
        while (states == States.BrokenShort)
        {
            yield return new WaitForSeconds(brokenDoorPause);

            float waitTime = UnityEngine.Random.Range(0.2f, 0.4f);

            // Play opening start sound
            startAudioSource.clip = doorBrokenStart;
            startAudioSource.Play();

            // Wait for door to fully open
            yield return MoveDoor(closedPos, midPos, 0.4f, null);

            StartCoroutine(FadeOutAndStop(startAudioSource, 0.1f));

            // Pause before slamming shut
            //yield return new WaitForSeconds(brokenDoorPause);

            // Play slam SFX
            middleAudioSource.clip = doorBrokenSlamShort;
            middleAudioSource.Play();

            isClosing = true;
            // Wait for door to fully close
            yield return MoveDoor(midPos, closedPos, 0.2f, () => isClosing = false);


        }

        isShortBreakOver = true;
    }

    public IEnumerator HandleDoorStuck()
    {
        yield return new WaitUntil(() => isShortBreakOver == true);

        yield return new WaitForSeconds(1f);

        endAudioSource.clip = doorStuck;
        endAudioSource.Play();

        yield return MoveDoor(closedPos, shortPos, 1.4f, null);

        //yield return new WaitForSeconds(0.2f);

        StartCoroutine(HandleDoorJoltOpen());


    }

    public IEnumerator HandleDoorJoltOpen()
    {
        //StartCoroutine(FadeOutAndStop(middleAudioSource, 0.1f));



        startAudioSource.clip = doorBrokenJolt;
        startAudioSource.Play();

        aboutToJolt = true;

        yield return MoveDoor(doorPart.position, bodyPos, 0.2f, null);

        showingBody = true;


    }


    /// <summary>
    /// Set the state of a door
    /// </summary>
    /// <param name="state"></param>
    public void SetState(States state)
    {
        States previousState = this.states;
        this.DoorState = state;

        if (state == States.Closed || state == States.Open)
        {
            SetButtonColor(greenBase, greenEmis);
            decal.material = doorManager.UnlockedMaterial;
            UnlockHologram();

            if (state == States.Open)
            {
                //Debug.Log("This part of the script is happening");
                //open the door if it wasn't already opening
                if (previousState != States.Open && previousState != States.Opening)
                {
                    StartCoroutine(OpenDoor());
                }
            }
            if (state == States.Closed)
            {
                //close the door if it wasn't already closed
                if (previousState != States.Closed && previousState != States.Locked && previousState != States.Closing)
                {
                    StartCoroutine(CloseDoor());
                }
            }
        }
        else if (state == States.Broken)
        {
            SetButtonColor(yellowBase, yellowEmis);
            decal.material = doorManager.WarningMaterial;
            BrokenHologram();
            StartCoroutine(HandleBrokenDoorLoop());
        }
        else if (state == States.Locked)
        {
            SetButtonColor(redBase, redEmis);
            LockHologram();
            decal.material = doorManager.LockedMaterial;

            if (previousState != States.Locked && previousState != States.Closed)
            {
                StartCoroutine(CloseAndLock());
            }

        }
        else if (state == States.BrokenShort)
        {
            SetButtonColor(yellowBase, yellowEmis);
            decal.material = doorManager.WarningMaterial;
            BrokenHologram();
            StartCoroutine(HandleBrokenDoorShort());
        }
        else if (state == States.JoltOpen)
        {
            SetButtonColor(yellowBase, yellowEmis);
            decal.material = doorManager.WarningMaterial;
            BrokenHologram();
            StartCoroutine(HandleDoorStuck());
        }
    }

    private IEnumerator CloseAndLock()
    {
        StartCoroutine(CloseDoor());
        yield return new WaitUntil(() => states == States.Closed);
        DoorState = States.Locked;
    }

    private void DialogueEnd(int sequenceIndex)
    {
        // Only open door if the first dialogue sequence is completed
        if (sequenceIndex == 0)
        {
            //Debug.Log($"Dialogue {sequenceIndex} completed, door can be opened.");
            dialogueComplete = true;
        }
    }

    public void PuzzleComplete()
    {
        if (states == States.Locked)
        {
            DoorState = States.Opening;
        }
    }

    public void LockDoor()
    {
        DoorState = States.Locked;
    }

    public void UnlockDoor()
    {
        if (states == States.Locked)
        {
            DoorState = States.Closed;
            UseDoor();
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
        if (states != States.Locked && states != States.Broken)
        {
            if (inRange)
            {
                if (states != States.Open && states != States.Opening)
                {
                    UseDoor();
                    //Debug.Log("fade off");
                    StartFade(1.0f, lightOff, 0.5f);
                }

            }
            else
            {
                if (states != States.Closed && states != States.Closed)
                {
                    UseDoor();

                    // only reactive door hologram if still in range
                    if (doorManager.DoorInRange(this))
                    {
                        StartFade(0.0f, lightOn, 0.5f);
                    }

                }
            }
        }
        
    }

    private void SetButtonColor(Color baseColor, Color emisColor)
    {
        foreach (GameObject button in buttons)
        {
            if (button != null)
            {
                Material m = button.GetComponent<Renderer>().material;
                m.SetColor("_BaseColor", baseColor);
                m.SetColor("_EmissionColor", emisColor);
            }

        }
    }

    public IEnumerator FadeOutAndStop(AudioSource source, float duration)
    {
        if (source == null || !source.isPlaying)
            yield break;

        float startVolume = source.volume;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, 0f, time / duration);
            yield return null;
        }

        source.Stop();
        source.volume = startVolume; // Restore volume for future use
    }

    public void ToggleSparks(bool isActive)
    {
        showSparks = isActive;
        foreach (Sparks spark in sparks)
        {
            if (spark != null)
            {
                spark.ToggleSparks(isActive);
            }

        }
    }

    public void StartFade(float alphaValue, float lightIntensity, float fadeSpeed)
    {
        if (hologramGroup != null)
        {
            // stops coroutine if one is running already
            if (fadeRoutine != null)
            {
                StopCoroutine(fadeRoutine);
            }

            fadeRoutine = StartCoroutine(HologramFade(alphaValue, lightIntensity, fadeSpeed));

            // semi hard coded way to set hologram being turned on or off. There should never be a time where the alpha isnt 1 or 0.
            if (alphaValue == 1.0f) hologramActive = false;
            else if (alphaValue == 0.0f) hologramActive = true;
        }
        
    }

    //private IEnumerator HologramFade(float alphaValue, float lightIntensity, float fadeSpeed)
    //{


    //    foreach (MeshRenderer renderer in hologramGroup)
    //    {

    //        float time = 0.0f;

    //        float startVal = renderer.material.GetFloat("_Fade");

    //        Light light = renderer.transform.GetComponentInChildren<Light>();
    //        float startIntensity = light.intensity;

    //        while (time <= fadeSpeed)
    //        {
    //            // material lerp
    //            float lerp = Mathf.Lerp(startVal, alphaValue, Mathf.Clamp01(time / fadeSpeed));
    //            renderer.material.SetFloat("_Fade", lerp);


    //            // light lerp
    //            float lightLerp = Mathf.Lerp(startIntensity, lightIntensity, Mathf.Clamp01(time / fadeSpeed));
    //            light.intensity = lightLerp;

    //            time += Time.deltaTime;
    //            yield return null;
    //        }

    //        renderer.material.SetFloat("_Fade", alphaValue);

    //    }
    //}

    private IEnumerator HologramFade(float alphaValue, float lightIntensity, float fadeSpeed)
    {

        float time = 0.0f;

        // assume all holograms start from same values
        float startVal = hologramGroup[0].material.GetFloat("_Fade");
        float startIntensity = hologramGroup[0].transform.GetComponentInChildren<Light>().intensity;

        // cache lights so we don�t call GetComponent every frame
        Light[] lights = new Light[hologramGroup.Length];
        for (int i = 0; i < hologramGroup.Length; i++)
            lights[i] = hologramGroup[i].transform.GetComponentInChildren<Light>();


        while (time <= fadeSpeed)
        {

            // material lerp
            float shaderFade = Mathf.Lerp(startVal, alphaValue, Mathf.Clamp01(time / fadeSpeed));
            // light lerp
            float lightFade = Mathf.Lerp(startIntensity, lightIntensity, Mathf.Clamp01(time / fadeSpeed));


            foreach (MeshRenderer renderer in hologramGroup)
            {
                renderer.material.SetFloat("_Fade", shaderFade);
            }

            foreach (Light light in lights)
            {
                light.intensity = lightFade;
            }

            time += Time.deltaTime;
            yield return null;
        }

        // finalize values
        foreach (MeshRenderer renderer in hologramGroup)
            renderer.material.SetFloat("_Fade", alphaValue);

        foreach (Light light in lights)
            light.intensity = lightIntensity;

    }


    private void ApplyTextSign()
    {
        if (hologramGroup != null && hologramGroup.Length != 0) 
        {
            for (int i = 0; i < hologramGroup.Length; i++)
            {
                if (hologramGroup[i].material.GetTexture("_LabelText") != textLabels[i])
                {
                    hologramGroup[i].material.SetTexture("_LabelText", textLabels[i]);
                }
            }
        }
    }

    private void LockHologram()
    {
        foreach (MeshRenderer renderer in hologramGroup)
        {
            renderer.material.SetTexture("_StatusIcon", doorManager.lockedTexture);
            renderer.material.SetColor("_IconColor", doorManager.lockedColor);
        }
    }

    private void UnlockHologram()
    {
        foreach (MeshRenderer renderer in hologramGroup)
        {
            renderer.material.SetTexture("_StatusIcon", doorManager.unlockedTexture);
            renderer.material.SetColor("_IconColor", doorManager.unlockedColor);
        }
    }

    private void BrokenHologram()
    {
        foreach (MeshRenderer renderer in hologramGroup)
        {
            renderer.material.SetTexture("_StatusIcon", doorManager.warningTexture);
            renderer.material.SetColor("_IconColor", doorManager.warningColor);
        }
    }



}
