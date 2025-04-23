using UnityEngine;

public class FireballProjectile : MonoBehaviour
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



        if (other.CompareTag("Player") || other.CompareTag("Ground"))
        {

            if (other.CompareTag("Player"))
            {
                PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damage);
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



        if (collision.gameObject.CompareTag("Player") || collision.gameObject.CompareTag("Ground"))
        {

            if (collision.gameObject.CompareTag("Player"))
            {
                PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damage);
                }
            }


            Explode();
        }
    }

    void Explode()
    {
        if (hasExploded) return;

        hasExploded = true;


        rb.linearVelocity = Vector2.zero;


        if (fireballCollider != null)
            fireballCollider.enabled = false;


        if (animator != null && animator.runtimeAnimatorController != null && animator.HasState(0, Animator.StringToHash("Explode")))
        {

            animator.SetTrigger("Explode");


            Destroy(gameObject, explosionDuration);
        }
        else
        {

            if (explosionPrefab != null)
            {
                GameObject explosion = Instantiate(explosionPrefab, transform.position, Quaternion.identity);
                Destroy(explosion, explosionDuration);
            }
            else
            {
            }


            Destroy(gameObject, 0.1f);
        }


        if (explosionSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(explosionSound);
        }


        if (trailEffect != null)
        {
            trailEffect.Stop();
        }
    }


    public void OnExplosionAnimationFinished()
    {
        Destroy(gameObject);
    }
}