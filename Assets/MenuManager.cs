using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    [SerializeField]
    private DeathMenu deathMenu;
    [SerializeField]
    private PauseMenu pauseMenu;

    [SerializeField]
    private Canvas canvas;

    [SerializeField]
    private ZeroGravity player;

    private void Update()
    {
        //if (!player.IsDead && pauseMenu.IsPaused)
        //{
        //    pauseMenu. = true;
        //    deathMenu.enabled = false;
        //}
        //else if(player.IsDead)
        //{
        //    pauseMenu.enabled = false;
        //    deathMenu.enabled = true;
        //}
    }


}
