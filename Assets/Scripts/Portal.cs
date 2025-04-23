using UnityEngine;
using UnityEngine.SceneManagement;

public class Portal : MonoBehaviour
{
    public string map2boos;




    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {




            PlayerHealth playerHealth = other.GetComponent<PlayerHealth>();
            if (playerHealth != null)
            {




                PlayerPrefs.SetInt("PlayerCurrentHealth", playerHealth.currentHealth);
                PlayerPrefs.SetInt("PlayerCurrentPotions", playerHealth.currentPotions);
                PlayerPrefs.SetInt("IsTransitioningScene", 1);


                if (GameManager.Instance != null)
                {
                    PlayerPrefs.SetInt("PlayerCoins", GameManager.Instance.coins);
                    PlayerPrefs.SetInt("PlayerKeys", GameManager.Instance.keys);



                }


                LevelManager levelManager = FindAnyObjectByType<LevelManager>();
                if (levelManager != null)
                {
                    levelManager.SaveLevelState();


                }
                else
                {

                }

                PlayerPrefs.Save();


            }
            else
            {

            }




            SceneManager.LoadScene(map2boos);
        }
    }
}
