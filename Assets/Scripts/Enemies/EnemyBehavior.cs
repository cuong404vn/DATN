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

    public Transform[] patrolPoints; 
    private int currentPatrolIndex = 0;

    private Transform player;
    private Rigidbody2D rb;
    private SpriteRenderer sprite;
    private Animator animator;

    private float lastAttackTime;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        sprite = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        player = GameObject.FindGameObjectWithTag("Player").transform;

        
        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            patrolPoints = new Transform[2];

            GameObject point1 = new GameObject("PatrolPoint1");
            point1.transform.position = transform.position + Vector3.left * 3;
            patrolPoints[0] = point1.transform;

            GameObject point2 = new GameObject("PatrolPoint2");
            point2.transform.position = transform.position + Vector3.right * 3;
            patrolPoints[1] = point2.transform;
        }
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
        
        if (patrolPoints.Length == 0)
        {
            currentState = EnemyState.Idle;
            return;
        }

     
        Transform targetPoint = patrolPoints[currentPatrolIndex];
        Vector2 direction = (targetPoint.position - transform.position).normalized;

        rb.linearVelocity = direction * moveSpeed;

       
        if (direction.x > 0)
            sprite.flipX = false;
        else if (direction.x < 0)
            sprite.flipX = true;

     
        if (Vector2.Distance(transform.position, targetPoint.position) < 0.1f)
        {
            currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
        }
    }

    void Chase()
    {
        
        Vector2 direction = (player.position - transform.position).normalized;
        rb.linearVelocity = direction * chaseSpeed;

        
        if (direction.x > 0)
            sprite.flipX = false;
        else if (direction.x < 0)
            sprite.flipX = true;
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

        if (patrolPoints != null && patrolPoints.Length > 0)
        {
            Gizmos.color = Color.blue;
            for (int i = 0; i < patrolPoints.Length; i++)
            {
                if (patrolPoints[i] != null)
                {
                    Gizmos.DrawSphere(patrolPoints[i].position, 0.2f);

                    if (i < patrolPoints.Length - 1 && patrolPoints[i + 1] != null)
                        Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[i + 1].position);
                    else if (patrolPoints[0] != null)
                        Gizmos.DrawLine(patrolPoints[i].position, patrolPoints[0].position);
                }
            }
        }
    }
}