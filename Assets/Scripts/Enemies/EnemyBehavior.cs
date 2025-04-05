using UnityEngine;

public class EnemyBehavior : MonoBehaviour
{
    public enum EnemyState { Idle, Patrol, Chase, Attack }

    public EnemyState currentState = EnemyState.Patrol;

    public float moveSpeed = 2f;
    public float chaseSpeed = 3f;
    public float detectionRange = 5f;
    public float attackRange = 1.5f;
    public int attackDamage = 1;
    public float attackCooldown = 1f;

    public Transform patrolCenter;
    public int patrolRange = 4; 

    private Transform player;
    private Rigidbody2D rb;
    private SpriteRenderer sprite;
    private Animator animator;

    private float lastAttackTime;
    private Vector3 patrolTarget;
    private bool movingRight = true; 

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        player = GameObject.FindGameObjectWithTag("Player").transform;

        
        if (patrolCenter == null)
        {
            GameObject center = new GameObject("PatrolCenter");
            center.transform.position = transform.position;
            patrolCenter = center.transform;
        }

        
        patrolTarget = patrolCenter.position + Vector3.right * patrolRange;
    }

    void Update()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        UpdateState(distanceToPlayer);

        switch (currentState)
        {
            case EnemyState.Idle:
                Idle();
                break;
            case EnemyState.Patrol:
                Patrol();
                break;
            case EnemyState.Chase:
                Chase();
                break;
            case EnemyState.Attack:
                Attack();
                break;
        }

        UpdateAnimation();
    }

    void UpdateState(float distanceToPlayer)
    {
        if (distanceToPlayer <= attackRange)
        {
            currentState = EnemyState.Attack;
        }
        else if (distanceToPlayer <= detectionRange)
        {
            currentState = EnemyState.Chase;
        }
        else
        {
            if (currentState != EnemyState.Patrol && currentState != EnemyState.Idle)
                currentState = EnemyState.Patrol;
        }
    }

    void Idle()
    {
        rb.linearVelocity = Vector2.zero;
    }

    void Patrol()
    {
        if (patrolCenter == null) return;

        Vector2 direction = (patrolTarget - transform.position).normalized;
        rb.linearVelocity = direction * moveSpeed;

        
        sprite.flipX = direction.x < 0;

        
        if (Vector2.Distance(transform.position, patrolTarget) < 0.1f)
        {
            if (movingRight)
            {
                patrolTarget = patrolCenter.position + Vector3.left * patrolRange;
            }
            else
            {
                patrolTarget = patrolCenter.position + Vector3.right * patrolRange;
            }
            movingRight = !movingRight;
        }
    }

    void Chase()
    {
        Vector2 direction = (player.position - transform.position).normalized;
        rb.linearVelocity = new Vector2(direction.x * chaseSpeed, rb.linearVelocity.y);

        sprite.flipX = direction.x < 0;

        
        if (Vector2.Distance(player.position, patrolCenter.position) > detectionRange * 1.5f)
        {
            currentState = EnemyState.Patrol;
            patrolTarget = patrolCenter.position;
        }
    }

    void Attack()
    {
        rb.linearVelocity = Vector2.zero;

        if (Time.time - lastAttackTime >= attackCooldown)
        {
            lastAttackTime = Time.time;

            if (animator != null)
                animator.SetTrigger("Attack");

            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
                playerHealth.TakeDamage(attackDamage);
        }
    }

    void UpdateAnimation()
    {
        if (animator != null)
        {
            if (HasParameter("IsMoving", animator))
            {
                animator.SetBool("IsMoving", rb.linearVelocity.magnitude > 0.1f);
            }

            if (HasParameter("State", animator))
            {
                animator.SetInteger("State", (int)currentState);
            }
        }
    }

    bool HasParameter(string paramName, Animator animator)
    {
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.name == paramName)
                return true;
        }
        return false;
    }

    public void OnAttackAnimationEvent()
    {
        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceToPlayer <= attackRange)
        {
            PlayerHealth playerHealth = player.GetComponent<PlayerHealth>();
            if (playerHealth != null)
                playerHealth.TakeDamage(attackDamage);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        if (patrolCenter != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawSphere(patrolCenter.position, 0.2f);
            Gizmos.DrawLine(patrolCenter.position, patrolCenter.position + Vector3.right * patrolRange);
            Gizmos.DrawLine(patrolCenter.position, patrolCenter.position + Vector3.left * patrolRange);
        }
    }
}