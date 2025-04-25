using UnityEngine;
using UnityEngine.UI;

public class SoundToggleButton : MonoBehaviour
{
    public Sprite soundOnSprite;
    public Sprite soundOffSprite;
    public Image iconImage;

    void Start()
    {
      
        UpdateIcon();

        
        UpdateAudioState();
    }

    public void OnClickToggleSound()
    {
        
        AudioManager.Instance.ToggleSound();

       
        UpdateIcon();

       
        UpdateAudioState();
    }

    void UpdateIcon()
    {
      
        bool isMuted = AudioManager.Instance.IsMuted();
        iconImage.sprite = isMuted ? soundOffSprite : soundOnSprite;
    }

    void UpdateAudioState()
    {
      
        AudioSource[] audioSources = Object.FindObjectsByType<AudioSource>(FindObjectsSortMode.None);

        foreach (var audioSource in audioSources)
        {
            
            audioSource.mute = AudioManager.Instance.IsMuted();
        }
    }
}
