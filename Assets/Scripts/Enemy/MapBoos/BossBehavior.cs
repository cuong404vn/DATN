using UnityEngine;
using System.Collections;

public class BossBehavior : MonoBehaviour
{

    public enum EnemyState { Idle, Patrol, Chase, Attack, Hurt, Die, Retreat, JumpBack, FireAttack, RockAttack }
    [Header("State Machine")]
    [SerializeField] private EnemyState currentState = EnemyState.Patrol;
    [SerializeField] private EnemyState previousState;

    [Header("Components")]
    [SerializeField] private Animator animator;
    [SerializeField] private Rigidbody2D rb;
    [SerializeField] private Collider2D enemyCollider;



    [Header("Movement & Detection")]
    [SerializeField] private float patrolSpeed = 1.5f;
    [SerializeField] private float chaseSpeed = 3f;
    [SerializeField] private float retreatSpeed = 2f;
    [SerializeField] private float detectionRange = 6f;
    [SerializeField] private float patrolRange = 4f;

    [Header("Attack")]
    [SerializeField] private float attackRange = 1.5f;
    [SerializeField] private int attackDamage = 10;
    [SerializeField] private float attackCooldown = 3f;
    [SerializeField] private float attackDuration = 1f;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRadius = 0.5f;
    [SerializeField] private AudioClip attackSound;

    [Header("Magic Fire Attack")]
    [SerializeField] private float fireAttackRange = 8f;
    [SerializeField] private Transform firePoint;
    [SerializeField] private GameObject fireballPrefab;
    [SerializeField] private float fireballSpeed = 7f;
    [SerializeField] private float fireAttackDuration = 1.5f;
    [SerializeField] private AudioClip fireSound;
    [SerializeField] private int fireDamage = 8;

    [Header("Rock Attack")]
    [SerializeField] private GameObject rockPrefab;
    [SerializeField] private float rockAttackRange = 8f;
    [SerializeField] private float rockAttackDuration = 2f;
    [SerializeField] private AudioClip rockAttackSound;
    [SerializeField] private int rockDamage = 15;
    [SerializeField] private float rockSpawnHeight = 5f;
    [SerializeField] private float rockFallDelay = 0.5f;

    [Header("Retreat Behavior")]
    [SerializeField] private float initialRetreatDistance = 2f;
    [SerializeField] private float furtherJumpBackDistance = 5f;
    [SerializeField] private float retreatDelay = 0.2f;
    [SerializeField] private float jumpBackDelay = 0.3f;





    [SerializeField] private int rockCount = 3;

    [SerializeField] private float rockSpacing = 10f;

    private Transform player;

    private float lastAttackTime = -Mathf.Infinity;
    private float lastActionTime = -Mathf.Infinity;
    private bool isHurt = false;
    private bool isDead = false;
    private bool facingRight = true;
    private AudioSource audioSource;
    private Coroutine currentActionCoroutine;
    private bool useFireAttack = true;
    private bool isAttacking = false;
    private int rangedAttackType = 0;

    private Vector3 patrolCenter;
    private Vector3 currentPatrolTarget;
    private bool patrollingRight = true;

    private bool rockSpawning = false;

    private readonly int hashIsWalking = Animator.StringToHash("IsWalking");
    private readonly int hashIsRunning = Animator.StringToHash("IsRunning");
    private readonly int hashAttack = Animator.StringToHash("Attack");
    private readonly int hashFireAttack = Animator.StringToHash("Magic_fire");
    private readonly int hashRockAttack = Animator.StringToHash("Magic_blade");



    void Awake()
    {

        if (animator == null) animator = GetComponent<Animator>();
        if (rb == null) rb = GetComponent<Rigidbody2D>();
        if (enemyCollider == null) enemyCollider = GetComponent<Collider2D>();

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
    }

    void Start()
    {
        SetPatrolTarget(true);
        previousState = currentState;
        lastActionTime = Time.time - attackCooldown;
        lastAttackTime = Time.time - attackCooldown;
    }

    void Update()
    {
        if (isDead || player == null || isHurt)
        {
            return;
        }



        {

            previousState = currentState;
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
            case EnemyState.Retreat:
            case EnemyState.JumpBack:

                break;
        }

        UpdateAnimator();
    }

