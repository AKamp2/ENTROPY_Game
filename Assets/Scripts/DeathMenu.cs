using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class DeathMenu : MonoBehaviour
{
    public static bool playerDead = false;
    public GameObject playerMenuUI;

    [SerializeField]
    private PlayerController playerInput;
    [SerializeField]
    private InputAction playerAction;

    [SerializeField]
    private ZeroGravity player = new ZeroGravity();

    private void Awake()
    {
        // Initialize the PlayerControls input actions
        playerInput = new PlayerController(); // Ensure this matches the generated class name
    }

    private void OnEnable()
    {
        playerInput.UI.Enable();
        // Access Pause action from the UI action map
        playerAction = playerInput.UI.Pause;
        playerAction.Enable();
        playerInput.UI.Enable();
    }

    private void OnDisable()
    {
        playerInput.UI.Disable();
        playerAction.Disable();
    }

    private void Start()
    {
        //setting to wait for the player isDead to match it
        playerDead = true;
    }

    private void Update()
    {
        //the player is dead, therefore each true signifys death
        if (playerDead == player.IsDead)
        {
            Die();
        }
    }

    public void LastCheckpoint()
    {
        Debug.Log("Load Last Checkpoint selected");
    }

    public void OptionsButton()
    {
        Debug.Log("Options selected");
    }

    public void LoadMenu()
    {
        Debug.Log("Menu selected");
        playerMenuUI.SetActive(false);
        playerDead = false;
        SceneManager.LoadScene("MainMenu");
    }

    public void Quit()
    {
        Debug.Log("Quit selected");
        Application.Quit();
    }

    public void Die()
    {
        playerMenuUI.SetActive(true);
        Time.timeScale = 0.5f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        //set all the player parameters as false to prevent moving, but looking around is fine
        player.CanMove = true;
        player.CanGrab = false;
        player.CanPropel = false;
        player.CanPushOff = false;
        player.CanRoll = false;

        //set the rigid body rotations to unconstrained, cool uncontrolled dead body rotations
        player.GetComponentInParent<Rigidbody>().constraints = RigidbodyConstraints.None;
    }
}

