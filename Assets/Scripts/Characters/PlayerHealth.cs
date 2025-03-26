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

    public Image[] healthImages; // Mảng các hình ảnh tim
    public Sprite fullHeart;
    public Sprite emptyHeart;

    public GameObject gameOverPanel;
    public Button retryButton;
    public Button mainMenuButton;

    void Start()
    {
        currentHealth = maxHealth;
        UpdateHealthUI();

        // Gán các sự kiện cho các nút
        if (gameOverPanel != null)
        {
            // Tìm các nút trong gameOverPanel nếu chưa được gán
            if (retryButton == null)
                retryButton = gameOverPanel.transform.Find("RetryButton")?.GetComponent<Button>();

            if (mainMenuButton == null)
                mainMenuButton = gameOverPanel.transform.Find("MainMenuButton")?.GetComponent<Button>();

            // Gán sự kiện cho các nút
            if (retryButton != null)
                retryButton.onClick.AddListener(() => GameManager.Instance.RetryLevel());

            if (mainMenuButton != null)
                mainMenuButton.onClick.AddListener(() => GameManager.Instance.GoToMainMenu());
        }
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
        for (int i = 0; i < healthImages.Length; i++)
        {
            if (i < currentHealth)
                healthImages[i].sprite = fullHeart;
            else
                healthImages[i].sprite = emptyHeart;
        }
    }

    void Die()
    {
        // Hiển thị màn hình game over
        if (gameOverPanel != null)
        {
            gameOverPanel.SetActive(true);

            // Đảm bảo các nút được gán sự kiện
            if (retryButton != null && retryButton.onClick.GetPersistentEventCount() == 0)
                retryButton.onClick.AddListener(() => GameManager.Instance.RetryLevel());

            if (mainMenuButton != null && mainMenuButton.onClick.GetPersistentEventCount() == 0)
                mainMenuButton.onClick.AddListener(() => GameManager.Instance.GoToMainMenu());
        }

        // Vô hiệu hóa người chơi
        GetComponent<PlayerController>().enabled = false;

        // Có thể thêm animation chết ở đây
    }

    IEnumerator BecomeInvincible()
    {
        isInvincible = true;

        // Hiệu ứng nhấp nháy khi bị thương
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        for (float i = 0; i < invincibilityTime; i += 0.1f)
        {
            sprite.enabled = !sprite.enabled;
            yield return new WaitForSeconds(0.1f);
        }
        sprite.enabled = true;

        isInvincible = false;
    }

    // Thêm phương thức xử lý va chạm với bẫy và dung nham
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Trap"))
        {
            // Trừ 1 máu khi chạm bẫy
            TakeDamage(1);
        }
        else if (other.CompareTag("DungNham"))
        {
            // Chết ngay lập tức khi chạm dung nham
            currentHealth = 0;
            Die();
        }
    }
}