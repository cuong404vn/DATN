using UnityEngine;

public class RockDamage : MonoBehaviour
{
    public int damage = 15;
    public GameObject impactEffectPrefab;
    public AudioClip impactSound;

    private bool hasHitSomething = false;

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (hasHitSomething) return;
        hasHitSomething = true;


        if (collision.gameObject.CompareTag("Player"))
        {
            PlayerHealth playerHealth = collision.gameObject.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {
                playerHealth.TakeDamage(damage);
                Debug.Log($"Rock hit player for {damage} damage!");
            }
        }


        if (impactEffectPrefab != null)
        {
            Instantiate(impactEffectPrefab, transform.position, Quaternion.identity);
        }


        if (impactSound != null)
        {
            AudioSource.PlayClipAtPoint(impactSound, transform.position);
        }


        enabled = false;
    }
}