    void FixedUpdate()
    {
        if (isDead || isHurt)
        {
            if (rb.linearVelocity != Vector2.zero)
            {
                rb.linearVelocity = Vector2.zero;
            }
        }
    }

    void UpdateCurrentState(float distanceToPlayer)
    {

        if (isHurt || isDead) return;


        if (currentActionCoroutine != null)
        {
            return;
        }

        float distanceToPatrolCenter = Vector2.Distance(transform.position, patrolCenter);
        bool isWithinPatrolArea = distanceToPatrolCenter <= patrolRange * 1.5f;


        bool canAttack = Time.time - lastActionTime >= attackCooldown;

        if (!canAttack)
        {

            if (distanceToPlayer <= detectionRange && isWithinPatrolArea &&
                currentState != EnemyState.Attack && currentState != EnemyState.FireAttack &&
                currentState != EnemyState.RockAttack)
            {
                currentState = EnemyState.Chase;
            }
            return;
        }


        if (distanceToPlayer <= attackRange && isWithinPatrolArea)
        {

            useFireAttack = false;
            currentActionCoroutine = StartCoroutine(PerformFullAttackSequence());
        }
        else if (distanceToPlayer <= fireAttackRange && distanceToPlayer > attackRange && isWithinPatrolArea && useFireAttack)
        {

            if (rangedAttackType == 0)
                currentActionCoroutine = StartCoroutine(PerformFireAttack());
            else
                currentActionCoroutine = StartCoroutine(PerformRockAttack());
        }
        else if (distanceToPlayer <= detectionRange && isWithinPatrolArea)
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

    void SetState(EnemyState newState)
    {
        previousState = currentState;
        currentState = newState;
    }

    void Idle()
    {
        if (isHurt || isDead) return;

        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
    }


    void Patrol()
    {
        if (isHurt || isDead)
        {
            if (rb.linearVelocity.x != 0)
                rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
            return;
        }

        float directionX = Mathf.Sign(currentPatrolTarget.x - transform.position.x);
        rb.linearVelocity = new Vector2(directionX * patrolSpeed, rb.linearVelocity.y);

        if (Mathf.Abs(transform.position.x - currentPatrolTarget.x) < 0.1f)
        {
            SetPatrolTarget(!patrollingRight);
            rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        }
    }


    void SetPatrolTarget(bool goRight)
    {
        patrollingRight = goRight;
        float targetX = patrolCenter.x + (goRight ? patrolRange : -patrolRange);
        currentPatrolTarget = new Vector3(targetX, transform.position.y);
        UpdateFacingDirectionTowards(currentPatrolTarget);
    }

    void Chase()
    {
        if (isHurt || isDead) return;


        Vector2 direction = (player.position - transform.position).normalized;
        rb.linearVelocity = new Vector2(direction.x * chaseSpeed, rb.linearVelocity.y);
        UpdateFacingDirectionTowards(player.position);
    }

    IEnumerator PerformFullAttackSequence()
    {

        yield return StartCoroutine(PerformAttack());
        yield return StartCoroutine(RetreatAfterAttack());
        yield return StartCoroutine(JumpBackFurther());


        useFireAttack = true;
        rangedAttackType = Random.Range(0, 2);


        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        float distanceToPatrolCenter = Vector2.Distance(transform.position, patrolCenter);
        float currentRangedAttackRange = (rangedAttackType == 0) ? fireAttackRange : rockAttackRange;


        if (distanceToPlayer <= detectionRange && distanceToPatrolCenter <= patrolRange * 1.5f)
        {
            SetState(EnemyState.Chase);

            lastActionTime = Time.time;
        }
        else
        {
            SetState(EnemyState.Patrol);
            SetPatrolTarget(transform.position.x < patrolCenter.x);
            lastActionTime = Time.time;
        }


        currentActionCoroutine = null;
    }

    IEnumerator PerformAttack()
    {

        SetState(EnemyState.Attack);


        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        UpdateFacingDirectionTowards(player.position);


        animator.SetTrigger(hashAttack);


        if (attackSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(attackSound);
        }


        yield return new WaitForSeconds(attackDuration / 2);


        DealDamage();


        yield return new WaitForSeconds(attackDuration / 2);


        lastAttackTime = Time.time;
    }

    IEnumerator RetreatAfterAttack()
    {
        SetState(EnemyState.Retreat);





        yield return new WaitForSeconds(retreatDelay);


        Vector2 playerPosition = player.position;
        float directionToPlayer = Mathf.Sign(playerPosition.x - transform.position.x);
        Vector2 retreatPosition = new Vector2(transform.position.x - directionToPlayer * initialRetreatDistance, transform.position.y);


        float startTime = Time.time;
        float journeyLength = Vector2.Distance(transform.position, retreatPosition);
        float distanceCovered = 0;

        while (distanceCovered < journeyLength)
        {
            if (isDead || isHurt) yield break;

            float distanceThisFrame = retreatSpeed * Time.deltaTime;
            distanceCovered += distanceThisFrame;


            rb.linearVelocity = new Vector2(-directionToPlayer * retreatSpeed, rb.linearVelocity.y);


            UpdateFacingDirectionTowards(playerPosition);


            if (Mathf.Abs(transform.position.x - retreatPosition.x) < 0.1f)
            {
                break;
            }

            yield return null;
        }


        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);



    }

