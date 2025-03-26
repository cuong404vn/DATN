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
       
        OnCoinsChanged?.Invoke(coins);
        OnKeysChanged?.Invoke(keys);
        Debug.Log("Scene loaded. GameManager coins: " + coins);
    }

    void Start()
    {
        currentSceneName = SceneManager.GetActiveScene().name;
        Debug.Log("GameManager started. Instance: " + (Instance == this ? "Valid" : "Invalid"));
    }

    public void AddCoins(int amount)
    {
        coins += amount;
        OnCoinsChanged?.Invoke(coins);
        Debug.Log("Coins added: " + amount + ", Total: " + coins);
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
}
