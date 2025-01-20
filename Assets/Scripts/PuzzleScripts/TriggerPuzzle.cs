using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.UI;

public class TriggerPuzzle : MonoBehaviour
{

    public Canvas puzzleCanvas;
    public float interactionDistance = 5f;
    public GameObject player;
    PlayerController playerController;
    public bool puzzleComplete = false;

    private bool showingPuzzle = false;

    public GameObject crosshair; // Reference to the crosshair image
    public GameObject playerUICanvas;
    public GameObject interactionText; // Reference to the TextMeshPro component for the interaction message
    private ZeroGravity playerManager;

    //audio
    public AudioSource audioSource;
    public AudioClip puzzleStart;

    private void Awake()
    {
        // Initialize the PlayerController and the Interact action
        playerController = new PlayerController();
        playerController.PlayerControls.Interact.performed += OnInteract; // Hook up Interact to your function
        
        
    }
    void Start()
    {
        puzzleCanvas.enabled = false;

        playerManager = player.GetComponent<ZeroGravity>();


        StartCoroutine(CheckDistance()); // Start the distance checking coroutine

    }

    private void OnEnable()
    {
        playerController.Enable();
    }

    private void OnDisable()
    {
        playerController.Disable();
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        // Only proceed if within interaction range 
        if(puzzleComplete == false)
        {
            if (Vector3.Distance(player.transform.position, transform.position) <= interactionDistance)
            {
                TogglePuzzle();
            }
        }
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private IEnumerator CheckDistance()
    {
        while (true) // Infinite loop
        {
            if(puzzleComplete == false) {
                // Check distance to show/hide interaction text
                if (Vector3.Distance(player.transform.position, transform.position) <= interactionDistance)
                {
                    interactionText.SetActive(true);
                }
                else
                {
                    interactionText.SetActive(false);
                }
            }
            

            yield return new WaitForSeconds(0.5f); // Wait for half a second before checking again
        }
    }

    public void TogglePuzzle()
    {
        showingPuzzle = !showingPuzzle;
        if(showingPuzzle)
        {
            audioSource.PlayOneShot(puzzleStart);
        }
        puzzleCanvas.enabled = showingPuzzle;
        playerManager.CanMove = !showingPuzzle;
        Cursor.lockState = showingPuzzle ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = showingPuzzle;
        //crosshair.SetActive(!showingPuzzle); // Hide crosshair when puzzle is active
        playerUICanvas.SetActive(!showingPuzzle);
        player.GetComponent<Rigidbody>().isKinematic = showingPuzzle;
    }
}
