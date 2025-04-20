using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    public AudioClip backgroundMusic;
    private AudioSource audioSource;

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

        if (FindObjectOfType<AudioListener>() == null)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
            {
                mainCamera.gameObject.AddComponent<AudioListener>();
                Debug.Log("Added AudioListener to Main Camera");
            }
            else
            {
                Debug.LogWarning("No Main Camera found in the scene to add AudioListener!");
            }
        }

        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            Debug.Log("AudioSource added to AudioManager");
        }

        if (backgroundMusic != null)
        {
            audioSource.clip = backgroundMusic;
            audioSource.loop = true;
            audioSource.volume = 0.5f;
            audioSource.Play();
            Debug.Log("Background music started playing: " + backgroundMusic.name);
        }
        else
        {
            Debug.LogWarning("Background music is not assigned in AudioManager!");
        }
    }

    public void SetVolume(float volume)
    {
        audioSource.volume = Mathf.Clamp01(volume);
    }

    public void StopMusic()
    {
        audioSource.Stop();
    }
}