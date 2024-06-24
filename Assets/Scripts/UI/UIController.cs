using UnityEngine;
using UnityEngine.SceneManagement;

public class UIController : MonoBehaviour
{
    public static bool paused;

    public GameObject panelMenu;
    public GameObject imagePause;

    void Awake()
    {
        imagePause.SetActive(false);
    }

    void Update()
    {
        // Pause the game if the escape key is pressed.
        if (!GameState.gameOver && Input.GetKeyDown("escape"))
            Pause();
    }

    // Load the given level.
    public void LoadLevel(int level)
    {        
        SceneManager.LoadScene("Level" + level);
        Resume();
    }

    // Load the main menu.
    public void LoadMenu()
    {
        SceneManager.LoadScene("MainMenu");
        Resume();
    }

    // Load the credits scene.
    public void LoadCredits()
    {
        SceneManager.LoadScene("Credits");
    }

    // Load the current scene again.
    public void Retry()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Pause the game by setting the timeScale to 0 and then opening the pause menu.
    public void Pause()
    {
        paused = true;
        panelMenu.SetActive(true);
        imagePause.SetActive(true);
        Time.timeScale = 0;
        AudioListener.pause = true;
    }

    // Resume a paused game by setting timeScale back to 1 and then closing the paused menu.
    public void Resume()
    {
        Time.timeScale = 1;
        imagePause.SetActive(false);
        panelMenu.SetActive(false);
        paused = false;
        AudioListener.pause = false;
    }
}
