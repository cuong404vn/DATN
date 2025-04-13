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

    public string enemyName = "Enemy";
    public GameObject healthBarPrefab;
    private GameObject healthBarInstance;
    private Slider healthBarSlider;
    private Text enemyNameText;

    private Animator anim;
    private MeleeEnemyMovement enemyMovement;

    // Thêm biến âm thanh
    public AudioClip hitSound; // Âm thanh khi bị đánh
    public AudioClip deathSound; // Âm thanh khi chết
    private AudioSource audioSource; // AudioSource để phát âm thanh

    void Start()
    {
        currentHealth = maxHealth;
        if (itemDrop == null)
            itemDrop = GetComponent<ItemDrop>();

        if (exitGateSpawnPoint == null && isBoss)
            exitGateSpawnPoint = transform;

        anim = GetComponent<Animator>();
        enemyMovement = GetComponent<MeleeEnemyMovement>();

        // Lấy hoặc thêm AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

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

        // Phát âm thanh khi bị đánh
        if (hitSound != null && audioSource != null)
        {
            Debug.Log("Playing hit sound for " + gameObject.name);
            audioSource.PlayOneShot(hitSound);
        }
        else
        {
            Debug.LogWarning("Hit sound or AudioSource is missing on " + gameObject.name);
        }

        if (hitEffect != null)
            Instantiate(hitEffect, transform.position, Quaternion.identity);

        StartCoroutine(FlashEffect());

        if (healthBarSlider != null)
            healthBarSlider.value = currentHealth;

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (anim != null)
        {
            anim.SetTrigger("die");
        }

        // Phát âm thanh khi chết
        if (deathSound != null && audioSource != null)
        {
            Debug.Log("Playing death sound for " + gameObject.name);
            audioSource.PlayOneShot(deathSound);
        }
        else
        {
            Debug.LogWarning("Death sound or AudioSource is missing on " + gameObject.name);
        }

        if (enemyMovement != null)
        {
            enemyMovement.enabled = false;
        }

        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
        {
            collider.enabled = false;
        }

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

        if (healthBarInstance != null)
            Destroy(healthBarInstance);

        Destroy(gameObject, 0.5f);
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