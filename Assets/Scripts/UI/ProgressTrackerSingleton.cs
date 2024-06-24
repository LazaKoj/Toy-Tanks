using UnityEngine;

public class ProgressTrackerSingleton : MonoBehaviour
{
    public static ProgressTrackerSingleton Instance;

    // Array stores boolean values that keep track of which levels have been completed.
    public bool[] LevelsCompleted;

    // Implementing the Singleton method.
    void Awake()
    {
        if (Instance == null)
        {
            DontDestroyOnLoad(gameObject);
            Instance = this;
        }
        else if (Instance != this)
            Destroy(gameObject);

        LevelsCompleted = new bool[3] { false, false, false };
    }
}
