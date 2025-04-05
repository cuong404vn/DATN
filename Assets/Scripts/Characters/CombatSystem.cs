using UnityEngine;

public class CombatSystem : MonoBehaviour
{
    public int attackDamage = 1;
    public float attackRange = 2.5f;
    public LayerMask enemyLayer;

    [Header("Attack Method")]
    public bool useRaycastAttack = true;
    public float raycastWidth = 2f;

    private Animator animator;
    private PlayerController playerController;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        animator = GetComponent<Animator>();
        playerController = GetComponent<PlayerController>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        // Nếu enemyLayer chưa được thiết lập, tự động gán layer của enemy
        if (enemyLayer.value == 0)
        {
            enemyLayer = LayerMask.GetMask("Enemy");
            if (enemyLayer.value == 0)
                enemyLayer = -1; // Tất cả các layer
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Attack();
        }
    }

    void Attack()
    {
        // Trigger Animation
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }

        if (useRaycastAttack)
            ApplyRaycastDamage();
        else
            ApplyDamage();
    }

    void ApplyRaycastDamage()
    {
        // Xác định hướng tấn công dựa trên hướng nhìn của player
        Vector2 attackDirection = Vector2.right;

        // Nếu có SpriteRenderer, kiểm tra hướng
        if (spriteRenderer != null && spriteRenderer.flipX)
        {
            attackDirection = Vector2.left;
        }

        // Tạo box cast với chiều rộng
        RaycastHit2D[] hits = Physics2D.BoxCastAll(
            transform.position,
            new Vector2(attackRange, raycastWidth),
            0f,
            attackDirection,
            attackRange
        );

        bool hitAnyEnemy = false;

        foreach (RaycastHit2D hit in hits)
        {
            // Bỏ qua player và các vật thể không phải kẻ địch
            if (hit.collider.CompareTag("Player") || hit.transform == transform)
                continue;

            // Chỉ xử lý kẻ địch
            bool isEnemy = hit.collider.CompareTag("Enemy") ||
                           hit.collider.name.ToLower().Contains("enemy") ||
                           hit.collider.GetComponent<EnemyHealth>() != null ||
                           hit.collider.GetComponent<EnemyBehavior>() != null;

            if (!isEnemy)
                continue;

            // Tìm và gây sát thương cho tất cả EnemyHealth
            EnemyHealth enemyHealth = hit.collider.GetComponent<EnemyHealth>();

            if (enemyHealth == null)
                enemyHealth = hit.collider.GetComponentInChildren<EnemyHealth>();

            if (enemyHealth == null)
                enemyHealth = hit.collider.GetComponentInParent<EnemyHealth>();

            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(attackDamage);
                hitAnyEnemy = true;
            }
        }

        if (!hitAnyEnemy)
        {
            ApplyDamage();
        }
    }

    void ApplyDamage()
    {
        // Find enemies in range
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, attackRange, enemyLayer);

        if (hitEnemies.Length == 0)
        {
            // Sử dụng tất cả các layer
            Collider2D[] allColliders = Physics2D.OverlapCircleAll(transform.position, attackRange);

            foreach (Collider2D col in allColliders)
            {
                // Bỏ qua player và các vật thể không phải kẻ địch
                if (col.CompareTag("Player") || col.transform == transform)
                    continue;

                // Chỉ xử lý kẻ địch
                bool isEnemy = col.CompareTag("Enemy") ||
                              col.name.ToLower().Contains("enemy") ||
                              col.GetComponent<EnemyHealth>() != null ||
                              col.GetComponent<EnemyBehavior>() != null;

                if (!isEnemy)
                    continue;

                // Kiểm tra xem có phải là kẻ địch không
                EnemyHealth enemyHealth = col.GetComponent<EnemyHealth>();
                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(attackDamage);
                }
                else
                {
                    // Kiểm tra thêm các component khác có thể có
                    if (col.GetComponent<EnemyBehavior>() != null ||
                        col.name.ToLower().Contains("enemy") ||
                        col.CompareTag("Enemy"))
                    {
                        // Thử tìm EnemyHealth trong các con hoặc cha
                        EnemyHealth health = col.GetComponentInChildren<EnemyHealth>();
                        if (health == null)
                            health = col.GetComponentInParent<EnemyHealth>();

                        if (health != null)
                        {
                            health.TakeDamage(attackDamage);
                        }
                    }
                }
            }
            return;
        }

        // Apply damage
        foreach (Collider2D enemy in hitEnemies)
        {
            // Bỏ qua player và các vật thể không phải kẻ địch
            if (enemy.CompareTag("Player") || enemy.transform == transform)
                continue;

            EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(attackDamage);
            }
            else
            {
                // Thử tìm trong các object con
                EnemyHealth childHealth = enemy.GetComponentInChildren<EnemyHealth>();
                if (childHealth != null)
                {
                    childHealth.TakeDamage(attackDamage);
                }
            }
        }
    }

    public void OnAttackAnimationEvent()
    {
        if (useRaycastAttack)
            ApplyRaycastDamage();
        else
            ApplyDamage();
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        if (useRaycastAttack)
        {
            Vector3 direction = Vector3.right; // Mặc định phải
            Vector3 boxSize = new Vector3(attackRange, raycastWidth, 0.1f);

            // vẽ box cast
            Gizmos.DrawWireCube(transform.position + direction * (attackRange / 2), boxSize);

            // vẽ thêm box cast về bên trái
            direction = Vector3.left;
            Gizmos.color = Color.blue;
            Gizmos.DrawWireCube(transform.position + direction * (attackRange / 2), boxSize);
        }
        else
        {
            Gizmos.DrawWireSphere(transform.position, attackRange);
        }
    }
}