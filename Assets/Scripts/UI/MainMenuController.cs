using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;

public class MainMenuController : MonoBehaviour
{
    public GameObject titleScreen;
    public GameObject instructions;
    public GameObject levelSelect;

    public GameObject level2Button;
    public GameObject level3Button;

    public ProgressTrackerSingleton progressTracker;

    void Awake()
    {
        ActivePanelTitleScreen();
        progressTracker = GameObject.Find("Progress_Tracker_Singleton").GetComponent<ProgressTrackerSingleton>();
        GameState.LoadGame();
    }

    void Start()
    {
        // Grey out the level 2 + 3 buttons if they cannot be played yet.
        if (progressTracker.LevelsCompleted[0] == false)
            level2Button.GetComponent<Image>().color = Color.gray;
        if (progressTracker.LevelsCompleted[1] == false)
            level3Button.GetComponent<Image>().color = Color.gray;
    }

    // Activate the corresponding panel for main menu.
    public void ActivePanelTitleScreen()
    {
        instructions.SetActive(false);
        levelSelect.SetActive(false);
        titleScreen.SetActive(true);
    }
    public void ActivePanelInstructions()
    {
        instructions.SetActive(true);
    }
    public void ActivePanelLevelSelect()
    {
        instructions.SetActive(false);
        titleScreen.SetActive(false);
        levelSelect.SetActive(true);
    }

    // Load the level given only if the previous level has been completed.
    public void LoadLevel(int level)
    {
        if(level == 1)
            SceneManager.LoadScene("Level1");
        else if(progressTracker.LevelsCompleted[level-2])
            SceneManager.LoadScene("Level"+level.ToString());
        
    }

    private void Update()
    {
        if (Input.GetKey(KeyCode.LeftAlt))
            if (Input.GetKeyDown(KeyCode.D))
                DeleteGame(); //deletes save data
    }

    // Load the credits scene.
    public void LoadCredits()
    {
        SceneManager.LoadScene("Credits");
    }

    // Quit the game in both the unity editor and during the actual game.
    public void QuitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }

    public void DeleteGame()
    {
        if (File.Exists(Application.persistentDataPath + "/gamesave.save"))
            File.Delete(Application.persistentDataPath + "/gamesave.save");
    }
}
