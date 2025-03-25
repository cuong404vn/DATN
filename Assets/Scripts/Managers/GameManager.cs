using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
using System.Collections;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public int coins = 0;
    public int keys = 0;

    public TextMeshProUGUI coinsText;
    public TextMeshProUGUI keysText;

    public string mainMenuScene = "MainMenu";
    private string currentSceneName;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    void Start()
    {
        UpdateUI();
        currentSceneName = SceneManager.GetActiveScene().name;
    }

    public void AddCoins(int amount)
    {
        coins += amount;
        UpdateUI();
    }

    public void AddKeys(int amount)
    {
        keys += amount;
        UpdateUI();
    }

    void UpdateUI()
    {
        if (coinsText != null)
            coinsText.text = coins.ToString();

        if (keysText != null)
            keysText.text = keys.ToString();
    }

    public void RetryLevel()
    {
        Time.timeScale = 1f; 
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        StartCoroutine(ResetGameState()); 
    }

    private IEnumerator ResetGameState()
    {
        yield return new WaitForSeconds(0.1f); 

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            
            if (!player.activeInHierarchy)
            {
                player.SetActive(true);
            }

            
            Animator animator = player.GetComponent<Animator>();
            if (animator != null)
            {
                animator.Rebind();
                animator.Update(0f);
            }

            
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
                rb.bodyType = RigidbodyType2D.Dynamic;
            }

            
            PlayerController playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.enabled = true;
            }
        }

        
        CameraFollow cameraFollow = Camera.main.GetComponent<CameraFollow>();
        if (cameraFollow != null)
        {
            cameraFollow.FindPlayer();
        }
    }


    public void GoToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(mainMenuScene);
    }

    private void ResetPlayerState()
    {
        GameObject player = GameObject.FindWithTag("Player");
        if (player != null)
        {
            
            Animator animator = player.GetComponent<Animator>();
            if (animator != null)
            {
                animator.Rebind();
                animator.Update(0f);
            }

           
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
                rb.bodyType = RigidbodyType2D.Dynamic; 
            }

            
            PlayerController playerController = player.GetComponent<PlayerController>();
            if (playerController != null)
            {
                playerController.enabled = true; 
            }

            
            CameraFollow cameraFollow = Camera.main.GetComponent<CameraFollow>();
            if (cameraFollow != null)
            {
                cameraFollow.FindPlayer();
            }
        }
    }
}
