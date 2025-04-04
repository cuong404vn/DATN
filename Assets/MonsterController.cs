using UnityEngine;

public class MonsterController : MonoBehaviour
{
    public Transform pointA;
    public Transform pointB;
    public float patrolSpeed = 2f;
    private bool movingToB = true;

    public float detectionRange = 10f;
    public float attackRange = 5f;
    public GameObject projectilePrefab;
    public float projectileSpeed = 10f;
    public float attackCooldown = 1f;
    private float lastAttackTime;

    private Transform player;
    private bool playerDetected = false;
    private float initialScaleX;
    private Rigidbody2D rb;

    void Start()
    {
        // Lưu Scale ban đầu
        initialScaleX = Mathf.Abs(transform.localScale.x);

        // Lấy Rigidbody2D
        rb = GetComponent<Rigidbody2D>();
        if (rb == null)
        {
            Debug.LogError("Monster không có Rigidbody2D! Vui lòng thêm Rigidbody2D.");
            enabled = false;
            return;
        }

        // Tìm Player
        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        else
        {
            Debug.LogError("Không tìm thấy GameObject với tag 'Player'! Vui lòng kiểm tra tag của Player.");
            enabled = false;
            return;
        }

        // Kiểm tra các tham chiếu
        if (pointA == null || pointB == null)
        {
            Debug.LogError("PointA hoặc PointB chưa được gán!");
            enabled = false;
            return;
        }

        if (projectilePrefab == null)
        {
            Debug.LogError("Projectile Prefab chưa được gán!");
            enabled = false;
            return;
        }

        // Đặt Monster ở giữa PointA và PointB
        transform.position = (pointA.position + pointB.position) / 2;
    }

    void FixedUpdate()
    {
        if (player == null) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= detectionRange)
        {
            playerDetected = true;
            FacePlayer();
            ChasePlayer();
        }
        else
        {
            playerDetected = false;
            PatrolBetweenPoints();
        }
    }

    void Update()
    {
        // Chỉ xử lý bắn chưởng trong Update để đảm bảo thời gian chính xác
        if (playerDetected)
        {
            float distanceToPlayer = Vector2.Distance(transform.position, player.position);
            if (distanceToPlayer <= attackRange && Time.time > lastAttackTime + attackCooldown)
            {
                ShootProjectile();
                lastAttackTime = Time.time;
            }
        }
    }

    void PatrolBetweenPoints()
    {
        // Xác định điểm đích
        Vector2 targetPoint = movingToB ? pointB.position : pointA.position;

        // Di chuyển đến điểm đích (chỉ di chuyển theo trục X)
        Vector2 direction = new Vector2(targetPoint.x - transform.position.x, 0).normalized;
        Vector2 newPosition = (Vector2)transform.position + direction * patrolSpeed * Time.deltaTime;
        rb.MovePosition(newPosition);

        // Quay mặt theo hướng di chuyển
        if (targetPoint.x > transform.position.x)
            transform.localScale = new Vector3(initialScaleX, transform.localScale.y, transform.localScale.z); // Quay phải
        else
            transform.localScale = new Vector3(-initialScaleX, transform.localScale.y, transform.localScale.z); // Quay trái

        // Đổi hướng khi đến gần điểm đích (chỉ so sánh trục X)
        if (Mathf.Abs(transform.position.x - targetPoint.x) < 0.1f)
        {
            movingToB = !movingToB;
            Debug.Log("Đổi hướng: " + (movingToB ? "Hướng PointB" : "Hướng PointA"));
        }
    }

    void ChasePlayer()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        Vector2 newPosition = (Vector2)transform.position + direction * patrolSpeed * Time.deltaTime;
        rb.MovePosition(newPosition);
    }

    void FacePlayer()
    {
        if (player.position.x > transform.position.x)
            transform.localScale = new Vector3(initialScaleX, transform.localScale.y, transform.localScale.z);
        else
            transform.localScale = new Vector3(-initialScaleX, transform.localScale.y, transform.localScale.z);
    }

    void ShootProjectile()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        GameObject projectile = Instantiate(projectilePrefab, transform.position, Quaternion.identity);
        Rigidbody2D rb = projectile.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = direction * projectileSpeed;
        }
        else
        {
            Debug.LogError("Projectile không có Rigidbody2D! Vui lòng thêm Rigidbody2D vào prefab.");
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, detectionRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}