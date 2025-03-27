using UnityEngine;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 3;
    public int currentHealth;
    public bool isBoss = false;

    public GameObject hitEffect; 
    public GameObject deathEffect; 

    public ItemDrop itemDrop; 
    public Transform exitGateSpawnPoint; 

    void Start()
    {
        currentHealth = maxHealth;
        if (itemDrop == null)
            itemDrop = GetComponent<ItemDrop>();

        
        if (exitGateSpawnPoint == null && isBoss)
            exitGateSpawnPoint = transform;
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        
        if (hitEffect != null)
            Instantiate(hitEffect, transform.position, Quaternion.identity);

        
        StartCoroutine(FlashEffect());

        if (currentHealth <= 0)
        {
            Die();
        }
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
            {
                Vector3 spawnPosition = exitGateSpawnPoint != null ?
                    exitGateSpawnPoint.position : transform.position;
                levelManager.ShowExitGate(spawnPosition);
            }
        }

       
        if (itemDrop != null)
            itemDrop.DropItem();

   
        Destroy(gameObject);
    }

    IEnumerator FlashEffect()
    {
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        Color originalColor = sprite.color;
        sprite.color = Color.red;
        yield return new WaitForSeconds(0.1f);
        sprite.color = originalColor;
    }
}