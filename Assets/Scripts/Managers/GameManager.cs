using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-100)]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int coins = 0;
    public int keys = 0;

    public string mainMenuScene = "MainMenu";
    private string currentSceneName;

    // Danh sách các scene không phải là màn chơi (để không reset coins và keys)
    private readonly string[] nonGameplayScenes = { "Login", "Register", "Home", "MainMenu" };


    public delegate void CurrencyChanged(int value);
    public event CurrencyChanged OnCoinsChanged;
    public event CurrencyChanged OnKeysChanged;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            SceneManager.sceneLoaded += OnSceneLoaded;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string sceneName = scene.name;

        // Kiểm tra nếu scene mới là màn chơi thì reset stats
        bool isGameplayScene = true;
        foreach (string nonGameplayScene in nonGameplayScenes)
        {
            if (sceneName == nonGameplayScene)
            {
                isGameplayScene = false;
                break;
            }
        }

        if (isGameplayScene)
        {
            ResetLevelStats();
        }

        OnCoinsChanged?.Invoke(coins);
        OnKeysChanged?.Invoke(keys);
    }

    void Start()
    {
        currentSceneName = SceneManager.GetActiveScene().name;
    }

    public void AddCoins(int amount)
    {
        coins += amount;
        OnCoinsChanged?.Invoke(coins);
    }

    public void AddKeys(int amount)
    {
        keys += amount;
        OnKeysChanged?.Invoke(keys);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuScene);
    }

    public void ResetLevelStats()
    {
        Debug.Log("Resetting coins and keys to 0");
        coins = 0;
        keys = 0;

        OnCoinsChanged?.Invoke(coins);
        OnKeysChanged?.Invoke(keys);
    }

    public void NotifyEnemyDefeated()
    {
        LevelManager levelManager = FindAnyObjectByType<LevelManager>();
        if (levelManager != null)
        {
            levelManager.EnemyDefeated();
        }
    }

    public int GetCoinCount()
    {
        return coins;
    }
}
