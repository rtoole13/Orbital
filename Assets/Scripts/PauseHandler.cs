using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TimeController))]
public class PauseHandler : MonoBehaviour
{
    private TimeController timeController;

    [SerializeField]
    private GameObject pauseUI;

    [SerializeField]
    private GameObject pauseTooltip;

    private bool pauseGame = false;

    #region UNITY
    private void Awake()
    {
        timeController = GetComponent<TimeController>();
        //TogglePause();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            TogglePause();
        }
    }
    #endregion UNITY

    void TogglePause()
    {
        pauseGame = !pauseGame;
        if (pauseGame)
        {
            timeController.Pause();
        }
        else
        {
            timeController.UnPause();
        }
        pauseUI.SetActive(pauseGame);
        pauseTooltip.SetActive(!pauseGame);
    }
}
