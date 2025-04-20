using UnityEngine;

public class SuicideEnemy : MonoBehaviour
{
    [Header("AI Settings")]
    public float detectionRadius = 5f;        
    public float moveSpeed = 3f;              
    public float explosionRadius = 1.5f;      
    public int damage = 20;                   

    [Header("Explosion Effect & Sound")]
    public GameObject explosionEffect;        
    public AudioClip explosionSound;          

    private Transform player;
    private bool isChasing = false;
    private bool hasExploded = false;

    private Animator animator;
    private AudioSource audioSource;

    void Start()
    {
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>(); 
        }

        audioSource.playOnAwake = false;
    }

    void Update()
    {
        if (hasExploded) return;

        
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        if (player != null)
        {
            float distance = Vector2.Distance(transform.position, player.position);

            if (!isChasing && distance < detectionRadius)
            {
                isChasing = true;
                animator.SetBool("isChasing", true);
            }

            if (isChasing)
            {
                Vector2 direction = (player.position - transform.position).normalized;
                transform.Translate(direction * moveSpeed * Time.deltaTime);

                if (direction.x != 0)
                {
                    Vector3 scale = transform.localScale;
                    scale.x = direction.x > 0 ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
                    transform.localScale = scale;
                }
            }

            if (distance < explosionRadius)
            {
                Explode();
            }
        }
    }

    void Explode()
    {
        if (hasExploded) return;

        hasExploded = true;

        animator.SetBool("isExploding", true);

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius);
        foreach (Collider2D hit in hits)
        {
            if (hit.CompareTag("Player"))
            {
                PlayerHealth playerHealth = hit.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damage);
                    Debug.Log("💥 Player trúng bom cảm tử! Gây " + damage + " sát thương.");
                }
            }
        }

        if (explosionSound != null)
        {
            audioSource.PlayOneShot(explosionSound);
        }

        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }

        Destroy(gameObject, 1f);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
