using UnityEngine;
using System.Collections;

public class quairong : MonoBehaviour
{
    // Tuần tra
    public float patrolSpeed = 2f;
    public float patrolDistance = 5f;
    private Vector2 startPos;
    private bool movingRight = true;

    // Phát hiện Player
    public float detectionRange = 4f;
    public LayerMask playerLayer;
    private Transform player;

    // Bắn chưởng
    public GameObject projectilePrefab;
    public Transform firePoint;
    public float projectileSpeed = 10f;
    public float fireRate = 2f;
    private float nextFireTime;

    // Animation
    private Animator animator;
    private SpriteRenderer spriteRenderer;
    private bool isTriggeredWalk = false;

    // Âm thanh
    [Header("Sounds")]
    public AudioClip attackSound; // Âm thanh khi tấn công
    private AudioSource attackAudioSource;

    // Vật lý
    private Rigidbody2D rb;

    // Collider
    private BoxCollider2D boxCollider;
    private Vector2 colliderOffsetRight = new Vector2(0.1f, 0f);
    private Vector2 colliderOffsetLeft;

    // Trạng thái
    private bool isDead = false;

    void Start()
    {
        startPos = transform.position;
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        // Khởi tạo AudioSource
        attackAudioSource = GetComponent<AudioSource>();
        if (attackAudioSource == null) Debug.LogError("Không tìm thấy AudioSource trên quái!");
        attackAudioSource.clip = attackSound;
        attackAudioSource.loop = false; // Không lặp lại âm thanh Attack

        if (animator == null) Debug.LogError("Không tìm thấy Animator trên quái!");
        if (spriteRenderer == null) Debug.LogError("Không tìm thấy SpriteRenderer trên quái!");
        if (rb == null) Debug.LogError("Không tìm thấy Rigidbody2D trên quái!");
        if (boxCollider == null) Debug.LogError("Không tìm thấy BoxCollider2D trên quái!");
        if (player == null) Debug.LogError("Không tìm thấy Player!");

        colliderOffsetLeft = new Vector2(-colliderOffsetRight.x, colliderOffsetRight.y);
        boxCollider.offset = colliderOffsetRight;
    }

    void FixedUpdate()
    {
        if (isDead) return;

        bool playerInRange = IsPlayerInRange();

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        string currentState = "Unknown";
        if (stateInfo.IsName("Idle")) currentState = "Idle";
        else if (stateInfo.IsName("Walk")) currentState = "Walk";
        else if (stateInfo.IsName("Attack")) currentState = "Attack";
        else if (stateInfo.IsName("Die")) currentState = "Die";
        Debug.Log($"Current state: {currentState}");

        if (playerInRange)
        {
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            if (!stateInfo.IsName("Attack"))
            {
                Debug.Log("Triggering Attack animation for vampireFire");
                animator.SetTrigger("Attack");
                isTriggeredWalk = false;

                // Phát âm thanh Attack
                if (attackAudioSource != null && attackSound != null)
                {
                    attackAudioSource.Play();
                }
            }
            FacePlayer();
            if (Time.time >= nextFireTime)
            {
                Shoot();
                nextFireTime = Time.time + fireRate;
            }
        }
        else
        {
            if (!stateInfo.IsName("Walk") && !isTriggeredWalk)
            {
                Debug.Log("Triggering Walk animation for vampireFire");
                animator.SetTrigger("Walk");
                isTriggeredWalk = true;
            }
            Patrol();
        }
    }

    void Patrol()
    {
        float distanceFromStart = Mathf.Abs(transform.position.x - startPos.x);
        if (distanceFromStart >= patrolDistance)
        {
            movingRight = !movingRight;
        }

        float moveDirection = movingRight ? 1f : -1f;
        rb.linearVelocity = new Vector2(moveDirection * patrolSpeed, rb.linearVelocity.y);

        spriteRenderer.flipX = !movingRight;
        boxCollider.offset = spriteRenderer.flipX ? colliderOffsetLeft : colliderOffsetRight;
    }

    bool IsPlayerInRange()
    {
        if (player == null) return false;
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        Debug.Log($"Distance to Player: {distanceToPlayer}");
        return distanceToPlayer <= detectionRange;
    }

    void FacePlayer()
    {
        if (player == null) return;
        bool playerOnRight = player.position.x > transform.position.x;
        spriteRenderer.flipX = !playerOnRight;
        boxCollider.offset = spriteRenderer.flipX ? colliderOffsetLeft : colliderOffsetRight;
    }

    void Shoot()
    {
        if (projectilePrefab == null || firePoint == null) return;

        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        Rigidbody2D rbProj = projectile.GetComponent<Rigidbody2D>();

        Vector2 direction = spriteRenderer.flipX ? Vector2.left : Vector2.right;
        rbProj.linearVelocity = direction * projectileSpeed;

        SpriteRenderer projSprite = projectile.GetComponent<SpriteRenderer>();
        if (projSprite != null)
        {
            projSprite.flipX = spriteRenderer.flipX;
        }

        Destroy(projectile, 5f);
    }

    public void TakeDamage(int damage)
    {
        if (isDead) return;
        isDead = true;
        animator.SetTrigger("Die");
        GetComponent<Collider2D>().enabled = false;
        Destroy(gameObject, 1f);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.blue;
        Gizmos.DrawLine(startPos - Vector2.right * patrolDistance, startPos + Vector2.right * patrolDistance);
    }
}