    IEnumerator JumpBackFurther()
    {
        SetState(EnemyState.JumpBack);





        yield return new WaitForSeconds(jumpBackDelay);


        Vector2 playerPosition = player.position;
        float directionToPlayer = Mathf.Sign(playerPosition.x - transform.position.x);
        Vector2 jumpBackPosition = new Vector2(transform.position.x - directionToPlayer * furtherJumpBackDistance, transform.position.y);


        Vector2 startPosition = transform.position;


        bool shouldFaceRight = directionToPlayer < 0;
        if (shouldFaceRight != facingRight)
        {
            Flip();
        }




        float jumpHeight = 3f;
        float gravity = Physics2D.gravity.magnitude * rb.gravityScale;
        float jumplinearVelocityY = Mathf.Sqrt(2 * gravity * jumpHeight);


        float jumpDistance = Mathf.Abs(jumpBackPosition.x - startPosition.x);
        float jumpTime = jumpDistance / (retreatSpeed * 1.5f);


        rb.linearVelocity = new Vector2(-directionToPlayer * retreatSpeed * 1.5f, jumplinearVelocityY);





        float elapsedTime = 0f;
        float maxJumpTime = jumpTime * 2f;

        while (elapsedTime < maxJumpTime)
        {
            if (isDead || isHurt) yield break;

            elapsedTime += Time.deltaTime;


            rb.linearVelocity = new Vector2(-directionToPlayer * retreatSpeed * 1.5f, rb.linearVelocity.y);


            if (Mathf.Abs(transform.position.x - jumpBackPosition.x) < 0.5f && rb.linearVelocity.y <= 0)
            {

                rb.linearVelocity = new Vector2(rb.linearVelocity.x * 0.5f, rb.linearVelocity.y);


                if (Mathf.Abs(transform.position.y - startPosition.y) < 0.1f)
                {


                    break;
                }
            }


            if ((directionToPlayer > 0 && transform.position.x < jumpBackPosition.x) ||
                (directionToPlayer < 0 && transform.position.x > jumpBackPosition.x))
            {


                break;
            }

            yield return null;
        }


        rb.linearVelocity = Vector2.zero;


        bool shouldFacePlayer = playerPosition.x > transform.position.x;
        if (shouldFacePlayer != facingRight)
        {
            Flip();
        }



    }

