using UnityEngine;

public class PlayerCollision : MonoBehaviour
{


    private void OnTriggerEnter2D(Collider2D collision)
    {
      
        GameManager gameManager = GameManager.Instance;

    
        if (gameManager == null)
        {
            Debug.LogError("GameManager not found in OnTriggerEnter2D!");
            return;
        }

        if (collision.CompareTag("Coin"))
        {
            Destroy(collision.gameObject);
            gameManager.AddCoins(1);
            Debug.Log("Coin collected! Total coins: " + gameManager.coins);
        }
        else if (collision.CompareTag("Trap"))
        {
          
        }
        else if (collision.CompareTag("Enemy"))
        {
           
        }
    }
}
