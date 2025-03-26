using UnityEngine;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{
    public int maxHealth = 3;
    public int currentHealth;

    public GameObject hitEffect; // Hiệu ứng khi bị đánh
    public GameObject deathEffect; // Hiệu ứng khi chết

    public ItemDrop itemDrop; // Tham chiếu đến script ItemDrop

    void Start()
    {
        currentHealth = maxHealth;
        if (itemDrop == null)
            itemDrop = GetComponent<ItemDrop>();
    }

    public void TakeDamage(int damage)
    {
        currentHealth -= damage;

        // Hiệu ứng khi bị đánh
        if (hitEffect != null)
            Instantiate(hitEffect, transform.position, Quaternion.identity);

        // Hiệu ứng nhấp nháy
        StartCoroutine(FlashEffect());

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    void Die()
    {
        // Hiệu ứng khi chết
        if (deathEffect != null)
            Instantiate(deathEffect, transform.position, Quaternion.identity);

        // Rơi vật phẩm
        if (itemDrop != null)
            itemDrop.DropItem();

        // Hủy đối tượng
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