using UnityEngine;

public class PlayerCollision : MonoBehaviour
{
    private Gamemanager gamemanager;

    private void Awake()
    {
        gamemanager = FindAnyObjectByType<Gamemanager>();
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Coin"))
        {
            Destroy(collision.gameObject);
            gamemanager.AddScore(1);
        }
        else if (collision.CompareTag("Trap"))
        {
            gamemanager.GameOver();
        }
        else if (collision.CompareTag("Enemy"))
        {
            gamemanager.GameOver();
        }
    }
}
