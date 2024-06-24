using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

public class GameState : MonoBehaviour
{
    private int timer, ticks;
    public static bool gameOver;
    public Text healthText;
    public Text timerText;
    public GameObject winPanel;
    public GameObject losePanel;

    void Awake()
    {
        winPanel.SetActive(false);
        losePanel.SetActive(false);
    }

    void Start()
    {
        gameOver = false;
        timer = 300;
        ticks = 0;
    }

    void LateUpdate()
    {

        if (!UIController.paused && !gameOver)
        {
            GameObject playerTank = GameObject.Find("Player_Tank");
            if (playerTank == null)
            {
                healthText.text = "Health: 0";
                losePanel.SetActive(true);
                gameOver = true;
            }
            else
            {
                ticks++;
                if (ticks % 60 == 0)
                    timer--;
                int playerHealth = playerTank.GetComponent<PlayerTank>().health;
                healthText.text = "Health: " + playerHealth;
                timerText.text = "Time: " + timer;

                if (timer == 0)
                {
                    losePanel.SetActive(true);
                    gameOver = true;
                }

                bool enemiesRemaining = false;
                string[] enemyTypes = { "Enemy_Tank", "Enemy_Stationary_Turret", "Enemy_Tank (Clone)", "Enemy_Stationary_Turret (Clone)" };
                for (int i = 0; i < enemyTypes.Length; i++)
                {
                    GameObject eObj = GameObject.Find(enemyTypes[i]);
                    if (eObj != null)
                        enemiesRemaining = true;
                }
                if (!enemiesRemaining)
                {
                    int levelCompleted = int.Parse(SceneManager.GetActiveScene().name.Substring(5)) - 1;

                    ProgressTrackerSingleton progressTracker = GameObject.Find("Progress_Tracker_Singleton").GetComponent<ProgressTrackerSingleton>();
                    progressTracker.LevelsCompleted[levelCompleted] = true;
                    SaveGame(levelCompleted);
                    winPanel.SetActive(true);
                    gameOver = true;
                }
            }
        }
    }

    public void SaveGame(int levelCompleted)
    {
        Save save = new Save();
        FileStream savefile = File.Create(Application.persistentDataPath + "/gamesave.save");
        BinaryFormatter bf = new BinaryFormatter();

        levelCompleted += 1;

        save.levelsUnlocked = levelCompleted;

        bf.Serialize(savefile, save);

        savefile.Close();
    }

    public static void LoadGame()
    {
        if (File.Exists(Application.persistentDataPath + "/gamesave.save"))
        {
            BinaryFormatter bf = new BinaryFormatter();
            ProgressTrackerSingleton pts = GameObject.Find("Progress_Tracker_Singleton").GetComponent<ProgressTrackerSingleton>();
            FileStream savefile = File.Open(Application.persistentDataPath + "/gamesave.save", FileMode.Open);
            Save save;

            save = (Save)bf.Deserialize(savefile);

            for (int i = 0; i < save.levelsUnlocked; ++i)
                pts.LevelsCompleted[i] = true;
        } 
        else
            Debug.Log("gamesave.save does not exist");
    }
}
