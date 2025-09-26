using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.Rendering.Universal;

public class StimDispenser : MonoBehaviour
{
    //var for detecting if the player is close enough to the dispenser to refill.
    [SerializeField]
    private bool canRefill = false;
    //var for detecting if the stim dispenser is available to be used - can make temporarily out of order.
    [SerializeField]
    private bool isUsable = false;

    [SerializeField]
    private Light spotlight;

    [SerializeField]
    private DecalProjector decal;

    [SerializeField]
    private Material glowingDecal;
    [SerializeField]
    private Material unlitDecal;

    private GameObject playerObj;
    private ZeroGravity playerScript;
    private PlayerUIManager uiScript;
    private WristMonitor wristMonitor;

    private float refillTime = 2f;
    private float holdTimer = 0f;
    private bool isRefilling = false;
    private float maxRefillDistance = 1.5f;
    private float lightIntensity = 0f;

    private Image progressBar;

    [SerializeField]
    private AudioSource audioSource;
    //Getters/Setters

    public bool CanRefill
    {
        get { return canRefill; }
        set { canRefill = value; }
    }

    public bool IsUsable
    {
        get { return isUsable; }
        set { isUsable = value; }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        lightIntensity = spotlight.intensity;
        if(isUsable)
        {
            decal.material = glowingDecal;
            spotlight.intensity = lightIntensity;
        }
        else
        {
            decal.material = unlitDecal;
            spotlight.intensity = 0f;
        }

        playerScript = FindFirstObjectByType<ZeroGravity>();
        playerObj = playerScript.gameObject;
        uiScript = playerObj.GetComponent<PlayerUIManager>();
        wristMonitor = FindFirstObjectByType<WristMonitor>();
        //Debug.Log(playerObj);
        //Debug.Log(playerScript);
        //Debug.Log(uiScript);
        progressBar = uiScript.ProgressBar;
    }

    // Update is called once per frame
    void Update()
    {
        if (isRefilling)
        {
            float progress = holdTimer / refillTime;
            progressBar.fillAmount = Mathf.Clamp01(progress);
            // Cancel refill if player is too far
            float distance = Vector3.Distance(playerObj.transform.position, transform.position);
            if (distance > maxRefillDistance)
            {
                //Debug.Log("Player moved too far. Cancelling refill.");
                StopRefill();
                return;
            }

            // Continue refill if still in range
            holdTimer += Time.deltaTime;
            if (holdTimer >= refillTime)
            {
                CompleteRefill();
                ResetRefill();
            }
        }
    }

    public void OnInteract(InputAction.CallbackContext context)
    {
        if (context.started && canRefill)
        {
            //Debug.Log("I'm interacting");
            StartRefill();
        }
        if (context.canceled)
        {
            StopRefill();
        }
    }

    public void StartRefill()
    {
        if (canRefill)
        {
            isRefilling = true;
            holdTimer = 0f;
            progressBar.enabled = true;
        }
    }

    public void StopRefill()
    {
        ResetRefill();
        canRefill = false; // force look again next time
        
    }

    private void ResetRefill()
    {
        isRefilling = false;
        holdTimer = 0f;
        progressBar.fillAmount = 0f;
        progressBar.enabled = false;
    }

    private void CompleteRefill()
    {
        // Replace this with logic to update wrist monitor and stim counts
        //Debug.Log("Stim refill complete!");
        audioSource.Play();
        playerScript.AddStimsToInv(3);
    }

    public void ToggleUsability(bool isUsable)
    {
        this.isUsable = isUsable;

        if(isUsable)
        {
            decal.material = glowingDecal;
            spotlight.intensity = lightIntensity;
        }
        else
        {
            decal.material = unlitDecal;
            spotlight.intensity = 0f;
        }
    }
}
