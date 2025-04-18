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
    private EnemyHealthBar healthBarScript;

    private Animator anim;
    private MeleeEnemyMovement enemyMovement;


    public AudioClip hitSound;
    public AudioClip deathSound;
    private AudioSource audioSource;

    void Start()
    {
        currentHealth = maxHealth;
        if (itemDrop == null)
            itemDrop = GetComponent<ItemDrop>();

        if (exitGateSpawnPoint == null && isBoss)
            exitGateSpawnPoint = transform;

        anim = GetComponent<Animator>();
        enemyMovement = GetComponent<MeleeEnemyMovement>();


        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();


        if (healthBarPrefab != null)
        {
            healthBarInstance = Instantiate(healthBarPrefab, transform.position + new Vector3(0, 1.5f, 0), Quaternion.identity);
            healthBarScript = healthBarInstance.GetComponent<EnemyHealthBar>();

            if (healthBarScript != null)
            {
                healthBarScript.target = transform;
                healthBarScript.SetHealth(currentHealth, maxHealth);
                healthBarScript.SetName(enemyName);
            }
        }
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        if (hitSound != null && audioSource != null)
            audioSource.PlayOneShot(hitSound);

        if (hitEffect != null)
            Instantiate(hitEffect, transform.position, Quaternion.identity);

        StartCoroutine(FlashEffect());


        if (healthBarScript != null)
            healthBarScript.SetHealth(currentHealth, maxHealth);

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        if (anim != null)
            anim.SetTrigger("die");

        if (deathSound != null && audioSource != null)
            audioSource.PlayOneShot(deathSound);

        if (enemyMovement != null)
            enemyMovement.enabled = false;

        Collider2D collider = GetComponent<Collider2D>();
        if (collider != null)
            collider.enabled = false;

        if (deathEffect != null)
            Instantiate(deathEffect, transform.position, Quaternion.identity);

        if (GameManager.Instance != null)
            GameManager.Instance.NotifyEnemyDefeated();

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
