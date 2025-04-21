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
        if (Input.GetKeyDown(KeyCode.Space) && jumpCount < maxJumps)
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
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y > 0.5f) // Chỉ tính là mặt đất khi va chạm từ phía dưới
                {
                    isGrounded = true;
                    jumpCount = 0;
                    animator.SetBool("isGrounded", true);

                    if (landingSound != null)
                    {
                        audioSource.PlayOneShot(landingSound);
                    }

                    break;
                }
            }
        }
    }

    void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
            animator.SetBool("isGrounded", false);
        }
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
