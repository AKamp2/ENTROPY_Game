using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ThanksMenu : MonoBehaviour
{
    public static bool playerWin = false;

    [SerializeField]
    private Win winScript;

    private void Awake()
    {
        playerWin = false;
    }

    public void Win()
    {
        
    }
    private void Update()
    {
        if (winScript.WinCondition && playerWin)
        {
            Win();
            playerWin = true;
        }

    }
}


