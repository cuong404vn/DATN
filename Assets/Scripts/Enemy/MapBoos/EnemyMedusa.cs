using UnityEngine;
using System.Collections;

public class EnemyMedusa : MonoBehaviour
{
    public enum EnemyState { Idle, Patrol, Chase, Attack, Retreat, Hurt, Die }
    [Header("State Machine")]
    [SerializeField] private EnemyState currentState = EnemyState.Patrol;

    [Header("Components")]
    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Collider2D enemyCollider;
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Health")]
    [SerializeField] private int maxHealth = 3;
    private int currentHealth;

    [Header("Movement & Detection")]
    [SerializeField] private float patrolSpeed = 1.2f;
    [SerializeField] private float chaseSpeed = 2.5f;
    [SerializeField] private float retreatSpeed = 3f;
    [SerializeField] private float detectionRange = 7f;
    [SerializeField] private float patrolRange = 3f;
    [SerializeField] private float minimumDistanceFromPlayer = 5f;
    [SerializeField] private float attackRange = 6f;

    [Header("Attack")]
    [SerializeField] private GameObject rockPrefab;
    [SerializeField] private float attackCooldown = 2f;
    [SerializeField] private int attackDamage = 15;
    [SerializeField] private float rockSpawnHeight = 5f;
    [SerializeField] private float rockFallDelay = 0.5f;
    [SerializeField] private AudioClip attackChargeSound;



    private Transform player;
    private float lastAttackTime = -Mathf.Infinity;
    private bool isHurt = false;
    private bool isDead = false;
    private bool facingRight = true;
    private AudioSource audioSource;
    private Vector3 patrolCenter;
    private Vector3 currentPatrolTarget;
    private bool patrollingRight = true;


    private readonly int hashIsWalking = Animator.StringToHash("IsWalking");
    private readonly int hashAttack = Animator.StringToHash("Attack");
    private readonly int hashHurt = Animator.StringToHash("Hurt");
    private readonly int hashDie = Animator.StringToHash("Die");

    void Awake()
    {

        if (animator == null) animator = GetComponent<Animator>();
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (enemyCollider == null) enemyCollider = GetComponent<Collider2D>();
        if (spriteRenderer == null) spriteRenderer = GetComponent<SpriteRenderer>();


        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();


        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
        }
        else
        {
            
            enabled = false;
            return;
        }


