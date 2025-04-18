using UnityEngine;

public class Collectible : MonoBehaviour
{
    public enum CollectibleType { Health, Coin, Key, HealthPotion }

    public CollectibleType type;
    public int value = 1;

    public float moveSpeed = 1f;
    public float moveHeight = 0.5f;
    public float rotateSpeed = 100f;

    private Vector3 startPosition;

    void Start()
    {
        startPosition = transform.position;
    }

    void Update()
    {

        transform.position = startPosition + new Vector3(0, Mathf.Sin(Time.time * moveSpeed) * moveHeight, 0);


        transform.Rotate(Vector3.up, rotateSpeed * Time.deltaTime);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {

            switch (type)
            {
                case CollectibleType.Health:
                    PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
                    if (playerHealth != null)
                        playerHealth.AddHealth(value);
                    break;

                case CollectibleType.Coin:

                    GameManager.Instance.AddCoins(value);
                    break;

                case CollectibleType.Key:

                    GameManager.Instance.AddKeys(value);
                    break;

                case CollectibleType.HealthPotion:
                    PlayerHealth playerHealthPotion = other.GetComponent<PlayerHealth>();
                    if (playerHealthPotion != null)
                        playerHealthPotion.AddHealthPotion(value);
                    break;
            }


            Destroy(gameObject);
        }
    }
}