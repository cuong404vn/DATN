using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class PlayerHealth : MonoBehaviour
{
    public int maxHealth = 5;
    public int currentHealth;
    public float invincibilityTime = 1f;
    private bool isInvincible = false;

    public Image[] healthImages; 
    public Sprite fullHeart;
    public Sprite emptyHeart;

    public GameObject gameOverPanel;
    public Button retryButton;
    public Button mainMenuButton;

    void Start()
    {
        SetupInitialState();
    }

    
    private void SetupInitialState()
    {
        currentHealth = maxHealth;

       
        if (gameOverPanel == null)
            gameOverPanel = GameObject.Find("GameOverPanel");

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(false);

            if (retryButton == null)
                retryButton = gameOverPanel.transform.Find("RetryButton")?.GetComponent<Button>();

            if (mainMenuButton == null)
                mainMenuButton = gameOverPanel.transform.Find("MainMenuButton")?.GetComponent<Button>();

         
            if (retryButton != null)
            {
                retryButton.onClick.RemoveAllListeners(); 
                retryButton.onClick.AddListener(RestartLevel);
            }

            if (mainMenuButton != null)
            {
                mainMenuButton.onClick.RemoveAllListeners(); 
                mainMenuButton.onClick.AddListener(() => GameManager.Instance.GoToMainMenu());
            }
        }

      
        if (healthImages == null || healthImages.Length == 0)
        {
           
            Transform healthUI = GameObject.Find("HealthUI")?.transform;
            if (healthUI != null)
            {
                healthImages = healthUI.GetComponentsInChildren<Image>();
            }
        }

        UpdateHealthUI();
    }

    public void TakeDamage(int damage)
    {
        if (isInvincible) return;

        currentHealth -= damage;

        if (currentHealth <= 0)
        {
            Die();
        }
        else
        {
            StartCoroutine(BecomeInvincible());
        }

        UpdateHealthUI();
    }

    public void AddHealth(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        UpdateHealthUI();
    }

    void UpdateHealthUI()
    {
        if (healthImages == null || healthImages.Length == 0) return;

        for (int i = 0; i < healthImages.Length; i++)
        {
            if (healthImages[i] != null)
            {
                healthImages[i].sprite = (i < currentHealth) ? fullHeart : emptyHeart;
            }
        }
    }

    void Die()
    {
      
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);

           
            if (retryButton != null && retryButton.onClick.GetPersistentEventCount() == 0)
                retryButton.onClick.AddListener(RestartLevel);

            if (mainMenuButton != null && mainMenuButton.onClick.GetPersistentEventCount() == 0)
                mainMenuButton.onClick.AddListener(() => GameManager.Instance.GoToMainMenu());
        }

    
        GetComponent<PlayerController>().enabled = false;

      
    }

    IEnumerator BecomeInvincible()
    {
        isInvincible = true;

       
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        for (float i = 0; i < invincibilityTime; i += 0.1f)
        {
            sprite.enabled = !sprite.enabled;
            yield return new WaitForSeconds(0.1f);
        }
        sprite.enabled = true;

        isInvincible = false;
    }

    private void RestartLevel()
    {
        Time.timeScale = 1f;

        if (gameOverPanel != null)
            gameOverPanel.SetActive(false);

        GameManager.Instance.ResetLevelStats();

      
        GetComponent<PlayerController>().enabled = false;
        GetComponent<Rigidbody2D>().simulated = false;

     
        StartCoroutine(LoadLevelWithReset());
    }

    private IEnumerator LoadLevelWithReset()
    {
       
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);

     
        yield return null;

      
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            
            PlayerController controller = player.GetComponent<PlayerController>();
            Rigidbody2D rb = player.GetComponent<Rigidbody2D>();
            Animator animator = player.GetComponent<Animator>();

            if (controller != null)
            {
                controller.enabled = true;
                controller.ResetState(); 
            }

            if (rb != null)
            {
                rb.linearVelocity = Vector2.zero;
                rb.angularVelocity = 0f;
                rb.simulated = true;
            }

            if (animator != null)
            {
                animator.Rebind();
                animator.Update(0f);
            }

          
            Transform spawnPoint = GameObject.FindGameObjectWithTag("SpawnPoint")?.transform;
            if (spawnPoint != null)
            {
                player.transform.position = spawnPoint.position;
            }
        }
    }

    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Trap"))
        {
           
            TakeDamage(1);
        }
        else if (other.CompareTag("DungNham"))
        {
           
            currentHealth = 0;
            Die();
        }
    }

    public void TakeDamageFromBoss(int damage)
    {
        TakeDamage(damage);
    }
}