        patrolCenter = transform.position;
        currentHealth = maxHealth;
    }

    void Start()
    {
        SetPatrolTarget(true);
    }

    void Update()
    {
        if (isDead || player == null || isHurt)
        {
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        UpdateCurrentState(distanceToPlayer);

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
            case EnemyState.Retreat:
                Retreat();
                break;
        }

        UpdateAnimator();
    }

    void UpdateCurrentState(float distanceToPlayer)
    {
        if (isHurt || isDead) return;


        if (distanceToPlayer < minimumDistanceFromPlayer)
        {
            currentState = EnemyState.Retreat;
        }

        else if (distanceToPlayer <= attackRange)
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
            {
                currentState = EnemyState.Patrol;
                SetPatrolTarget(transform.position.x < patrolCenter.x);
            }
        }
    }

    void Idle()
    {
        if (isHurt || isDead) return;
        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }

    void Patrol()
    {
        if (isHurt || isDead) return;

        float directionX = Mathf.Sign(currentPatrolTarget.x - transform.position.x);
        rb.linearVelocity = new Vector2(directionX * patrolSpeed, rb.linearVelocity.y);

        if (Mathf.Abs(transform.position.x - currentPatrolTarget.x) < 0.1f)
        {
            SetPatrolTarget(!patrollingRight);
        }
    }

    void SetPatrolTarget(bool goRight)
    {
        patrollingRight = goRight;
        float targetX = patrolCenter.x + (goRight ? patrolRange : -patrolRange);
        currentPatrolTarget = new Vector3(targetX, transform.position.y);
        UpdateFacingDirection(currentPatrolTarget);
    }

    void Chase()
    {
        if (isHurt || isDead) return;


        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceToPlayer < minimumDistanceFromPlayer)
        {
            currentState = EnemyState.Retreat;
            return;
        }


        Vector2 direction = (player.position - transform.position).normalized;
        rb.linearVelocity = new Vector2(direction.x * chaseSpeed, rb.linearVelocity.y);
        UpdateFacingDirection(player.position);
    }

    void Retreat()
    {
        if (isHurt || isDead) return;


        Vector2 direction = (transform.position - player.position).normalized;
        rb.linearVelocity = new Vector2(direction.x * retreatSpeed, rb.linearVelocity.y);


        UpdateFacingDirection(player.position);


        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        if (distanceToPlayer >= minimumDistanceFromPlayer)
        {
            currentState = EnemyState.Attack;
        }
    }

    void Attack()
    {
        if (isHurt || isDead) return;


        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);


        UpdateFacingDirection(player.position);


        if (Time.time >= lastAttackTime + attackCooldown)
        {
            lastAttackTime = Time.time;


            animator.SetTrigger(hashAttack);


            if (attackChargeSound != null && audioSource != null)
            {
                audioSource.PlayOneShot(attackChargeSound);
            }


            StartCoroutine(SpawnRockAbovePlayer());
        }
    }

    IEnumerator SpawnRockAbovePlayer()
    {

        yield return new WaitForSeconds(0.5f);

        if (rockPrefab != null && player != null)
        {

            Vector3 rockPosition = player.position + Vector3.up * rockSpawnHeight;


            GameObject warningEffect = CreateWarningEffect(player.position);


            yield return new WaitForSeconds(rockFallDelay);


            if (warningEffect != null)
            {
                Destroy(warningEffect);
            }


            GameObject rock = Instantiate(rockPrefab, rockPosition, Quaternion.identity);


            VienDaRoi rockComponent = rock.GetComponent<VienDaRoi>();
            if (rockComponent != null)
            {
                rockComponent.damage = attackDamage;
            }
            else
            {

                Rigidbody2D rockRb = rock.GetComponent<Rigidbody2D>();
                if (rockRb == null)
                {
                    rockRb = rock.AddComponent<Rigidbody2D>();
                }


                rockRb.gravityScale = 2f;
                rockRb.mass = 2f;


                RockDamage rockDamage = rock.AddComponent<RockDamage>();
                rockDamage.damage = attackDamage;


                Destroy(rock, 5f);
            }
        }
    }


    GameObject CreateWarningEffect(Vector3 position)
    {

        GameObject warningObj = new GameObject("RockWarning");
        warningObj.transform.position = new Vector3(position.x, position.y + 0.1f, position.z);


        SpriteRenderer warningSprite = warningObj.AddComponent<SpriteRenderer>();
        warningSprite.color = new Color(1f, 0f, 0f, 0.5f);


        warningSprite.sprite = CreateCircleSprite();


        warningObj.transform.localScale = new Vector3(1.5f, 0.2f, 1f);

        return warningObj;
    }


    Sprite CreateCircleSprite()
    {


        Texture2D texture = new Texture2D(128, 128);


        for (int y = 0; y < texture.height; y++)
        {
            for (int x = 0; x < texture.width; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), new Vector2(texture.width / 2, texture.height / 2));
                if (distance < texture.width / 2)
                {
                    texture.SetPixel(x, y, Color.white);
                }
                else
                {
                    texture.SetPixel(x, y, Color.clear);
                }
            }
        }

        texture.Apply();

        return Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
    }




    void UpdateAnimator()
    {
        bool isActuallyWalking = (currentState == EnemyState.Patrol ||
                                 currentState == EnemyState.Chase ||
                                 currentState == EnemyState.Retreat) &&
                                 Mathf.Abs(rb.linearVelocity.x) > 0.1f;

        animator.SetBool(hashIsWalking, isActuallyWalking && !isHurt && !isDead);
    }

    void UpdateFacingDirection(Vector3 targetPosition)
    {
        if (isHurt || isDead) return;

        bool shouldFaceRight = targetPosition.x > transform.position.x;

        if (shouldFaceRight != facingRight)
        {
            Flip();
        }
    }

    void Flip()
    {
        facingRight = !facingRight;
        transform.Rotate(0f, 180f, 0f);
    }

    void OnDrawGizmosSelected()
    {

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, detectionRange);


        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);


        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, minimumDistanceFromPlayer);


        if (Application.isPlaying)
        {
            Gizmos.color = Color.cyan;
            Vector3 rightEdge = patrolCenter + Vector3.right * patrolRange;
            Vector3 leftEdge = patrolCenter + Vector3.left * patrolRange;
            Gizmos.DrawLine(leftEdge + Vector3.up * 0.2f, rightEdge + Vector3.up * 0.2f);
            Gizmos.DrawSphere(leftEdge, 0.15f);
            Gizmos.DrawSphere(rightEdge, 0.15f);
        }
        else
        {
            Gizmos.color = Color.cyan;
            Vector3 currentPos = transform.position;
            Vector3 rightEdge = currentPos + Vector3.right * patrolRange;
            Vector3 leftEdge = currentPos + Vector3.left * patrolRange;
            Gizmos.DrawLine(leftEdge + Vector3.up * 0.2f, rightEdge + Vector3.up * 0.2f);
            Gizmos.DrawSphere(leftEdge, 0.15f);
            Gizmos.DrawSphere(rightEdge, 0.15f);
        }
    }
}