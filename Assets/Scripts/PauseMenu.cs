
// Pause audiousing System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
public class PauseMenu : MonoBehaviour 
{

    public static bool IsPaused = false;
    [SerializeField]
    public GameObject pauseMenuUI;

    private PlayerController playerInput;
    private InputAction pauseAction;
    public AudioSource dialogueAudioSource; // Reference to the DialogueManager's AudioSource

    [SerializeField]
    private GameObject player;

    private bool canPause = false;

    public bool CanPause
    {
        get { return canPause; }
        set { canPause = value; }
    }


    private void Awake()
    {
        // Initialize the PlayerControls input actions
        playerInput = new PlayerController(); // Ensure this matches the generated class name
        canPause = true;
    }

    private void OnEnable()
    {
        // Access Pause action from the UI action map
        pauseAction = playerInput.UI.Pause;
        pauseAction.Enable();

        // Subscribe to the performed event
        pauseAction.performed += OnPausePressed;
    }

    private void OnDisable()
    {
        pauseAction.Disable();
        pauseAction.performed -= OnPausePressed;
    }

    public void OnPausePressed(InputAction.CallbackContext context)
    {
        if (canPause)
        {
            if (IsPaused)
            {
                Resume();
            }
            else
            {
                Pause();
            }
        }
    }

    public void Resume()
    {
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        IsPaused = false;
        

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        player.SetActive(true);

        // Resume audio
        if (dialogueAudioSource != null)
        {
            dialogueAudioSource.UnPause();
        }
    }

    public void LastCheckpoint()
    {
        Debug.Log("Load Last Checkpoint selected");
    }

    public void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        IsPaused = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        player.SetActive(false);

        if (dialogueAudioSource != null && dialogueAudioSource.isPlaying)
        {
            dialogueAudioSource.Pause();
        }
    }

    public void OptionsButton()
    {
        Debug.Log("Options selected");
    }

    public void LoadMenu()
    {
        Debug.Log("Menu selected");
        IsPaused = false;
        SceneManager.LoadScene("MainMenu");
    }

    public void Quit()
    {
        Debug.Log("Quit selected");
        Application.Quit();
    }

    private void Update()
    {
        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            Debug.Log("ESC Key Detected!");
        }
    }
}
