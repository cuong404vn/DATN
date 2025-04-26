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

    public int maxPotions = 10;
    public int currentPotions = 0;

    public Image potionIcon;
    public Text potionCountText;
    public GameObject potionUseEffect;
    public AudioClip potionUseSound;


    [Header("Death Effect")]
    public AudioClip deathSound;
    public GameObject deathEffectPrefab;
    public float delayBeforeGameOver = 1.5f;

    private Animator animator;
    private bool isDead = false;
    private bool isShopOpen = false;
    private bool isFromPortal = false;

    void Start()
    {

        bool isTransitioning = PlayerPrefs.GetInt("IsTransitioningScene", 0) == 1;

        if (isTransitioning)
        {

            int savedHealth = PlayerPrefs.GetInt("PlayerCurrentHealth", maxHealth);
            int savedPotions = PlayerPrefs.GetInt("PlayerCurrentPotions", 0);


            isFromPortal = true;
            currentHealth = savedHealth;
            currentPotions = savedPotions;


            PlayerPrefs.SetInt("IsTransitioningScene", 0);
            PlayerPrefs.Save();
        }
        else
        {

            currentHealth = maxHealth;
        }

        SetupInitialState();
        animator = GetComponent<Animator>();
    }


    private void SetupInitialState()
    {
        if (!isFromPortal)
        {

            currentHealth = maxHealth;
        }
        else
        {

        }


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
        UpdatePotionUI();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1) && !isShopOpen)
        {
            UseHealthPotion();
        }
    }

    public void TakeDamage(int damage)
    {

        if (isInvincible || isDead) return;

        string damageSource = "Unknown";

        System.Diagnostics.StackTrace stackTrace = new System.Diagnostics.StackTrace(true);
        if (stackTrace.FrameCount > 1)
        {
            System.Diagnostics.StackFrame callerFrame = stackTrace.GetFrame(1);
            damageSource = callerFrame.GetMethod().Name;

            string callerClassName = callerFrame.GetMethod().DeclaringType.Name;
            damageSource = $"{callerClassName}.{damageSource}";
        }

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

        if (isDead) return;
        isDead = true;


        StopAllCoroutines();


        if (deathSound != null)
        {
            AudioSource.PlayClipAtPoint(deathSound, transform.position);
        }


        if (deathEffectPrefab != null)
        {
            Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
        }


        if (animator != null)
        {

            animator.SetTrigger("Death");


        }


        GetComponent<PlayerController>().enabled = false;
        Rigidbody2D rb = GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = 0;
        }


        StartCoroutine(ShowGameOverAfterDelay());
    }

    private IEnumerator ShowGameOverAfterDelay()
    {
        yield return new WaitForSeconds(delayBeforeGameOver);


        Time.timeScale = 0f;

        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);


            if (retryButton != null && retryButton.onClick.GetPersistentEventCount() == 0)
                retryButton.onClick.AddListener(RestartLevel);

            if (mainMenuButton != null && mainMenuButton.onClick.GetPersistentEventCount() == 0)
                mainMenuButton.onClick.AddListener(() => GameManager.Instance.GoToMainMenu());
        }



        gameObject.SetActive(true);


        EnemyQuaiBay[] allFlyingEnemies = FindObjectsByType<EnemyQuaiBay>(FindObjectsSortMode.None);
        if (allFlyingEnemies != null && allFlyingEnemies.Length > 0)
        {
            foreach (EnemyQuaiBay enemy in allFlyingEnemies)
            {
                if (enemy != null)
                {
                    enemy.SendMessage("ResetPlayerReference", SendMessageOptions.DontRequireReceiver);
                }
            }
        }
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


        GetComponent<PlayerController>().enabled = false;
        GetComponent<Rigidbody2D>().simulated = false;


        string currentMapID = SceneManager.GetActiveScene().name;
        string originalMapID = PlayerPrefs.GetString("OriginalMapID", "");




        if (string.IsNullOrEmpty(originalMapID))
        {
            originalMapID = currentMapID;

        }


        GameManager.Instance.ResetLevelStats();


        PlayerPrefs.DeleteKey("IsTransitioningScene");
        PlayerPrefs.DeleteKey("PlayerCurrentHealth");
        PlayerPrefs.DeleteKey("PlayerCurrentPotions");
        PlayerPrefs.DeleteKey("GameTime");
        PlayerPrefs.DeleteKey("TotalScore");
        PlayerPrefs.DeleteKey("EnemiesDefeated");
        PlayerPrefs.DeleteKey("PlayerCoins");
        PlayerPrefs.DeleteKey("PlayerKeys");


        PlayerPrefs.SetInt("IsRestarting", 1);
        PlayerPrefs.Save();


        SceneManager.LoadScene(originalMapID);
    }

    private IEnumerator LoadLevelWithReset()
    {

        if (PlayerPrefs.GetInt("IsRestarting", 0) == 1)
        {

            string firstMap = PlayerPrefs.GetString("FirstMap", "");
            if (!string.IsNullOrEmpty(firstMap))
            {

                PlayerPrefs.DeleteKey("FirstMap");
                PlayerPrefs.DeleteKey("IsRestarting");
                PlayerPrefs.Save();
                SceneManager.LoadScene(firstMap);
                yield break;
            }

            PlayerPrefs.DeleteKey("IsRestarting");
            PlayerPrefs.Save();
        }


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

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy") ||
            collision.gameObject.layer == LayerMask.NameToLayer("Enemy") ||
            collision.gameObject.name.Contains("Enemy"))
        {
            TakeDamage(1);
        }
        else if (collision.gameObject.CompareTag("Trap"))
        {
            TakeDamage(1);
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

    public void UseHealthPotion()
    {
        if (currentPotions > 0 && currentHealth < maxHealth)
        {
            currentPotions--;
            AddHealth(1);

            if (potionUseEffect != null)
            {
                StartCoroutine(ShowPotionUseEffect());
            }

            if (potionUseSound != null)
            {
                AudioSource.PlayClipAtPoint(potionUseSound, transform.position, 0.7f);
            }

            UpdatePotionUI();
        }
    }

    public bool AddHealthPotion(int amount)
    {
        if (currentPotions < maxPotions)
        {
            int potionsToAdd = Mathf.Min(amount, maxPotions - currentPotions);
            currentPotions += potionsToAdd;
            UpdatePotionUI();
            return true;
        }
        return false;
    }

    private void UpdatePotionUI()
    {
        if (potionCountText != null)
        {
            potionCountText.text = currentPotions + "/" + maxPotions;
        }
    }

    private IEnumerator ShowPotionUseEffect()
    {
        potionUseEffect.SetActive(true);

        Transform effectTransform = potionUseEffect.transform;
        float duration = 0.2f;

        Image effectImage = potionUseEffect.GetComponent<Image>();
        Color originalColor = effectImage.color;
        effectImage.color = Color.white;

        float time = 0;
        Vector3 originalScale = effectTransform.localScale;
        Vector3 targetScale = originalScale * 0.8f;

        while (time < duration / 2)
        {
            time += Time.deltaTime;
            float t = time / (duration / 2);
            effectTransform.localScale = Vector3.Lerp(originalScale, targetScale, t);
            yield return null;
        }

        time = 0;
        while (time < duration / 2)
        {
            time += Time.deltaTime;
            float t = time / (duration / 2);
            effectTransform.localScale = Vector3.Lerp(targetScale, originalScale, t);
            yield return null;
        }

        effectImage.color = originalColor;
        effectTransform.localScale = originalScale;
        potionUseEffect.SetActive(true);
    }

    public void SetShopOpen(bool isOpen)
    {
        isShopOpen = isOpen;
    }


    public void SaveHealthState()
    {

        PlayerPrefs.SetInt("PlayerCurrentHealth", currentHealth);
        PlayerPrefs.SetInt("PlayerCurrentPotions", currentPotions);
        PlayerPrefs.SetInt("IsTransitioningScene", 1);
        PlayerPrefs.Save();


    }
}