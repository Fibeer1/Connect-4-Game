using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class Pause : MonoBehaviour
{
    public bool paused = false;
    public static Pause instance;
    [SerializeField] private GameObject pauseMenu;
    [SerializeField] private GameObject tutorialScreen;
    [SerializeField] private TextMeshProUGUI interactionControls;
    [SerializeField] private Player player;

    private void Awake()
    {
        instance = this;
        if (player == null)
        {
            player = FindAnyObjectByType<Player>();
        }
        //Usually bad practice, but since I haven't implemented volume settings it's fine
        AudioListener.volume = 1;
        pauseMenu.SetActive(false);
        tutorialScreen.SetActive(false);
        interactionControls.gameObject.SetActive(false);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (paused)
            {
                Resume();
            }
            else if (!paused)
            {
                PauseGame();
            }
        }
    }

    private void OnInteractionControls(bool shouldActivate, string controlText = "")
    {
        interactionControls.text = controlText;
        interactionControls.gameObject.SetActive(shouldActivate);
    }

    public static void ToggleInteractionControls(bool shouldActivate, string controlsText = "")
    {
        instance.OnInteractionControls(shouldActivate, controlsText);
    }

    public void Resume()
    {
        player.enabled = true;
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
        AudioListener.pause = false;
        AudioListener.volume = 1;
        paused = false;
    }

    public void PauseGame()
    {
        player.enabled = false;
        pauseMenu.SetActive(true);
        Time.timeScale = 0f;
        AudioListener.pause = true;
        AudioListener.volume = 1;
        paused = true;
    }    

    public void ResetLevel()
    {
        pauseMenu.SetActive(false);
        Time.timeScale = 1f;
        AudioListener.pause = false;
        AudioListener.volume = 1;
        paused = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ReturnToMenu()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        AudioListener.volume = 1;
        SceneManager.LoadScene(0);
    }

    public void ExitGame()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        AudioListener.volume = 1;
        Application.Quit();
    }
}
