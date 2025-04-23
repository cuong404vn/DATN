using UnityEngine;
using UnityEngine.UI;

public class Boss : MonoBehaviour
{
    [Header("Boss Stats")]
    public int maxHealth = 10;
    public int meleeDamage = 20;
    public float moveSpeed = 1f;
    public float moveDistance = 3f;
    public float detectionRange = 5f;
    public float meleeAttackRange = 2f;
    public float meleeAttackCooldown = 1.5f;
    public float specialAttackRange = 4f;
    public float specialAttackCooldown = 3f;

    [Header("References")]
    public GameObject bulletPrefab; 
    public GameObject exitGatePrefab; 
    public Transform exitGateSpawnPoint; 
    public GameObject healthBarPrefab; 
    public AudioClip meleeAttackSound;
    public AudioClip specialAttackSound;
    public AudioClip deathSound;

    private int currentHealth;
    private Transform player;
    private bool isPlayerInRange = false;
    private float lastMeleeAttackTime;
    private float lastSpecialAttackTime;
    private Vector3 startPosition;
    private bool movingRight = true;
    private Rigidbody2D rb;
    private Animator anim;
    private AudioSource audioSource;
    private GameObject healthBarInstance;
    private Slider healthBarSlider;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        startPosition = transform.position;
        currentHealth = maxHealth;

        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null)
        {
            Debug.LogError("Player not found! Ensure the Player has the 'Player' tag.");
        }

        if (healthBarPrefab != null)
        {
            healthBarInstance = Instantiate(healthBarPrefab, transform.position + new Vector3(0, 1, 0), Quaternion.identity);
            healthBarSlider = healthBarInstance.GetComponentInChildren<Slider>();
            if (healthBarSlider != null)
            {
                healthBarSlider.maxValue = maxHealth;
                healthBarSlider.value = currentHealth;
            }
            else
            {
                Debug.LogWarning("Slider component not found in healthBarPrefab!");
            }
        }
        else
        {
            Debug.LogWarning("Health Bar Prefab is not assigned in BossController!");
        }
    }

    void Update()
    {
        if (player == null || currentHealth <= 0)
        {
            Patrol();
            return;
        }

        if (healthBarInstance != null)
        {
            healthBarInstance.transform.position = transform.position + new Vector3(0, 1, 0);
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        Debug.Log("Distance to Player: " + distanceToPlayer);

        if (distanceToPlayer <= detectionRange)
        {
            isPlayerInRange = true;
        }
        else
        {
            isPlayerInRange = false;
        }

        if (isPlayerInRange)
        {
            if (distanceToPlayer <= meleeAttackRange)
            {
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
                anim.SetBool("isWalking", false);

                if (Time.time >= lastMeleeAttackTime + meleeAttackCooldown)
                {
                    MeleeAttack();
                    lastMeleeAttackTime = Time.time;
                }
            }
            else if (distanceToPlayer <= specialAttackRange && Time.time >= lastSpecialAttackTime + specialAttackCooldown)
            {
                SpecialAttack();
                lastSpecialAttackTime = Time.time;
            }
            else
            {
                Vector2 direction = (player.position - transform.position).normalized;
                rb.linearVelocity = new Vector2(direction.x * moveSpeed, rb.linearVelocity.y);
                anim.SetBool("isWalking", true);
                anim.ResetTrigger("attack");
                anim.ResetTrigger("specialAttack");
            }

            if (player.position.x < transform.position.x)
                transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            else
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else
        {
            Patrol();
            anim.ResetTrigger("attack");
            anim.ResetTrigger("specialAttack");
        }
    }

    void Patrol()
    {
        if (movingRight)
        {
            rb.linearVelocity = new Vector2(moveSpeed, rb.linearVelocity.y);
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }
        else
        {
            rb.linearVelocity = new Vector2(-moveSpeed, rb.linearVelocity.y);
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        }

        if (transform.position.x > startPosition.x + moveDistance)
            movingRight = false;
        else if (transform.position.x < startPosition.x - moveDistance)
            movingRight = true;

        anim.SetBool("isWalking", true);
    }

    void MeleeAttack()
    {
        Debug.Log("Boss Melee Attack triggered!");
        anim.SetTrigger("attack");

        if (meleeAttackSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(meleeAttackSound);
        }

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, meleeAttackRange);
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                Debug.Log("Player hit by melee attack! Damage: " + meleeDamage);
                hit.GetComponent<PlayerHealth>().TakeDamage(meleeDamage);
            }
        }
    }

    void SpecialAttack()
    {
        Debug.Log("Boss Special Attack triggered!");
        anim.SetTrigger("specialAttack");

        if (specialAttackSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(specialAttackSound);
        }

        if (bulletPrefab != null && player != null)
        {
            GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
            BossBullet bulletScript = bullet.GetComponent<BossBullet>();
            Vector2 direction = (player.position - transform.position).normalized;
            bulletScript.SetDirection(direction);
        }
    }

    public void TakeDamage(int damage)
    {
        if (currentHealth <= 0) return;

        currentHealth -= damage;
        Debug.Log("Boss took damage! Current HP: " + currentHealth);

        if (healthBarSlider != null)
        {
            healthBarSlider.value = currentHealth;
        }

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        Debug.Log("Boss died!");
        anim.SetTrigger("die");

        if (deathSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(deathSound);
        }

        if (exitGatePrefab != null)
        {
            Vector3 spawnPosition = exitGateSpawnPoint != null ? exitGateSpawnPoint.position : transform.position;
            Instantiate(exitGatePrefab, spawnPosition, Quaternion.identity);
        }

        if (healthBarInstance != null)
        {
            Destroy(healthBarInstance);
        }

        rb.linearVelocity = Vector2.zero;
        GetComponent<Collider2D>().enabled = false;
        enabled = false; 
        Destroy(gameObject, 1f);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, meleeAttackRange);
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, specialAttackRange);
    }
}