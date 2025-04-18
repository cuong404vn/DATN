using UnityEngine;

public class Fireballl : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private int damage = 10;
    [SerializeField] private float speed = 7f;
    [SerializeField] private float lifetime = 5f;
    [SerializeField] private float explosionDuration = 0.5f;

    [Header("Effects")]
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private AudioClip explosionSound;
    [SerializeField] private ParticleSystem trailEffect;


    private Rigidbody2D rb;
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private Collider2D fireballCollider;
    private AudioSource audioSource;


    private bool hasExploded = false;
    private float direction = 1f;
    private GameObject sourceEnemy;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        fireballCollider = GetComponent<Collider2D>();
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }


        Destroy(gameObject, lifetime);
    }


    public void Initialize(float direction, float speed, GameObject source = null)
    {
        this.direction = direction;
        this.speed = speed;
        this.sourceEnemy = source;


        rb.linearVelocity = new Vector2(direction * speed, 0);


        if (direction < 0 && spriteRenderer != null)
        {
            transform.localScale = new Vector3(-1, 1, 1);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (hasExploded) return;


        if (sourceEnemy != null && other.gameObject == sourceEnemy)
        {
            return;
        }

        Debug.Log($"Fireball hit: {other.gameObject.name} with tag: {other.tag}");


        if (other.CompareTag("Player") || other.CompareTag("Ground"))
        {

            if (other.CompareTag("Player"))
            {
                PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damage);
                    Debug.Log($"Dealt {damage} damage to player");
                }
            }


            Explode();
        }
    }


    void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasExploded) return;


        if (sourceEnemy != null && collision.gameObject == sourceEnemy)
        {
            return;
        }

        Debug.Log($"Fireball collision with: {collision.gameObject.name} with tag: {collision.gameObject.tag}");


        if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Ground"))
        {

            if (collision.gameObject.CompareTag("Player"))
            {
                PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damage);
                    Debug.Log($"Dealt {damage} damage to player (collision)");
                }
            }


            Explode();
        }
    }

    void Explode()
    {
        if (hasExploded) return;

        hasExploded = true;
        Debug.Log("Fireball exploding!");


        rb.linearVelocity = Vector2.zero;


        if (fireballCollider != null)
            fireballCollider.enabled = false;


        if (animator != null && animator.runtimeAnimatorController != null && animator.HasState(0, Animator.StringToHash("Explode")))
        {

            animator.SetTrigger("Explode");
            Debug.Log("Playing explosion animation");


            Destroy(gameObject, explosionDuration);
        }
        else
        {

            if (explosionPrefab != null)
            {
                GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
                Debug.Log($"Spawned explosion prefab: {explosionPrefab.name}");
                Destroy(explosion, explosionDuration);
            }
            else
            {
                Debug.LogWarning("No explosion prefab assigned and no animation available!");
            }


            Destroy(gameObject, 0.1f);
        }


        if (explosionSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(explosionSound);
            Debug.Log("Playing explosion sound");
        }


        if (trailEffect != null)
        {
            trailEffect.Stop();
        }
    }


    public void OnExplosionAnimationFinished()
    {
        Debug.Log("Explosion animation finished");
        Destroy(gameObject);
    }
}