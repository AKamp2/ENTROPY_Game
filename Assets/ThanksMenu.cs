using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ThanksMenu : MonoBehaviour
{
    public static bool playerWin = false;
    [SerializeField]
    private GameObject canvas;


    public AudioSource dialogueAudioSource;
    [SerializeField]
    private PauseMenu pauseMenu;

    [SerializeField]
    private ZeroGravity player;

    [SerializeField]
    private GameObject playerCanvas;

    [SerializeField]
    private Win winScript;

    private void Awake()
    {
        playerWin = false;
        canvas.SetActive(false);
        playerCanvas.SetActive(true);
        pauseMenu.CanPause = true;
    }

    private void OnEnable()
    {
        pauseMenu.CanPause = true;
        // Access Pause action from the UI action map
    }

    private void OnDisable()
    {

        pauseMenu.CanPause = true;
    }


    public void LoadMenu()
    {
        Debug.Log("Menu selected");
        canvas.SetActive(false);
        playerWin = false;
        pauseMenu.CanPause = true;
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }

    public void Quit()
    {
        Debug.Log("Quit selected");
        Application.Quit();
    }

    public void Win()
    {
        if (canvas != null)
        {
            canvas.SetActive(true); // Show win menu
        }

        Time.timeScale = 0.75f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        //set all the player parameters as false to prevent moving, but looking around is fine
        player.CanMove = true;
        player.CanGrab = false;
        player.CanPropel = false;
        player.CanPushOff = false;
        player.CanRoll = false;

        pauseMenu.CanPause = false;

        playerCanvas.SetActive(false);

        

        //set the rigid body rotations to unconstrained, cool uncontrolled dead body rotations
        Rigidbody rb = player.GetComponentInParent<Rigidbody>();
        if (rb != null)
        {
            rb.constraints = RigidbodyConstraints.None;
        }
    }
    private void Update()
    {
        //the player is dead, therefore each true signifys death
        if (player != null && winScript.WinCondition && !playerWin)
        {
            Win();
            playerWin = true;
        }

    }
}


