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


        if (enemyLayer.value == 0)
        {
            enemyLayer = LayerMask.GetMask("Enemy");
            if (enemyLayer.value == 0)
                enemyLayer = -1;
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

        Vector2 attackDirection = Vector2.right;


        if (spriteRenderer != null && spriteRenderer.flipX)
        {
            attackDirection = Vector2.left;
        }


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

            if (hit.collider.CompareTag("Player") || hit.transform == transform)
                continue;


            bool isEnemy = hit.collider.CompareTag("Enemy") ||
                           hit.collider.name.ToLower().Contains("enemy") ||
                           hit.collider.GetComponent<EnemyHealth>() != null ||
                           hit.collider.GetComponent<EnemyBehavior>() != null;

            if (!isEnemy)
                continue;


            EnemyHealth enemyHealth = hit.collider.GetComponent<EnemyHealth>();

            if (enemyHealth == null)
                enemyHealth = hit.collider.GetComponentInChildren<EnemyHealth>();

            if (enemyHealth == null)
                enemyHealth = hit.collider.GetComponentInParent<EnemyHealth>();

            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(attackDamage);

                // Tính toán hướng đẩy lùi chính xác - từ người chơi đến kẻ địch
                Vector2 knockbackDirection = (hit.transform.position - transform.position);
                // Chỉ sử dụng hướng ngang và đảm bảo đẩy lùi theo hướng xa người chơi
                knockbackDirection.y = 0;

                // Áp dụng knockback
                enemyHealth.ApplyKnockback(knockbackDirection.normalized);

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

        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(transform.position, attackRange, enemyLayer);

        if (hitEnemies.Length == 0)
        {

            Collider2D[] allColliders = Physics2D.OverlapCircleAll(transform.position, attackRange);

            foreach (Collider2D col in allColliders)
            {

                if (col.CompareTag("Player") || col.transform == transform)
                    continue;


                bool isEnemy = col.CompareTag("Enemy") ||
                              col.name.ToLower().Contains("enemy") ||
                              col.GetComponent<EnemyHealth>() != null ||
                              col.GetComponent<EnemyBehavior>() != null;

                if (!isEnemy)
                    continue;


                EnemyHealth enemyHealth = col.GetComponent<EnemyHealth>();
                if (enemyHealth != null)
                {
                    enemyHealth.TakeDamage(attackDamage);

                    // Tính toán hướng đẩy lùi chính xác
                    Vector2 knockbackDirection = (col.transform.position - transform.position);
                    // Chỉ sử dụng hướng ngang
                    knockbackDirection.y = 0;

                    enemyHealth.ApplyKnockback(knockbackDirection.normalized);
                }
                else
                {

                    if (col.GetComponent<EnemyBehavior>() != null ||
                        col.name.ToLower().Contains("enemy") ||
                        col.CompareTag("Enemy"))
                    {

                        EnemyHealth health = col.GetComponentInChildren<EnemyHealth>();
                        if (health == null)
                            health = col.GetComponentInParent<EnemyHealth>();

                        if (health != null)
                        {
                            health.TakeDamage(attackDamage);

                            // Tính toán hướng đẩy lùi chính xác
                            Vector2 knockbackDirection = (col.transform.position - transform.position);
                            // Chỉ sử dụng hướng ngang
                            knockbackDirection.y = 0;

                            health.ApplyKnockback(knockbackDirection.normalized);
                        }
                    }
                }
            }
            return;
        }


        foreach (Collider2D enemy in hitEnemies)
        {

            if (enemy.CompareTag("Player") || enemy.transform == transform)
                continue;

            EnemyHealth enemyHealth = enemy.GetComponent<EnemyHealth>();
            if (enemyHealth != null)
            {
                enemyHealth.TakeDamage(attackDamage);

                // Tính toán hướng đẩy lùi chính xác
                Vector2 knockbackDirection = (enemy.transform.position - transform.position);
                // Chỉ sử dụng hướng ngang
                knockbackDirection.y = 0;

                enemyHealth.ApplyKnockback(knockbackDirection.normalized);
            }
            else
            {

                EnemyHealth childHealth = enemy.GetComponentInChildren<EnemyHealth>();
                if (childHealth != null)
                {
                    childHealth.TakeDamage(attackDamage);

                    // Tính toán hướng đẩy lùi chính xác
                    Vector2 knockbackDirection = (enemy.transform.position - transform.position);
                    // Chỉ sử dụng hướng ngang
                    knockbackDirection.y = 0;

                    childHealth.ApplyKnockback(knockbackDirection.normalized);
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
            Vector3 direction = Vector3.right;
            Vector3 boxSize = new Vector3(attackRange, raycastWidth, 0.1f);


            Gizmos.DrawWireCube(transform.position + direction * (attackRange / 2), boxSize);


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