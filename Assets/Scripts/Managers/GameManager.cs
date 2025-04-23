using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(-100)]
public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int coins = 0;
    public int keys = 0;


    [HideInInspector] public int savedPlayerHealth = 5;
    [HideInInspector] public int savedPlayerPotions = 0;
    [HideInInspector] public bool isTransitioningScene = false;

    public string mainMenuScene = "MainMenu";
    private string currentSceneName;


    private readonly string[] nonGameplayScenes = { "Login", "Register", "Home", "MainMenu" };


    public delegate void CurrencyChanged(int value);
    public event CurrencyChanged OnCoinsChanged;
    public event CurrencyChanged OnKeysChanged;

    void Awake()
    {

        bool isFirstInstance = Instance == null;

        if (isFirstInstance)
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
        if (Instance == this)
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string sceneName = scene.name;
        bool isTransitioning = PlayerPrefs.GetInt("IsTransitioningScene", 0) == 1;
        bool isRestarting = PlayerPrefs.GetInt("IsRestarting", 0) == 1;




        if (isRestarting)
        {

            PlayerPrefs.DeleteKey("IsRestarting");
            PlayerPrefs.Save();

            return; // Không xử lý tiếp, để PlayerHealth quản lý logic restart
        }


        bool isGameplayScene = true;
        foreach (string nonGameplayScene in nonGameplayScenes)
        {
            if (sceneName == nonGameplayScene)
            {
                isGameplayScene = false;
                break;
            }
        }

        if (isGameplayScene && !isTransitioning && !isRestarting)
        {

            ResetLevelStats();
        }
        else if (isTransitioning)
        {

            coins = PlayerPrefs.GetInt("PlayerCoins", 0);
            keys = PlayerPrefs.GetInt("PlayerKeys", 0);

            int savedHealth = PlayerPrefs.GetInt("PlayerCurrentHealth", 5);

        


            OnCoinsChanged?.Invoke(coins);
            OnKeysChanged?.Invoke(keys);
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

    public void RemoveCoins(int amount)
    {
        coins -= amount;
        if (coins < 0)
            coins = 0;

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
