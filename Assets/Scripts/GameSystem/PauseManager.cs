using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class PauseManager : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject pauseMenuUI;
    public Button pauseButton;
    public Button resumeButton;
    public Button restartButton;
    public Button homeButton;

    private bool isPaused = false;

    void Start()
    {

        if (pauseMenuUI != null)
            pauseMenuUI.SetActive(false);


        if (pauseButton != null)
            pauseButton.onClick.AddListener(TogglePause);

        if (resumeButton != null)
            resumeButton.onClick.AddListener(ResumeGame);

        if (restartButton != null)
            restartButton.onClick.AddListener(RestartGame);

        if (homeButton != null)
            homeButton.onClick.AddListener(ReturnToHome);
    }

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePause();
        }
    }

    public void TogglePause()
    {
        isPaused = !isPaused;
        pauseMenuUI.SetActive(isPaused);


        Time.timeScale = isPaused ? 0f : 1f;
    }

    public void ResumeGame()
    {

        isPaused = false;
        pauseMenuUI.SetActive(false);
        Time.timeScale = 1f;
    }

    public void RestartGame()
    {

        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void ReturnToHome()
    {

        Time.timeScale = 1f;
        SceneManager.LoadScene("Home");
    }
}
