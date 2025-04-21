using UnityEngine;

public class SuicideEnemy : MonoBehaviour
{
    [Header("AI Settings")]
    public float detectionRadius = 5f;        // Bán kính phát hiện Player
    public float moveSpeed = 3f;              // Tốc độ chạy
    public float explosionRadius = 1.5f;      // Bán kính gây nổ
    public int damage = 20;                   // Sát thương gây ra khi nổ

    [Header("Explosion Effect & Sound")]
    public GameObject explosionEffect;        // Prefab hiệu ứng nổ
    public AudioClip explosionSound;          // Âm thanh phát nổ

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
            audioSource = gameObject.AddComponent<AudioSource>(); // Gắn AudioSource nếu chưa có
        }

        audioSource.playOnAwake = false;
    }

    void Update()
    {
        if (hasExploded) return;

        // Tìm Player theo tag nếu chưa tìm thấy
        if (player == null)
        {
            GameObject p = GameObject.FindGameObjectWithTag("Player");
            if (p != null) player = p.transform;
        }

        if (player != null)
        {
            float distance = Vector2.Distance(transform.position, player.position);

            // Nếu Player nằm trong vùng phát hiện => bắt đầu đuổi theo
            if (!isChasing && distance < detectionRadius)
            {
                isChasing = true;
                animator.SetBool("isChasing", true);
            }

            // Di chuyển về phía Player nếu đang đuổi
            if (isChasing)
            {
                Vector2 direction = (player.position - transform.position).normalized;
                transform.Translate(direction * moveSpeed * Time.deltaTime);

                // Quay mặt đúng hướng Player
                if (direction.x != 0)
                {
                    Vector3 scale = transform.localScale;
                    scale.x = direction.x > 0 ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
                    transform.localScale = scale;
                }
            }

            // Nếu Player nằm trong vùng nổ => phát nổ
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

        // Gọi animation nổ
        animator.SetBool("isExploding", true);

        // Gây sát thương cho Player trong vùng nổ
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

        // Phát âm thanh nổ
        if (explosionSound != null)
        {
            audioSource.PlayOneShot(explosionSound);
        }

        // Gọi hiệu ứng nổ
        if (explosionEffect != null)
        {
            Instantiate(explosionEffect, transform.position, Quaternion.identity);
        }

        // Tự hủy sau 1 giây (hoặc đúng bằng thời gian animation nổ)
        Destroy(gameObject, 1f);
    }

    // Vẽ vùng phát hiện và vùng nổ trong Scene view
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}
