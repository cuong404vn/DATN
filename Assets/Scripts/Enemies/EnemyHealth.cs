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

    [Header("Stun Settings")]
    public float stunDuration = 0.5f;
    public float knockbackForce = 3f;
    private bool isStunned = false;

    private GameObject healthBarInstance;
    private EnemyHealthBar healthBarScript;

    private Animator anim;
    private MeleeEnemyMovement enemyMovement;
    private Rigidbody2D rb;

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
        rb = GetComponent<Rigidbody2D>();

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


    public void ApplyKnockback(Vector2 direction)
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
            if (rb == null) return;
        }


        rb.constraints = RigidbodyConstraints2D.FreezeRotation;


        rb.linearVelocity = Vector2.zero;
        float originalGravity = rb.gravityScale;
        rb.gravityScale = 0;


        Vector3 currentPos = transform.position;
        Vector2 horizontalDirection = new Vector2(direction.x, 0).normalized;
        float knockbackDistance = knockbackForce * 0.2f;


        Collider2D collider = GetComponent<Collider2D>();
        if (collider == null)
        {

            Vector2 boxSize = new Vector2(0.5f, 1f);
            Vector2 boxCenter = new Vector2(currentPos.x, currentPos.y + 0.5f);


            RaycastHit2D[] hits = Physics2D.BoxCastAll(
                boxCenter,
                boxSize,
                0f,
                horizontalDirection,
                knockbackDistance,
                LayerMask.GetMask("Ground")
            );

            float minDistance = knockbackDistance;
            bool hitWall = false;

            foreach (RaycastHit2D hit in hits)
            {

                if (hit.collider.gameObject == gameObject)
                    continue;


                float hitDistance = hit.distance;
                if (hitDistance < minDistance)
                {
                    minDistance = hitDistance;
                    hitWall = true;
                }
            }


            Vector3 targetPos;
            if (hitWall)
            {

                targetPos = currentPos + (Vector3)(horizontalDirection * (minDistance - 0.5f));
            }
            else
            {

                targetPos = currentPos + (Vector3)(horizontalDirection * knockbackDistance);
            }


            StartCoroutine(KnockbackMove(currentPos, targetPos, 0.2f));
        }
        else
        {

            Vector2 boxSize = Vector2.zero;
            Vector2 boxCenter = Vector2.zero;


            if (collider is BoxCollider2D boxCollider)
            {
                boxSize = boxCollider.size;
                boxCenter = (Vector2)currentPos + boxCollider.offset;
            }
            else if (collider is CircleCollider2D circleCollider)
            {
                float radius = circleCollider.radius;
                boxSize = new Vector2(radius * 2, radius * 2);
                boxCenter = (Vector2)currentPos + circleCollider.offset;
            }
            else
            {

                boxSize = new Vector2(0.5f, 1f);
                boxCenter = new Vector2(currentPos.x, currentPos.y + 0.5f);
            }


            RaycastHit2D[] hits = Physics2D.BoxCastAll(
                boxCenter,
                boxSize,
                0f,
                horizontalDirection,
                knockbackDistance,
                LayerMask.GetMask("Ground")
            );

            float minDistance = knockbackDistance;
            bool hitWall = false;

            foreach (RaycastHit2D hit in hits)
            {

                if (hit.collider.gameObject == gameObject)
                    continue;


                float hitDistance = hit.distance;
                if (hitDistance < minDistance)
                {
                    minDistance = hitDistance;
                    hitWall = true;
                }
            }


            Vector3 targetPos;
            if (hitWall)
            {

                targetPos = currentPos + (Vector3)(horizontalDirection * (minDistance - 0.5f));
            }
            else
            {

                targetPos = currentPos + (Vector3)(horizontalDirection * knockbackDistance);
            }


            StartCoroutine(KnockbackMove(currentPos, targetPos, 0.2f));
        }


        StartCoroutine(StunEffect(originalGravity));
    }

    IEnumerator KnockbackMove(Vector3 startPos, Vector3 endPos, float duration)
    {
        float elapsed = 0;
        while (elapsed < duration && this != null && gameObject.activeInHierarchy)
        {
            transform.position = Vector3.Lerp(startPos, endPos, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (this != null && gameObject.activeInHierarchy)
            transform.position = endPos;
    }

    IEnumerator StunEffect(float originalGravity)
    {
        isStunned = true;


        if (anim != null)
            anim.SetTrigger("Hurt");


        if (enemyMovement != null)
            enemyMovement.enabled = false;


        yield return new WaitForSeconds(stunDuration);


        if (rb != null && this != null && gameObject.activeInHierarchy)
        {
            rb.linearVelocity = Vector2.zero;
            rb.gravityScale = originalGravity;
        }

        if (enemyMovement != null && this != null && gameObject.activeInHierarchy)
            enemyMovement.enabled = true;

        isStunned = false;
    }

    void Die()
    {
        if (anim != null)
        {
            anim.ResetTrigger("Attack");
            anim.ResetTrigger("Hurt");
            anim.SetBool("IsWalking", false);
            anim.SetBool("IsRunning", false);

            anim.SetTrigger("Die");
        }

        MonoBehaviour[] allScripts = GetComponents<MonoBehaviour>();
        foreach (MonoBehaviour script in allScripts)
        {
            if (script != this && !(script is AudioSource))
            {
                script.enabled = false;
            }
        }

        if (deathSound != null && audioSource != null)
            audioSource.PlayOneShot(deathSound);

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


        Destroy(gameObject, 0.9f);
    }

    IEnumerator FlashEffect()
    {
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        Color originalColor = sprite.color;
        sprite.color = Color.red;
        yield return new WaitForSeconds(0.5f);
        sprite.color = originalColor;
    }

    void OnTriggerEnter2D(Collider2D other)
    {

        if (other.CompareTag("DungNham"))
        {

            currentHealth = 0;
            Die();
        }
    }
}
