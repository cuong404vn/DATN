using UnityEngine;

public class PlayerCollision : MonoBehaviour
{


    private void OnTriggerEnter2D(Collider2D collision)
    {
      
        GameManager gameManager = GameManager.Instance;

   
        if (gameManager == null)
        {
           
            return;
        }

        if (collision.CompareTag("Coin"))
        {
            Destroy(collision.gameObject);
            gameManager.AddCoins(1);
           
        }
        else if (collision.CompareTag("Trap"))
        {
            
        }
        else if (collision.CompareTag("Enemy"))
        {
           
        }
        else if (collision.CompareTag("Boss"))
        {
            
        }
    }
}