    void DealDamage()
    {
        if (isDead) return;




        Collider2D[] hitPlayers = Physics2D.OverlapCircleAll(attackPoint.position, attackRadius, 1 << LayerMask.NameToLayer("Player"));
        foreach (Collider2D playerCollider in hitPlayers)
        {
            PlayerHealth playerHealth = playerCollider.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(attackDamage);


            }
        }
    }

    public void AnimationEvent_DealDamage()
    {




    }

    public void AnimationEvent_AttackEnd()
    {


    }

    void UpdateAnimator()
    {

        bool isMoving = (currentState == EnemyState.Patrol ||
                         currentState == EnemyState.Chase ||
                         currentState == EnemyState.Retreat ||
                         currentState == EnemyState.JumpBack) &&
                         Mathf.Abs(rb.linearVelocity.x) > 0.1f;


        bool isRunning = currentState == EnemyState.Chase && Mathf.Abs(rb.linearVelocity.x) > 0.1f;


        bool inAttackState = currentState == EnemyState.Attack ||
                             currentState == EnemyState.FireAttack ||
                             currentState == EnemyState.RockAttack;


        animator.SetBool(hashIsWalking, isMoving && !isRunning && !isHurt && !isDead && !inAttackState);
        animator.SetBool(hashIsRunning, isRunning && !isHurt && !isDead && !inAttackState);
    }

    void UpdateFacingDirectionTowards(Vector3 targetPosition)
    {
        if (isHurt || isDead) return;


        if (currentState == EnemyState.Retreat || currentState == EnemyState.JumpBack) return;

        if (targetPosition.x > transform.position.x && !facingRight)
        {
            Flip();
        }
        else if (targetPosition.x < transform.position.x && facingRight)
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


        Gizmos.color = new Color(1f, 0.5f, 0f, 0.7f);
        Gizmos.DrawWireSphere(transform.position, fireAttackRange);

        if (attackPoint != null)
        {
            Gizmos.color = Color.magenta;
            Gizmos.DrawWireSphere(attackPoint.position, attackRadius);
        }


        if (firePoint != null)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(firePoint.position, 0.1f);


            Gizmos.DrawRay(firePoint.position, (facingRight ? Vector3.right : Vector3.left) * 2f);
        }

        if (Application.isPlaying)
        {
            Gizmos.color = Color.blue;
            Vector3 rightEdge = patrolCenter + Vector3.right * patrolRange;
            Vector3 leftEdge = patrolCenter + Vector3.left * patrolRange;
            Gizmos.DrawLine(leftEdge + Vector3.up * 0.2f, rightEdge + Vector3.up * 0.2f);
            Gizmos.DrawSphere(leftEdge, 0.15f);
            Gizmos.DrawSphere(rightEdge, 0.15f);

            if (currentState == EnemyState.Patrol && !isHurt && !isDead)
            {
                Gizmos.color = Color.cyan;
                Gizmos.DrawLine(transform.position, currentPatrolTarget);
            }


            if (player != null)
            {

                float directionToPlayer = Mathf.Sign(player.position.x - transform.position.x);
                Vector3 retreatPos = transform.position - new Vector3(directionToPlayer * initialRetreatDistance, 0, 0);
                Gizmos.color = new Color(1f, 0.5f, 0f, 0.5f);
                Gizmos.DrawSphere(retreatPos, 0.3f);


                Vector3 jumpBackPos = retreatPos - new Vector3(directionToPlayer * furtherJumpBackDistance, 0, 0);
                Gizmos.color = new Color(0.5f, 0f, 0.5f, 0.5f);
                Gizmos.DrawSphere(jumpBackPos, 0.3f);
            }
        }
        else
        {
            Gizmos.color = Color.blue;
            Vector3 currentPos = transform.position;
            Vector3 rightEdge = currentPos + Vector3.right * patrolRange;
            Vector3 leftEdge = currentPos + Vector3.left * patrolRange;
            Gizmos.DrawLine(leftEdge + Vector3.up * 0.2f, rightEdge + Vector3.up * 0.2f);
            Gizmos.DrawSphere(leftEdge, 0.15f);
            Gizmos.DrawSphere(rightEdge, 0.15f);
        }
    }

    IEnumerator PerformFireAttack()
    {

        SetState(EnemyState.FireAttack);
        isAttacking = true;


        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        UpdateFacingDirectionTowards(player.position);


        animator.SetTrigger(hashFireAttack);


        yield return new WaitForSeconds(0.5f);


        yield return new WaitForSeconds(fireAttackDuration - 0.5f);


        lastAttackTime = Time.time;
        lastActionTime = Time.time;
        isAttacking = false;
        useFireAttack = false;


        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        float distanceToPatrolCenter = Vector2.Distance(transform.position, patrolCenter);


        if (distanceToPlayer <= detectionRange && distanceToPatrolCenter <= patrolRange * 1.5f)
        {
            SetState(EnemyState.Chase);
        }
        else
        {
            SetState(EnemyState.Patrol);
            SetPatrolTarget(transform.position.x < patrolCenter.x);
        }


        currentActionCoroutine = null;
    }

    void SpawnFireball()
    {
        if (fireballPrefab != null && firePoint != null)
        {

            GameObject fireball = Instantiate(fireballPrefab, firePoint.position, Quaternion.identity);


            FireballProjectile fireballScript = fireball.GetComponent<FireballProjectile>();
            if (fireballScript != null)
            {

                float direction = facingRight ? 1 : -1;
                fireballScript.Initialize(direction, fireballSpeed, gameObject);



            }
            else
            {

                Rigidbody2D fireballRb = fireball.GetComponent<Rigidbody2D>();
                if (fireballRb != null)
                {
                    float direction = facingRight ? 1 : -1;
                    fireballRb.linearVelocity = new Vector2(direction * fireballSpeed, 0);


                    if (!facingRight)
                    {
                        fireball.transform.localScale = new Vector3(-1, 1, 1);
                    }
                }
            }



        }
        else
        {


        }
    }


    public void AnimationEvent_ShootFire()
    {
        if (isDead) return;


        if (fireSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(fireSound);
        }

        SpawnFireball();
    }

    IEnumerator PerformRockAttack()
    {

        SetState(EnemyState.RockAttack);
        isAttacking = true;
        rockSpawning = false;


        rb.linearVelocity = new Vector2(0, rb.linearVelocity.y);
        UpdateFacingDirectionTowards(player.position);


        animator.SetTrigger(hashRockAttack);


        yield return new WaitForSeconds(0.5f);


        yield return new WaitForSeconds(rockAttackDuration - 0.5f);


        lastAttackTime = Time.time;
        lastActionTime = Time.time;
        isAttacking = false;


        useFireAttack = false;


        float distanceToPlayer = Vector2.Distance(transform.position, player.position);
        float distanceToPatrolCenter = Vector2.Distance(transform.position, patrolCenter);


        if (distanceToPlayer <= detectionRange && distanceToPatrolCenter <= patrolRange * 1.5f)
        {
            SetState(EnemyState.Chase);
        }
        else
        {
            SetState(EnemyState.Patrol);
            SetPatrolTarget(transform.position.x < patrolCenter.x);
        }


        currentActionCoroutine = null;
    }


    public void AnimationEvent_SpawnRock()
    {
        if (isDead || rockSpawning) return;


        if (rockAttackSound != null && audioSource != null)
        {
            audioSource.PlayOneShot(rockAttackSound);
        }


        rockSpawning = true;
        StartCoroutine(SpawnRockAbovePlayer());
    }

    IEnumerator SpawnRockAbovePlayer()
    {
        try
        {

            yield return new WaitForSeconds(0.5f);

            if (rockPrefab != null && player != null)
            {

                Vector3 playerPosition = player.position;


                Vector3[] rockPositions = new Vector3[rockCount];
                GameObject[] warningEffects = new GameObject[rockCount];


                for (int i = 0; i < rockCount; i++)
                {

                    float xOffset = (i - rockCount / 2) * rockSpacing;


                    Vector3 groundPosition = new Vector3(playerPosition.x + xOffset, playerPosition.y, playerPosition.z);


                    rockPositions[i] = groundPosition + Vector3.up * rockSpawnHeight;


                    warningEffects[i] = CreateWarningEffect(groundPosition);
                }


                yield return new WaitForSeconds(rockFallDelay);


                foreach (GameObject warning in warningEffects)
                {
                    if (warning != null)
                    {
                        Destroy(warning);
                    }
                }


                for (int i = 0; i < rockCount; i++)
                {

                    GameObject rock = Instantiate(rockPrefab, rockPositions[i], Quaternion.identity);


                    VienDaRoi rockComponent = rock.GetComponent<VienDaRoi>();
                    if (rockComponent != null)
                    {
                        rockComponent.damage = rockDamage;
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


                        RockDamage rockDamageComponent = rock.AddComponent<RockDamage>();
                        rockDamageComponent.damage = rockDamage;


                        Destroy(rock, 5f);
                    }
                }



            }
        }
        finally
        {

            rockSpawning = false;
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


    public class RockDamage : MonoBehaviour
    {
        public int damage = 15;

        void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
                if (playerHealth != null)
                {
                    playerHealth.TakeDamage(damage);
                }

                Destroy(gameObject);
            }
            else if (collision.gameObject.CompareTag("Ground"))
            {

                Destroy(gameObject, 0.5f);
            }
        }
    }
}