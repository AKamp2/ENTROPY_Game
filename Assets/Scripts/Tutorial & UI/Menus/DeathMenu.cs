using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class DeathMenu : MonoBehaviour
{
    

    //private void Awake()
    //{
    //    playerDead = false;
    //    canvas.SetActive(false);
    //    playerCanvas.SetActive(true);
    //    pauseMenu.CanPause = true;
    //}

    //private void OnEnable()
    //{
    //    pauseMenu.CanPause = true;
    //    // Access Pause action from the UI action map
    //}

    //public void Die()
    //{
    //    if (canvas != null)
    //    {
    //        canvas.SetActive(true); // Show death menu
    //    }

    //    Time.timeScale = 0.75f;

    //    Cursor.lockState = CursorLockMode.None;
    //    Cursor.visible = true;

    //    //set all the player parameters as false to prevent moving, but looking around is fine
    //    player.CanMove = true;
    //    player.CanGrab = false;
    //    player.CanPropel = false;
    //    player.CanPushOff = false;
    //    player.CanRoll = false;

    //    pauseMenu.CanPause = false;

    //    playerCanvas.SetActive(false);

    //    //set the rigid body rotations to unconstrained, cool uncontrolled dead body rotations
    //    Rigidbody rb = player.GetComponentInParent<Rigidbody>();
    //    if(rb != null )
    //    {
    //        rb.constraints = RigidbodyConstraints.None;
    //    }
    //}
    //private void Update()
    //{
    //    //the player is dead, therefore each true signifys death
    //    if (player != null && player.IsDead && !playerDead)
    //    {
    //        Die();
    //        playerDead = true;
    //    }

    //}
}

