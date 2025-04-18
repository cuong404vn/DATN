using UnityEngine;

public class PatrolShooterEnemy : MonoBehaviour
{
    [Header("References")]
    public Transform pointA; // Điểm tuần tra A
    public Transform pointB; // Điểm tuần tra B
    public Transform firePoint; // Điểm bắn chưởng
    public GameObject projectilePrefab; // Prefab chưởng
    public Transform player; // Tham chiếu đến Player

    [Header("Patrol Settings")]
    public float moveSpeed = 2f; // Tốc độ di chuyển
    public float waitTime = 1f; // Thời gian chờ tại mỗi điểm

    [Header("Detection Settings")]
    public float detectionRange = 5f; // Tầm phát hiện Player
    public LayerMask playerLayer; // Layer của Player

    [Header("Attack Settings")]
    public float shootInterval = 2f; // Thời gian giữa các lần bắn
    public float projectileSpeed = 10f; // Tốc độ chưởng
    public float projectileLifetime = 3f; // Thời gian tồn tại của chưởng
    public int contactDamage = 5; // Sát thương khi chạm
    public float shootAnimationDelay = 0.5f; // Delay sau khi bắn để animation hoàn tất

    private Transform targetPoint; // Điểm tuần tra hiện tại (A hoặc B)
    private float waitTimer = 0f; // Đếm thời gian chờ
    private float shootTimer = 0f; // Đếm thời gian bắn
    private Animator animator; // Animator của Enemy
    private bool isFacingRight = true; // Hướng mặt của Enemy
    private bool isShooting = false; // Trạng thái đang bắn
    private bool isIdle = false; // Trạng thái Idle
    private float shootAnimationTimer = 0f; // Đếm thời gian delay animation

    void Start()
    {
        animator = GetComponent<Animator>();
        player = GameObject.FindGameObjectWithTag("Player").transform;
        targetPoint = pointA; // Bắt đầu di chuyển đến PointA
        animator.SetBool("IsWalking", true);
        animator.SetBool("IsIdle", false);

        // Khởi tạo hướng ban đầu dựa trên vị trí PointA và PointB
        if (pointA.position.x < pointB.position.x)
        {
            isFacingRight = true; // Mặt phải nếu PointA ở bên trái
        }
        else
        {
            isFacingRight = false; // Mặt trái nếu PointA ở bên phải
        }
        UpdateSpriteDirection();
    }

    void Update()
    {
        if (isShooting)
        {
            // Đợi animation bắn hoàn tất
            shootAnimationTimer += Time.deltaTime;
            if (shootAnimationTimer >= shootAnimationDelay)
            {
                isShooting = false;
                shootAnimationTimer = 0f;
                // Quay lại trạng thái trước đó (Idle hoặc Walk)
                animator.SetBool("IsWalking", !isIdle);
                animator.SetBool("IsIdle", isIdle);
            }
            return;
        }

        // Phát hiện Player
        bool playerDetected = DetectPlayer();

        if (playerDetected)
        {
            // Dừng tuần tra hoặc Idle, bắn chưởng
            animator.SetBool("IsWalking", false);
            animator.SetBool("IsIdle", false);
            shootTimer += Time.deltaTime;
            if (shootTimer >= shootInterval)
            {
                Shoot();
                shootTimer = 0f;
                isShooting = true;
            }

            // Xoay về phía Player
            Vector2 direction = (player.position - transform.position).normalized;
            if (direction.x > 0 && !isFacingRight || direction.x < 0 && isFacingRight)
            {
                Flip();
            }
        }
        else
        {
            // Nếu không phát hiện Player, tuần tra hoặc Idle
            if (isIdle)
            {
                HandleIdle();
            }
            else
            {
                Patrol();
            }
        }
    }

    void Patrol()
    {
        if (isShooting) return; // Không di chuyển khi đang bắn

        // Di chuyển đến điểm mục tiêu
        transform.position = Vector2.MoveTowards(transform.position, targetPoint.position, moveSpeed * Time.deltaTime);

        // Xoay hướng dựa trên điểm mục tiêu
        bool movingToPointB = targetPoint == pointB;
        bool shouldFaceRight = movingToPointB ? (pointA.position.x < pointB.position.x) : (pointB.position.x < pointA.position.x);
        if (shouldFaceRight && !isFacingRight || !shouldFaceRight && isFacingRight)
        {
            Flip();
        }

        // Kiểm tra đến điểm mục tiêu
        if (Vector2.Distance(transform.position, targetPoint.position) < 0.1f)
        {
            // Chuyển sang trạng thái Idle
            isIdle = true;
            animator.SetBool("IsWalking", false);
            animator.SetBool("IsIdle", true);
        }
    }

    void HandleIdle()
    {
        waitTimer += Time.deltaTime;
        if (waitTimer >= waitTime)
        {
            // Chuyển điểm mục tiêu và quay lại trạng thái tuần tra
            targetPoint = (targetPoint == pointA) ? pointB : pointA;
            waitTimer = 0f;
            isIdle = false;
            animator.SetBool("IsWalking", true);
            animator.SetBool("IsIdle", false);
        }
    }

    bool DetectPlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, detectionRange, playerLayer);
        return hit.collider != null && hit.collider.CompareTag("Player");
    }

    void Shoot()
    {
        animator.SetTrigger("Shoot");
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, Quaternion.identity);
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        // Chưởng bay theo hướng Enemy đang đối mặt
        Vector2 direction = isFacingRight ? Vector2.right : Vector2.left;
        rb.linearVelocity = direction * projectileSpeed;

        // Lật sprite của chưởng dựa trên hướng di chuyển
        Vector3 projectileScale = projectile.transform.localScale;
        projectileScale.x = isFacingRight ? Mathf.Abs(projectileScale.x) : -Mathf.Abs(projectileScale.x);
        projectile.transform.localScale = projectileScale;

        Destroy(projectile, projectileLifetime);
    }

    void Flip()
    {
        isFacingRight = !isFacingRight;
        UpdateSpriteDirection();
    }

    void UpdateSpriteDirection()
    {
        Vector3 scale = transform.localScale;
        scale.x = isFacingRight ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
        transform.localScale = scale;
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(contactDamage);
            }
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
    }
}