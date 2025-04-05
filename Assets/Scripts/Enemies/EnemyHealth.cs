using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 3;
    public int currentHealth;
    public bool isBoss = false;

    public GameObject hitEffect;
    public GameObject deathEffect;

    public ItemDrop itemDrop;
    public Transform exitGateSpawnPoint;

    public string enemyName = "Enemy"; // Tên của quái, có thể được đặt từ Inspector
    public GameObject healthBarPrefab; // Prefab thanh máu
    private GameObject healthBarInstance;
    private Slider healthBarSlider;
    private Text enemyNameText;

    void Start()
    {
        currentHealth = maxHealth;
        if (itemDrop == null)
            itemDrop = GetComponent<ItemDrop>();

        if (exitGateSpawnPoint == null && isBoss)
            exitGateSpawnPoint = transform;

        // Tạo thanh máu
        if (healthBarPrefab != null)
        {
            healthBarInstance = Instantiate(healthBarPrefab, transform.position + new Vector3(0, 1.5f, 0), Quaternion.identity, transform);
            healthBarSlider = healthBarInstance.GetComponentInChildren<Slider>();
            enemyNameText = healthBarInstance.GetComponentInChildren<Text>();

            if (healthBarSlider != null)
                healthBarSlider.maxValue = maxHealth;
            healthBarSlider.value = currentHealth;

            if (enemyNameText != null)
                enemyNameText.text = enemyName;
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        if (hitEffect != null)
            Instantiate(hitEffect, transform.position, Quaternion.identity);

        StartCoroutine(FlashEffect());

        // Cập nhật thanh máu
        if (healthBarSlider != null)
            healthBarSlider.value = currentHealth;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (deathEffect != null)
            Instantiate(deathEffect, transform.position, Quaternion.identity);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.NotifyEnemyDefeated();
        }

        if (isBoss)
        {
            LevelManager levelManager = FindAnyObjectByType<LevelManager>();
            if (levelManager != null)
            {
                Vector3 spawnPosition = exitGateSpawnPoint != null ?
                    exitGateSpawnPoint.position : transform.position;
                levelManager.ShowExitGate(spawnPosition);
            }
        }

        if (itemDrop != null)
            itemDrop.DropItem();

        Destroy(healthBarInstance);
        Destroy(gameObject);
    }

    IEnumerator FlashEffect()
    {
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        Color originalColor = sprite.color;
        sprite.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        sprite.color = originalColor;
    }
}
