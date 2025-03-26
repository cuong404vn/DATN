using UnityEngine;
using UnityEngine.SceneManagement;

public class LavaZone : MonoBehaviour
{
    public GameObject GameOver; 

    void Start()
    {
        GameOver.SetActive(false); 
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            ShowGameOver();
        }
    }

    void ShowGameOver()
    {
        GameOver.SetActive(true); 
        Time.timeScale = 0; 
    }

    public void RestartGame()
    {
        Time.timeScale = 1; 
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}
