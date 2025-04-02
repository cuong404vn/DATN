using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private float startingHealth;
    public float currentHealth { get; private set; }
    private Animator anim;
    private bool dead;

<<<<<<< HEAD
    public GameObject hitEffect;
    public GameObject deathEffect;

    public ItemDrop itemDrop;
    public Transform exitGateSpawnPoint;

    public string enemyName = "Enemy"; // Tên của quái, có thể được đặt từ Inspector
    public GameObject healthBarPrefab; // Prefab thanh máu
    private GameObject healthBarInstance;
    private Slider healthBarSlider;
    private Text enemyNameText;
=======
    [Header("iFrames")]
    [SerializeField] private float iFramesDuration;
    [SerializeField] private int numberOfFlashes;
    private SpriteRenderer spriteRend;

    [Header("Components")]
    [SerializeField] private Behaviour[] components;
    private bool invulnerable;
>>>>>>> 80917b7e59e816d461dd016f1335dd2987c52030

    private void Awake()
    {
<<<<<<< HEAD
        currentHealth = maxHealth;
        if (itemDrop == null)
            itemDrop = GetComponent<ItemDrop>();

        if (exitGateSpawnPoint == null && isBoss)
            exitGateSpawnPoint = transform;

        // Tạo thanh máu
        if (healthBarPrefab != null)
        {
            healthBarInstance = Instantiate(healthBarPrefab, transform.position + new Vector3(0, 1.5f, 0), Quaternion.identity, transform);
            healthBarSlider = healthBarInstance.GetComponentInChildren<Slider>();
            enemyNameText = healthBarInstance.GetComponentInChildren<Text>();

            if (healthBarSlider != null)
                healthBarSlider.maxValue = maxHealth;
            healthBarSlider.value = currentHealth;

            if (enemyNameText != null)
                enemyNameText.text = enemyName;
        }
=======
        currentHealth = startingHealth;
        anim = GetComponent<Animator>();
        spriteRend = GetComponent<SpriteRenderer>();
>>>>>>> 80917b7e59e816d461dd016f1335dd2987c52030
    }
    public void TakeDamage(float _damage)
    {
        if (invulnerable) return;
        currentHealth = Mathf.Clamp(currentHealth - _damage, 0, startingHealth);

<<<<<<< HEAD
        if (hitEffect != null)
            Instantiate(hitEffect, transform.position, Quaternion.identity);

        StartCoroutine(FlashEffect());

        // Cập nhật thanh máu
        if (healthBarSlider != null)
            healthBarSlider.value = currentHealth;

        if (currentHealth <= 0)
=======
        if (currentHealth > 0)
>>>>>>> 80917b7e59e816d461dd016f1335dd2987c52030
        {
            anim.SetTrigger("hurt");
            StartCoroutine(Invunerability());
        }
<<<<<<< HEAD
    }

    void Die()
    {
        if (deathEffect != null)
            Instantiate(deathEffect, transform.position, Quaternion.identity);

        if (GameManager.Instance != null)
        {
            GameManager.Instance.NotifyEnemyDefeated();
        }

        if (isBoss)
        {
            LevelManager levelManager = FindAnyObjectByType<LevelManager>();
            if (levelManager != null)
=======
        else
        {
            if (!dead)
>>>>>>> 80917b7e59e816d461dd016f1335dd2987c52030
            {
                anim.SetTrigger("die");

                //Deactivate all attached component classes
                foreach (Behaviour component in components)
                    component.enabled = false;

                dead = true;
            }
        }
<<<<<<< HEAD

        if (itemDrop != null)
            itemDrop.DropItem();

        Destroy(healthBarInstance);
        Destroy(gameObject);
=======
>>>>>>> 80917b7e59e816d461dd016f1335dd2987c52030
    }
    public void AddHealth(float _value)
    {
        currentHealth = Mathf.Clamp(currentHealth + _value, 0, startingHealth);
    }
    private IEnumerator Invunerability()
    {
        invulnerable = true;
        Physics2D.IgnoreLayerCollision(10, 11, true);
        for (int i = 0; i < numberOfFlashes; i++)
        {
            spriteRend.color = new Color(1, 0, 0, 0.5f);
            yield return new WaitForSeconds(iFramesDuration / (numberOfFlashes * 2));
            spriteRend.color = Color.white;
            yield return new WaitForSeconds(iFramesDuration / (numberOfFlashes * 2));
        }
        Physics2D.IgnoreLayerCollision(10, 11, false);
        invulnerable = false;
    }
}
