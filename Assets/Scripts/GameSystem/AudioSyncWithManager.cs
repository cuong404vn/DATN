using UnityEngine;

public class AudioSyncWithManager : MonoBehaviour
{
    void Start()
    {
        if (AudioManager.Instance != null)
        {
            AudioListener audioListener = GetComponent<AudioListener>();
            if (audioListener != null)
            {
                bool isMuted = AudioManager.Instance.IsMuted();
                audioListener.enabled = !isMuted;
                Debug.Log("AudioListener set to " + (!isMuted));
            }
            else
            {
                Debug.LogWarning("No AudioListener found on Main Camera.");
            }
        }
        else
        {
            Debug.LogWarning("AudioManager.Instance is null");
        }
    }
}
