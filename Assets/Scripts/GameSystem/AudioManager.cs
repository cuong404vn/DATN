using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    private bool isMuted;
    private string currentSceneName;

    void Awake()
    {


        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

        }
        else
        {

            Destroy(gameObject);
            return;
        }


        isMuted = PlayerPrefs.GetInt("AudioMuted", 0) == 1;



        currentSceneName = SceneManager.GetActiveScene().name;



        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        currentSceneName = scene.name;



        Invoke("UpdateAllAudioStates", 0.1f);
    }

    public void ToggleSound()
    {
        isMuted = !isMuted;
        PlayerPrefs.SetInt("AudioMuted", isMuted ? 1 : 0);
        PlayerPrefs.Save();



        UpdateAllAudioStates();
    }

    public bool IsMuted()
    {
        return isMuted;
    }


    public void UpdateAllAudioStates()
    {



        AudioSource[] audioSources = FindObjectsOfType<AudioSource>();


        foreach (var audioSource in audioSources)
        {
            audioSource.mute = isMuted;

        }


        ManageAudioListeners();
    }


    private void ManageAudioListeners()
    {
        AudioListener[] listeners = FindObjectsOfType<AudioListener>();


        if (listeners.Length == 0)
        {

            return;
        }


        bool foundActiveListener = false;
        foreach (var listener in listeners)
        {
            if (!foundActiveListener)
            {
                listener.enabled = true;
                foundActiveListener = true;

            }
            else
            {
                listener.enabled = false;

            }
        }
    }

    private void OnDestroy()
    {



        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
