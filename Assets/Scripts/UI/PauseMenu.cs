using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Adapted from https://youtu.be/JivuXdrIHK0 (Brackeys)
public class PauseMenu : MonoBehaviour
{
    // TODO: have buttons to start new 1P or 2P
    public static bool isPaused = true;
    protected static bool gameOngoing = false;

    public GameObject pauseMenuUI;
    public GameObject resumeButton;
    public GameObject pauseButton;
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
        pauseButton.SetActive(true);
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
        gameOngoing = true;
    }

    private void NewGame(bool isTwoPlayer) {
        Resume();
        pieceManager.ResetPieces();
        pieceManager.isTwoPlayer = isTwoPlayer;
        pieceManager.SwitchSides(Color.black);
    }

    public void New2P() => NewGame(true);

    public void New1P() => NewGame(false);

    public void Pause()
    {
        pauseButton.SetActive(false);
        pauseMenuUI.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
        Debug.Log("Pause!");
    }

    public void Quit()
    {
        print("Quitting game...");
        Screen.fullScreen = false;
        Application.Quit();
    }
}
