using UnityEngine;

public class GameAudioController : MonoBehaviour
{
    public static GameAudioController Instance { get; private set; }

    [Header("Audio Settings")]
    public AudioClip backgroundMusic;
    private AudioSource audioSource;

    void Awake()
    {
        // Singleton để tránh trùng lặp
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Giữ qua các scene
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Thêm AudioSource nếu chưa có
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();

        // Thêm AudioListener nếu chưa có
        if (FindAnyObjectByType<AudioListener>() == null && Camera.main != null)
            Camera.main.gameObject.AddComponent<AudioListener>();

        // Cài đặt và phát nhạc nền
        if (backgroundMusic != null)
        {
            audioSource.clip = backgroundMusic;
            audioSource.loop = true;
            audioSource.volume = 0.5f;
            audioSource.Play();
        }
        else
        {
            Debug.LogWarning("No background music assigned!");
        }
    }

    public void SetVolume(float volume)
    {
        audioSource.volume = Mathf.Clamp01(volume);
    }

    public void StopMusic()
    {
        if (audioSource.isPlaying)
            audioSource.Stop();
    }

    public void PlayNewMusic(AudioClip newClip)
    {
        if (newClip != null)
        {
            audioSource.Stop();
            audioSource.clip = newClip;
            audioSource.Play();
        }
    }
}
