using UnityEngine;
using UnityEngine.SceneManagement;

public class TimeScaleResetter : MonoBehaviour
{
    void Awake()
    {
        
        Time.timeScale = 1f;
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        
        Time.timeScale = 1f;
    }
}