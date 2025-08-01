using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class StimDispenser : MonoBehaviour
{
    public bool canRefill = false;

    private GameObject playerObj;
    private ZeroGravity playerScript;
    private PlayerUIManager uiScript;
    private WristMonitor wristMonitor;

    private float refillTime = 2f;
    private float holdTimer = 0f;
    private bool isRefilling = false;
    private float maxRefillDistance = 1.5f;

    private Image progressBar;

    [SerializeField]
    private AudioSource audioSource;
    //Getters/Setters

    public bool CanRefill
    {
        get { return canRefill; }
        set { canRefill = value; }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerScript = FindFirstObjectByType<ZeroGravity>();
        playerObj = playerScript.gameObject;
        uiScript = playerObj.GetComponent<PlayerUIManager>();
        wristMonitor = FindFirstObjectByType<WristMonitor>();
        Debug.Log(playerObj);
        Debug.Log(playerScript);
        Debug.Log(uiScript);
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
            Debug.Log("I'm interacting");
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
        Debug.Log("Stim refill complete!");
        playerScript.AddStimsToInv(3);
        wristMonitor.UpdateStims(3);
    }
}
