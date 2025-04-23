using UnityEngine;

public class CombatSystem : MonoBehaviour
{
    public int attackDamage = 1;
    public float attackRange = 1.5f; // Giảm phạm vi tấn công
    public LayerMask enemyLayer;

    [Header("Attack Method")]
    public bool useRaycastAttack = true;
    public float raycastWidth = 1.5f; // Giảm chiều rộng của raycast

    [Header("Attack Point")]
    public Transform attackPoint; // Điểm gây sát thương
    public float attackPointDistance = 1.2f; // Giảm khoảng cách từ nhân vật đến điểm tấn công

    [Header("Attack Cooldown")]
    public float attackCooldown = 0.5f; // Thời gian giữa các lần tấn công
    private float lastAttackTime = 0f; // Thời gian tấn công gần nhất

    private Animator animator;
    private PlayerController playerController;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        animator = GetComponent<Animator>();
        playerController = GetComponent<PlayerController>();
        spriteRenderer = GetComponent<SpriteRenderer>();


        if (attackPoint == null)
        {
            GameObject attackPointObj = new GameObject("AttackPoint");
            attackPoint = attackPointObj.transform;
            attackPoint.parent = transform;
            UpdateAttackPointPosition();
        }

        if (enemyLayer.value == 0)
        {
            enemyLayer = LayerMask.GetMask("Enemy");
            if (enemyLayer.value == 0)
                enemyLayer = -1;
        }
    }

    void Update()
    {

        UpdateAttackPointPosition();


        if (Input.GetMouseButtonDown(0) && Time.time - lastAttackTime >= attackCooldown)
        {
            lastAttackTime = Time.time;
            StartAttack();
        }
    }

    void UpdateAttackPointPosition()
    {
        if (attackPoint != null)
        {

            float direction = (spriteRenderer != null && spriteRenderer.flipX) ? -1f : 1f;


            attackPoint.localPosition = new Vector3(direction * attackPointDistance, 0, 0);
        }
    }

    void StartAttack()
    {
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }



    }


    public void OnAttackAnimationEvent()
    {
        if (useRaycastAttack)
            ApplyRaycastDamage();
        else
            ApplyDamage();
    }

    void ApplyRaycastDamage()
    {
        if (attackPoint == null) return;

        Vector2 attackDirection = Vector2.right;

        if (spriteRenderer != null && spriteRenderer.flipX)
        {
            attackDirection = Vector2.left;
        }


        RaycastHit2D[] hits = Physics2D.BoxCastAll(
            attackPoint.position,
            new Vector2(attackRange, raycastWidth),
            0f,
            attackDirection,
            0.1f // Giảm khoảng cách tấn công xuống gần như là 0
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


                Vector2 knockbackDirection = (hit.transform.position - transform.position);

                knockbackDirection.y = 0;


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
        if (attackPoint == null) return;


        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayer);

        if (hitEnemies.Length == 0)
        {
            Collider2D[] allColliders = Physics2D.OverlapCircleAll(attackPoint.position, attackRange);

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


                    Vector2 knockbackDirection = (col.transform.position - transform.position);

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


                            Vector2 knockbackDirection = (col.transform.position - transform.position);

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


                Vector2 knockbackDirection = (enemy.transform.position - transform.position);

                knockbackDirection.y = 0;

                enemyHealth.ApplyKnockback(knockbackDirection.normalized);
            }
            else
            {
                EnemyHealth childHealth = enemy.GetComponentInChildren<EnemyHealth>();
                if (childHealth != null)
                {
                    childHealth.TakeDamage(attackDamage);


                    Vector2 knockbackDirection = (enemy.transform.position - transform.position);

                    knockbackDirection.y = 0;

                    childHealth.ApplyKnockback(knockbackDirection.normalized);
                }
            }
        }
    }

    void OnDrawGizmosSelected()
    {

        if (attackPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(attackPoint.position, 0.2f);

            Gizmos.color = Color.red;
            if (useRaycastAttack)
            {
                Vector3 direction = Vector3.right;
                if (spriteRenderer != null && spriteRenderer.flipX)
                    direction = Vector3.left;

                Vector3 boxSize = new Vector3(attackRange, raycastWidth, 0.1f);
                Gizmos.DrawWireCube(attackPoint.position, boxSize);
            }
            else
            {
                Gizmos.DrawWireSphere(attackPoint.position, attackRange);
            }
        }
        else
        {

            Vector3 direction = Vector3.right;
            float distance = attackPointDistance;
            if (spriteRenderer != null && Application.isPlaying)
            {
                if (spriteRenderer.flipX)
                {
                    direction = Vector3.left;
                }
            }

            Vector3 pointPosition = transform.position + direction * distance;

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(pointPosition, 0.2f);

            Gizmos.color = Color.red;
            if (useRaycastAttack)
            {
                Vector3 boxSize = new Vector3(attackRange, raycastWidth, 0.1f);
                Gizmos.DrawWireCube(pointPosition, boxSize);
            }
            else
            {
                Gizmos.DrawWireSphere(pointPosition, attackRange);
            }
        }
    }
}