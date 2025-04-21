using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerController : MonoBehaviour
{
    public float speed = 5f;
    public float jumpForce = 7f;
    private Rigidbody2D rb;
    private bool isGrounded;
    private int jumpCount;
    public int maxJumps = 2;
    private SpriteRenderer spriteRenderer;
    private Animator animator;
    private bool isAttacking;
    private Vector3 startPosition;

    public float attackDuration = 0.5f;

    public AudioClip idleSound;
    public AudioClip runSound;
    public AudioClip attackSound;
    public AudioClip jump1Sound;
    public AudioClip jump2Sound;
    public AudioClip landingSound;

    private AudioSource audioSource;
    private bool isPlayingRunSound = false;

    private static PlayerController instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        startPosition = transform.position;
    }

    void Update()
    {
        CheckGroundedState();
        Move();
        Jump();
        Attack();
        UpdateAnimation();
        UpdateSounds();
    }

    void Move()
    {
        if (isAttacking) return;

        float moveInput = Input.GetAxis("Horizontal");
        rb.linearVelocity = new Vector2(moveInput * speed, rb.linearVelocity.y);

        if (moveInput > 0)
            spriteRenderer.flipX = false;
        else if (moveInput < 0)
            spriteRenderer.flipX = true;

        animator.SetBool("isRunning", moveInput != 0);
    }

    void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (isGrounded && jumpCount >= maxJumps)
            {
                jumpCount = 0;
            }

            if (jumpCount < maxJumps)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
                jumpCount++;

                if (jumpCount == 1)
                {
                    animator.SetTrigger("Jump1");
                    PlaySound(jump1Sound);
                }
                else if (jumpCount == 2)
                {
                    animator.SetTrigger("Jump2");
                    PlaySound(jump2Sound);
                }
            }
        }
    }

    void Attack()
    {
        if (Input.GetMouseButtonDown(0) && !isAttacking)
        {
            isAttacking = true;
            animator.SetBool("isAttacking", true);
            PlaySound(attackSound);

            CancelInvoke("ResetAttack");
            Invoke("ResetAttack", attackDuration);
        }
    }

    void ResetAttack()
    {
        isAttacking = false;
        animator.SetBool("isAttacking", false);
    }

    void UpdateAnimation()
    {
        if (isGrounded)
        {
            animator.SetBool("isIdle", rb.linearVelocity.x == 0);
        }
    }

    void UpdateSounds()
    {
        if (animator.GetBool("isRunning") && isGrounded && !isAttacking)
        {
            if (!isPlayingRunSound && runSound != null)
            {
                audioSource.clip = runSound;
                audioSource.loop = true;
                audioSource.Play();
                isPlayingRunSound = true;
            }
        }
        else
        {
            if (isPlayingRunSound)
            {
                audioSource.loop = false;
                if (audioSource.clip == runSound)
                {
                    audioSource.Stop();
                }
                isPlayingRunSound = false;
            }

            if (animator.GetBool("isIdle") && isGrounded && !isAttacking && !audioSource.isPlaying)
            {
                PlayIdleSound();
            }
        }
    }

    void PlayIdleSound()
    {
        if (idleSound != null && !audioSource.isPlaying)
        {
            audioSource.clip = idleSound;
            audioSource.loop = true;
            audioSource.Play();
        }
    }

    void PlaySound(AudioClip clip)
    {
        if (clip != null)
        {
            audioSource.Stop();
            audioSource.loop = false;
            audioSource.clip = clip;
            audioSource.Play();
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            CheckGroundContact(collision);
        }
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            CheckGroundContact(collision);
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            Collider2D[] grounds = Physics2D.OverlapCircleAll(transform.position, 0.2f);
            bool stillOnGround = false;

            foreach (Collider2D ground in grounds)
            {
                if (ground.CompareTag("Ground") && ground.gameObject != collision.gameObject)
                {
                    stillOnGround = true;
                    break;
                }
            }

            if (!stillOnGround)
            {
                isGrounded = false;
                animator.SetBool("isGrounded", false);
            }
        }
    }

    void CheckGroundContact(Collision2D collision)
    {
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y > 0.3f)
            {
                if (!isGrounded)
                {
                    isGrounded = true;
                    jumpCount = 0;
                    animator.SetBool("isGrounded", true);

                    if (landingSound != null)
                    {
                        audioSource.PlayOneShot(landingSound);
                    }
                }
                return;
            }
        }
    }

    void CheckGroundedState()
    {
        Vector2 center = transform.position - new Vector3(0, GetComponent<Collider2D>().bounds.extents.y - 0.05f, 0);
        Vector2 left = center - new Vector2(GetComponent<Collider2D>().bounds.extents.x * 0.8f, 0);
        Vector2 right = center + new Vector2(GetComponent<Collider2D>().bounds.extents.x * 0.8f, 0);

        float rayLength = 0.15f;

        RaycastHit2D hitCenter = Physics2D.Raycast(center, Vector2.down, rayLength, LayerMask.GetMask("Ground"));
        RaycastHit2D hitLeft = Physics2D.Raycast(left, Vector2.down, rayLength, LayerMask.GetMask("Ground"));
        RaycastHit2D hitRight = Physics2D.Raycast(right, Vector2.down, rayLength, LayerMask.GetMask("Ground"));



        if ((hitCenter && hitCenter.collider.CompareTag("Ground")) ||
            (hitLeft && hitLeft.collider.CompareTag("Ground")) ||
            (hitRight && hitRight.collider.CompareTag("Ground")))
        {
            if (!isGrounded)
            {
                isGrounded = true;
                jumpCount = 0;
                animator.SetBool("isGrounded", true);
            }
        }
        else
        {
            if (rb.linearVelocity.y < -0.1f && isGrounded && !IsOnAnyGround())
            {
                isGrounded = false;
                animator.SetBool("isGrounded", false);
            }
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
        }
    }

    bool IsOnAnyGround()
    {
        Bounds bounds = GetComponent<Collider2D>().bounds;
        Vector2 boxCenter = new Vector2(bounds.center.x, bounds.min.y + 0.1f);
        Vector2 boxSize = new Vector2(bounds.size.x * 0.9f, 0.1f);

        Collider2D[] colliders = Physics2D.OverlapBoxAll(boxCenter, boxSize, 0f);
        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag("Ground") && collider.gameObject != gameObject)
            {
                return true;
            }
        }

        return false;
    }

    public void ChangeScene(string sceneName)
    {
        SceneManager.LoadScene(sceneName);
    }

    public void ResetState()
    {
        rb.linearVelocity = Vector2.zero;
        rb.angularVelocity = 0f;

        isGrounded = true;
        jumpCount = 0;
        isAttacking = false;

        animator.SetBool("isRunning", false);
        animator.SetBool("isGrounded", true);
        animator.SetBool("isIdle", true);
        animator.SetBool("isAttacking", false);

        animator.ResetTrigger("Jump1");
        animator.ResetTrigger("Jump2");

        animator.Play("Player_Idle");

        transform.position = startPosition;

        audioSource.Stop();
        isPlayingRunSound = false;
    }
}
