using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    // TODO: have buttons to start new 1P or 2P
    public static bool isPaused = true;
    protected static bool gameOngoing = false;

    public GameObject pauseMenuUI;
    public GameObject resumeButton;
    public PieceManager pieceManager;

    void Start()
    {
        if (SaveSystem.LoadGame() == null)
            resumeButton.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && gameOngoing)
        {
            if (isPaused)
            {
                Resume();
            } else
            {
                Pause();
            }
        } else if (Input.GetKeyDown(KeyCode.F11))
            Screen.fullScreen = !Screen.fullScreen;
    }

    public void Resume()
    {
        resumeButton.SetActive(true);
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
        gameOngoing = true;
    }

    public void New2P()
    {
        Resume();
        pieceManager.ResetPieces();
        pieceManager.SwitchSides(Color.black);
    }

    void Pause()
    {
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }

    public void Quit()
    {
        print("Quitting game...");
        Application.Quit();
    }
}
